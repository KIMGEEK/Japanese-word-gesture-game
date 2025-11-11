import cv2, math
import mediapipe as mp

mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)
canvas = None

def dist(a, b):
    return math.hypot(a[0]-b[0], a[1]-b[1])

with mp_hands.Hands( # 초기 설정이고 실제 성능 혹은 인식 개수 바꾸려면 랜드마크 픽셀 좌표로 변환하는 부분(변수 이름: multi_hand_landmarks)에서 확인
    static_image_mode=False,
    max_num_hands=2,
    min_detection_confidence=0.8,
    min_tracking_confidence=0.6
) as hands:
    prev = None
    drawing = False
    while True:
        ok, frame = cap.read()
        if not ok: break
        frame = cv2.flip(frame, 1)
        h, w = frame.shape[:2]
        if canvas is None:
            canvas = 255 * (frame[:, :, 0:1] * 0 + 1)  # 흰색 단일채널 캔버스

        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        res = hands.process(rgb)

        if res.multi_hand_landmarks:
            for hand in res.multi_hand_landmarks:
            # 랜드마크 → 픽셀 좌표
                pts = [(int(l.x*w), int(l.y*h)) for l in hand.landmark]
                idx_tip = pts[8]
                thumb_tip = pts[4]

                # 엄지-검지 거리 기반 그리기 토글
                drawing = dist(idx_tip, thumb_tip) < 40  # 카메라/해상도에 맞게 조정

                # 부드럽게(간단한 지수평활)
                alpha = 0.5
                if prev is None:
                    smooth = idx_tip
                else:
                    smooth = (int(alpha*idx_tip[0] + (1-alpha)*prev[0]),
                            int(alpha*idx_tip[1] + (1-alpha)*prev[1]))

                if drawing and prev is not None:
                    cv2.line(canvas, prev, smooth, color=0, thickness=4)  # 검은 선

                prev = smooth

                # 참고: 손 랜드마크 시각화
                mp_drawing.draw_landmarks(frame, hand, mp_hands.HAND_CONNECTIONS)
        else:
            prev = None
            drawing = False

        # 합성하여 표시
        show = frame.copy()
        show[:, :, 1] = cv2.subtract(show[:, :, 1], canvas[:, :, 0] - 255)  # 간단 합성
        cv2.imshow("draw", show)
        if cv2.waitKey(1) & 0xFF == 27:
            break

cap.release()
cv2.destroyAllWindows()
