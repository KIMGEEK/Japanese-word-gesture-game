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

    private Vector2 smoothPos;   // 스무딩된 좌표
    private float smoothFactor = 0.15f; // 작을수록 부드럽게

    private void Awake()
    {
        if (wsClient != null)
        {
            wsClient.OnHandPositionReceived += OnHandPositionReceived;
        }
    }

    private Vector2 targetPos; // WebSocket에서 받은 원본 좌표

    private void OnDestroy()
    {
        if (wsClient != null)
        {
            wsClient.OnHandPositionReceived -= OnHandPositionReceived;
        }
    }

    private void OnHandPositionReceived(Vector2 normalizedPos)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // (0,0)~(1,1) → Canvas 좌표로 맵핑
        float x = (normalizedPos.x - 0.5f) * canvasRect.sizeDelta.x;
        float y = ((1 - normalizedPos.y) - 0.5f) * canvasRect.sizeDelta.y;

        targetPos = new Vector2(x, y);
    }

    private void Update()
    {
        smoothPos = Vector2.Lerp(smoothPos, targetPos, smoothFactor);

        if (cursorRect != null)
            cursorRect.anchoredPosition = smoothPos;
    }
}
