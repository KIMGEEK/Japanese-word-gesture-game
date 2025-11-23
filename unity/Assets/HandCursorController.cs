using UnityEngine;
using UnityEngine.UI;

public class HandCursorController : MonoBehaviour
{
    [Header("손 좌표 WebSocket 클라이언트")]
    public HandWebSocketClient wsClient;

    [Header("화면 위에 움직일 커서(예: 작은 원 모양 Image)")]
    public RectTransform cursorRect;

    [Header("좌표 해석용 Canvas")]
    public Canvas canvas;

    public static Vector2 cursorNormalizedPos; // 0~1 좌표 저장

    private void Awake()
    {
        if (wsClient != null)
        {
            wsClient.OnHandPositionReceived += OnHandPositionReceived;
        }
    }

    private void OnDestroy()
    {
        if (wsClient != null)
        {
            wsClient.OnHandPositionReceived -= OnHandPositionReceived;
        }
    }

    private void OnHandPositionReceived(Vector2 normalizedPos)
    {
        cursorNormalizedPos = normalizedPos;
        // 서버에서 넘겨주는 x,y가 0~1 사이라고 가정 (왼쪽~오른쪽, 아래~위)
        // Canvas 기준 좌표로 변환
        if (canvas == null || cursorRect == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // (0,0)~(1,1) → Canvas 좌표로 맵핑
        float x = (normalizedPos.x - 0.5f) * canvasRect.sizeDelta.x;
        float y = ((1 - normalizedPos.y) - 0.5f) * canvasRect.sizeDelta.y;

        cursorRect.anchoredPosition = new Vector2(x, y);
    }
}
