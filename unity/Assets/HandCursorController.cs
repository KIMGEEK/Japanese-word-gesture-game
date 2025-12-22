using UnityEngine;

public class HandCursorController : MonoBehaviour
{
    [Header("손 좌표 WebSocket 클라이언트 (비워도 됨: 자동 탐색)")]
    public HandWebSocketClient wsClient;

    [Header("화면 위에 움직일 커서(예: 작은 원 모양 Image)")]
    public RectTransform cursorRect;

    [Header("좌표 해석용 Canvas")]
    public Canvas canvas;

    [Range(0.01f, 1f)]
    public float smoothFactor = 0.15f;

    private Vector2 smoothPos;
    private Vector2 targetPos;
    private bool hasReceivedAnyPos = false;

    private HandInputManager mgr;

    private void Awake()
    {
        // 커서 숨김(좌표 못 받는 동안 0,0으로 끌려가지 않게)
        if (cursorRect != null)
            cursorRect.gameObject.SetActive(false);

        // 매니저/클라이언트 자동 확보
        mgr = HandInputManager.Instance != null
            ? HandInputManager.Instance
            : Object.FindFirstObjectByType<HandInputManager>(FindObjectsInactive.Include);

        if (wsClient == null && mgr != null)
            wsClient = mgr.Client;

        if (wsClient != null)
            wsClient.OnHandPositionReceived += OnHandPositionReceived;

        if (mgr != null)
            mgr.OnSceneReloaded += ResetCursorState;
    }

    private void OnDestroy()
    {
        if (wsClient != null)
            wsClient.OnHandPositionReceived -= OnHandPositionReceived;

        if (mgr != null)
            mgr.OnSceneReloaded -= ResetCursorState;
    }

    public void ResetCursorState()
    {
        hasReceivedAnyPos = false;
        smoothPos = Vector2.zero;
        targetPos = Vector2.zero;

        if (cursorRect != null)
            cursorRect.gameObject.SetActive(false);
    }

    private void OnHandPositionReceived(Vector2 normalizedPos)
    {
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        float x = (normalizedPos.x - 0.5f) * canvasRect.sizeDelta.x;
        float y = ((1 - normalizedPos.y) - 0.5f) * canvasRect.sizeDelta.y;

        targetPos = new Vector2(x, y);

        if (!hasReceivedAnyPos)
        {
            hasReceivedAnyPos = true;
            smoothPos = targetPos;

            if (cursorRect != null)
                cursorRect.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!hasReceivedAnyPos) return;

        smoothPos = Vector2.Lerp(smoothPos, targetPos, smoothFactor);

        if (cursorRect != null)
            cursorRect.anchoredPosition = smoothPos;
    }
}
