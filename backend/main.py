import cv2
import json
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Depends
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
    japanese = Column(String, index=True)
    korean = Column(String)
    level = Column(Integer, index=True)

    # ✅ 추가: 후보군(히라가나 5글자 배열을 JSON 문자열로 저장)
    candidates = Column(String)  # 예: '["り","ん","ご","み","さ"]'

# 데이터베이스 테이블 생성 (앱 시작 시 자동 생성)
Base.metadata.create_all(bind=engine)

# --- Pydantic 스키마 ---
class WordCreate(BaseModel):
    japanese: str
    korean: str
    level: int
    candidates: List[str]

class WordResponse(BaseModel):
    id: int
    japanese: str
    korean: str
    level: int
    candidates: List[str]

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

# [POST] 단어 추가
@app.post("/words/", response_model=WordResponse)
def create_word(word: WordCreate, db: Session = Depends(get_db)):
    db_word = Word(
        japanese=word.japanese,
        korean=word.korean,
        level=word.level,
        candidates=json.dumps(word.candidates, ensure_ascii=False)  # 저장
    )
    db.add(db_word)
    db.commit()
    db.refresh(db_word)

    # 응답은 배열로
    return WordResponse(
        id=db_word.id,
        japanese=db_word.japanese,
        korean=db_word.korean,
        level=db_word.level,
        candidates=word.candidates
    )

# [GET] 특정 레벨의 단어들만 싹 긁어오는 API
@app.get("/words/level/{level}", response_model=list[WordResponse])
def get_words_by_level(level: int, db: Session = Depends(get_db)):
    rows = db.query(Word).filter(Word.level == level).all()

    result = []
    for w in rows:
        try:
            cand = json.loads(w.candidates) if w.candidates else []
        except Exception:
            cand = []

        result.append(WordResponse(
            id=w.id,
            japanese=w.japanese,
            korean=w.korean,
            level=w.level,
            candidates=cand
        ))
    return result



@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    print("Client Connected")

    # ✅ 여기서 MediaPipe를 지연 import / 지연 초기화
    try:
        import mediapipe as mp
        mp_hands = mp.solutions.hands
        mp_drawing = mp.solutions.drawing_utils
        hands = mp_hands.Hands(
            max_num_hands=1,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5
        )
    except Exception as e:
        # mediapipe/protobuf 충돌이 나도 서버 전체가 죽지 않게 처리
        print(f"[MediaPipe Init Error] {e}")
        await websocket.send_json({"error": "mediapipe_init_failed", "detail": str(e)})
        await websocket.close()
        return

    cap = cv2.VideoCapture(0)

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                break

            frame = cv2.flip(frame, 1)
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            results = hands.process(rgb_frame)

            data_to_send = {}
            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
                    index_finger_tip = hand_landmarks.landmark[8]
                    data_to_send = {"x": index_finger_tip.x, "y": index_finger_tip.y}

            if data_to_send:
                await websocket.send_json(data_to_send)

            cv2.imshow('MediaPipe Hands Server View', frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

            await asyncio.sleep(0.01)

    except WebSocketDisconnect:
        print("Client Disconnected")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        cap.release()
        cv2.destroyAllWindows()
        await websocket.close()


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)