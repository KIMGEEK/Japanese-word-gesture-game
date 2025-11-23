import cv2
import mediapipe as mp
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Depends, HTTPException
from sqlalchemy import create_engine, Column, Integer, String
from sqlalchemy.orm import sessionmaker, Session, declarative_base
from pydantic import BaseModel
from typing import List
import asyncio

# ==========================================
# 1. 데이터베이스 설정 (SQLite)
# ==========================================
SQLALCHEMY_DATABASE_URL = "sqlite:///./vocab.db"

# check_same_thread=False: SQLite를 멀티 스레드 환경(FastAPI)에서 쓸 때 필요
engine = create_engine(
    SQLALCHEMY_DATABASE_URL, connect_args={"check_same_thread": False}
)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# 데이터베이스 테이블 생성 (앱 시작 시 자동 생성)
Base = declarative_base()

# --- DB 모델 정의 ---
class Word(Base):
    __tablename__ = "words"

    id = Column(Integer, primary_key=True, index=True)
    japanese = Column(String, index=True)  # DB 타입: String
    korean = Column(String)                # DB 타입: String
    level = Column(Integer, index=True)    # DB 타입: Integer

# 데이터베이스 테이블 생성 (앱 시작 시 자동 생성)
Base.metadata.create_all(bind=engine)

# --- Pydantic 스키마 ---
class WordCreate(BaseModel):
    japanese: str  
    korean: str    
    level: int

class WordResponse(WordCreate):
    id: int
    class Config:
        from_attributes = True

# --- DB 세션 의존성 함수 ---
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

app = FastAPI()

# --- MediaPipe 설정 ---
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

# Hands 모델 초기화 (최소 감지 신뢰도 0.5, 최소 추적 신뢰도 0.5)
hands = mp_hands.Hands(
    max_num_hands=1,                # 손은 하나만 인식 (필요 시 변경)
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

# [POST] 단어 추가
@app.post("/words/", response_model=WordResponse)
def create_word(word: WordCreate, db: Session = Depends(get_db)):
    # Pydantic 모델(word)의 데이터를 DB 모델(Word)로 변환
    db_word = Word(japanese=word.japanese, korean=word.korean, level=word.level)
    db.add(db_word)
    db.commit()
    db.refresh(db_word)
    return db_word

# [GET] 특정 레벨의 단어들만 싹 긁어오는 API
@app.get("/words/level/{target_level}", response_model=List[WordResponse])
def get_words_by_level(target_level: int, db: Session = Depends(get_db)):
    # DB에서 level 컬럼이 target_level과 같은 것만 필터링
    words = db.query(Word).filter(Word.level == target_level).all()
    
    if not words:
        # (선택사항) 해당 레벨 단어가 하나도 없으면 빈 리스트 반환
        return []
        
    return words

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    print("Client Connected")

    # 웹캠 캡처 시작 (0: 기본 카메라)
    cap = cv2.VideoCapture(0)

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                break

            # 1. 이미지 전처리
            # 웹캠 좌우 반전
            frame = cv2.flip(frame, 1)
            
            # MediaPipe는 RGB 이미지를 사용, BGR -> RGB 변환
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            
            # 2. 손 인식 수행
            results = hands.process(rgb_frame)

            data_to_send = {}

            # 3. 랜드마크 추출 및 시각화
            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    # 화면에 손 뼈대 그리기 (디버깅용)
                    mp_drawing.draw_landmarks(
                        frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                    # 검지 손가락 끝(Index Finger Tip) 좌표 추출
                    # 랜드마크 인덱스 8번
                    index_finger_tip = hand_landmarks.landmark[8]
                    
                    # x, y는 0.0 ~ 1.0 사이의 정규화된 값
                    # (0,0: 화면 왼쪽 위, 1,1: 화면 오른쪽 아래)
                    data_to_send = {
                        "x": index_finger_tip.x,
                        "y": index_finger_tip.y
                    }
            
            # 4. 데이터 전송 (손이 감지되었을 때만)
            if data_to_send:
                # 유니티로 JSON 데이터 전송
                await websocket.send_json(data_to_send)
            
            # 5. 디버깅 화면 출력 (서버 측에서 확인용)
            cv2.imshow('MediaPipe Hands Server View', frame)
            
            # 'q' 키를 누르면 종료
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
            
            # 비동기 루프의 원활한 처리를 위해 아주 짧은 대기
            await asyncio.sleep(0.01)

    except WebSocketDisconnect:
        print("Client Disconnected")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        # 자원 해제
        cap.release()
        cv2.destroyAllWindows()
        await websocket.close()

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)