import os
import platform
import cv2
import json
import asyncio
from typing import List, Optional

from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Depends
from sqlalchemy import create_engine, Column, Integer, String
from sqlalchemy.orm import sessionmaker, Session, declarative_base
from pydantic import BaseModel

# ==========================================
# 0. 옵션 (환경변수로 제어)
# ==========================================
# 디버그 프리뷰 창 띄우기 (0/1)
SHOW_PREVIEW = os.getenv("SHOW_PREVIEW", "0") == "1"
# 랜드마크 선 그리기 (0/1) - SHOW_PREVIEW가 켜져있을 때만 의미 있음
DRAW_LANDMARKS = os.getenv("DRAW_LANDMARKS", "0") == "1"
# 카메라 해상도/FPS (원하는 값으로 수정 가능)
CAM_WIDTH = int(os.getenv("CAM_WIDTH", "640"))
CAM_HEIGHT = int(os.getenv("CAM_HEIGHT", "480"))
CAM_FPS = int(os.getenv("CAM_FPS", "30"))

# ==========================================
# 1. 데이터베이스 설정 (SQLite)
# ==========================================
SQLALCHEMY_DATABASE_URL = "sqlite:///./vocab.db"

engine = create_engine(
    SQLALCHEMY_DATABASE_URL, connect_args={"check_same_thread": False}
)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


class Word(Base):
    __tablename__ = "words"

    id = Column(Integer, primary_key=True, index=True)
    japanese = Column(String, index=True)
    korean = Column(String)
    level = Column(Integer, index=True)
    candidates = Column(String)  # 예: '["り","ん","ご","み","さ"]'


Base.metadata.create_all(bind=engine)


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


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


# ==========================================
# 2. MediaPipe 글로벌 초기화 (1순위: 프리웜)
# ==========================================
mp = None
mp_hands = None
mp_drawing = None
hands = None

# 동시에 여러 클라이언트가 붙는 상황을 방지/정리하기 위한 락
hands_lock = asyncio.Lock()
active_ws_lock = asyncio.Lock()
active_ws: Optional[WebSocket] = None


def _open_camera() -> cv2.VideoCapture:
    """2순위: Windows에서 CAP_DSHOW로 카메라 오픈 + 기본 세팅."""
    is_windows = platform.system().lower().startswith("win")
    if is_windows:
        cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
    else:
        cap = cv2.VideoCapture(0)

    # 가능한 경우 캡처 파이프라인 안정화
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAM_WIDTH)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAM_HEIGHT)
    cap.set(cv2.CAP_PROP_FPS, CAM_FPS)
    return cap


def _safe_close_preview():
    if SHOW_PREVIEW:
        try:
            cv2.destroyAllWindows()
        except Exception:
            pass


app = FastAPI()


@app.on_event("startup")
def startup_prewarm():
    """1순위: 서버 시작 시 MediaPipe import + Hands 생성(프리웜)"""
    global mp, mp_hands, mp_drawing, hands

    try:
        import mediapipe as _mp

        mp = _mp
        mp_hands = mp.solutions.hands
        mp_drawing = mp.solutions.drawing_utils

        # Hands는 생성 비용이 커서 "서버 시작 시" 1회만 만들어 재사용
        hands = mp_hands.Hands(
            max_num_hands=1,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5,
        )

        # (선택) 첫 process() 워밍업: 아주 작은 더미 프레임 1회 처리
        # -> 첫 연결 체감 대기 감소 목적
        import numpy as np

        dummy = np.zeros((CAM_HEIGHT, CAM_WIDTH, 3), dtype=np.uint8)
        hands.process(dummy)

        print("[Startup] MediaPipe Hands prewarmed.")
    except Exception as e:
        # 서버 전체를 죽이지 않고, 웹소켓 연결 시 에러로 안내
        hands = None
        print(f"[Startup] MediaPipe prewarm failed: {e}")


@app.on_event("shutdown")
def shutdown_cleanup():
    global hands
    try:
        if hands is not None:
            hands.close()
    except Exception:
        pass
    _safe_close_preview()


# ==========================================
# 3. 단어 API
# ==========================================
@app.post("/words/", response_model=WordResponse)
def create_word(word: WordCreate, db: Session = Depends(get_db)):
    db_word = Word(
        japanese=word.japanese,
        korean=word.korean,
        level=word.level,
        candidates=json.dumps(word.candidates, ensure_ascii=False),
    )
    db.add(db_word)
    db.commit()
    db.refresh(db_word)

    return WordResponse(
        id=db_word.id,
        japanese=db_word.japanese,
        korean=db_word.korean,
        level=db_word.level,
        candidates=word.candidates,
    )


@app.get("/words/level/{level}", response_model=list[WordResponse])
def get_words_by_level(level: int, db: Session = Depends(get_db)):
    rows = db.query(Word).filter(Word.level == level).all()
    result = []
    for w in rows:
        try:
            cand = json.loads(w.candidates) if w.candidates else []
        except Exception:
            cand = []
        result.append(
            WordResponse(
                id=w.id,
                japanese=w.japanese,
                korean=w.korean,
                level=w.level,
                candidates=cand,
            )
        )
    return result


# ==========================================
# 4. WebSocket: 손가락 좌표 스트리밍
# ==========================================
@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    global active_ws

    await websocket.accept()
    print("Client Connected")

    # 4순위(UX): 준비중 상태를 먼저 알려서 유니티에서 로딩 표시 가능
    await websocket.send_json({"status": "warming_up"})

    # 동시에 여러 클라가 붙으면 카메라/리소스 충돌 가능 -> 1명만 허용
    async with active_ws_lock:
        if active_ws is not None:
            await websocket.send_json({"error": "busy", "detail": "Another client is already connected."})
            await websocket.close()
            return
        active_ws = websocket

    # MediaPipe 프리웜 실패 시 여기서 안내
    if hands is None or mp_hands is None:
        await websocket.send_json({"error": "mediapipe_init_failed", "detail": "MediaPipe/Hands not initialized."})
        async with active_ws_lock:
            active_ws = None
        await websocket.close()
        return

    # 2순위: 카메라 오픈 최적화
    cap = _open_camera()
    if not cap.isOpened():
        await websocket.send_json({"error": "camera_open_failed", "detail": "Could not open camera."})
        cap.release()
        async with active_ws_lock:
            active_ws = None
        await websocket.close()
        return

    await websocket.send_json({"status": "ready"})

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                await websocket.send_json({"error": "camera_read_failed"})
                break

            frame = cv2.flip(frame, 1)
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            # hands는 전역 재사용(1순위). 혹시 몰라 process()는 락으로 보호
            async with hands_lock:
                results = hands.process(rgb_frame)

            data_to_send = None
            if results.multi_hand_landmarks:
                hand_landmarks = results.multi_hand_landmarks[0]

                # 3순위: 디버그용 드로잉/프리뷰는 옵션으로만
                if SHOW_PREVIEW and DRAW_LANDMARKS:
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                index_finger_tip = hand_landmarks.landmark[8]
                data_to_send = {"x": index_finger_tip.x, "y": index_finger_tip.y}

            if data_to_send is not None:
                await websocket.send_json(data_to_send)

            if SHOW_PREVIEW:
                cv2.imshow("MediaPipe Hands Server View", frame)
                if cv2.waitKey(1) & 0xFF == ord("q"):
                    break

            await asyncio.sleep(0.01)

    except WebSocketDisconnect:
        print("Client Disconnected")
    except Exception as e:
        print(f"Error: {e}")
        try:
            await websocket.send_json({"error": "server_error", "detail": str(e)})
        except Exception:
            pass
    finally:
        cap.release()
        _safe_close_preview()
        async with active_ws_lock:
            active_ws = None
        try:
            await websocket.close()
        except Exception:
            pass


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)
