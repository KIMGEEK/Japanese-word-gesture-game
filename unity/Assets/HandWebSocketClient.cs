using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

[Serializable]
public class HandPositionMessage
{
    public float x;
    public float y;
}

public class HandWebSocketClient : MonoBehaviour
{
    [Header("WebSocket 서버 주소")]
    public string websocketUrl = "ws://127.0.0.1:8000/ws";

    private WebSocket websocket;
    private bool isConnecting;
    private bool isQuitting;

    public event Action<Vector2> OnHandPositionReceived;

    public bool IsOpen => websocket != null && websocket.State == WebSocketState.Open;

    private async void Start()
    {
        // 매니저가 있을 때도 Start가 호출될 수 있으니 안전하게 보장만
        await EnsureConnected();
    }

    public async Task EnsureConnected()
    {
        if (isConnecting) return;
        if (websocket != null &&
            (websocket.State == WebSocketState.Open || websocket.State == WebSocketState.Connecting))
            return;

        isConnecting = true;
        try
        {
            await ConnectInternal();
        }
        finally
        {
            isConnecting = false;
        }
    }

    private async Task ConnectInternal()
    {
        await CloseAsync();

        websocket = new WebSocket(websocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("[HandWebSocketClient] 연결 성공");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("[HandWebSocketClient] 에러: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.LogWarning("[HandWebSocketClient] 연결 종료: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);

            // ✅ 서버가 {"status":"warming_up"} / {"status":"ready"} 같은 메시지를 먼저 보낼 수 있음
            // JsonUtility는 없는 필드를 0으로 채우므로, x/y 키가 없으면 절대 파싱하지 않는다.
            if (!json.Contains("\"x\"") || !json.Contains("\"y\""))
                return;

            try
            {
                var msg = JsonUtility.FromJson<HandPositionMessage>(json);
                if (msg == null) return;

                var pos = new Vector2(msg.x, msg.y);

                // (선택) 범위 필터링: 서버가 정규화 0~1을 준다는 전제
                if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
                    return;

                OnHandPositionReceived?.Invoke(pos);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[HandWebSocketClient] JSON 파싱 스킵: " + ex.Message + " / raw=" + json);
            }
        };

        await websocket.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    public async Task CloseAsync()
    {
        if (websocket == null) return;

        try
        {
            if (websocket.State == WebSocketState.Open || websocket.State == WebSocketState.Connecting)
                await websocket.Close();
        }
        catch { }
        finally
        {
            websocket = null;
        }
    }

    public void ForceClose()
    {
        _ = CloseAsync();
    }

    private void OnDisable()
    {
        // ✅ DontDestroyOnLoad 매니저 구조에서는 씬 로드로 비활성화되는 경우가 있을 수 있으니
        // 여기서 끊어버리면 "재도전 시 아예 연결 시도조차 안 함"처럼 보이는 문제가 재발할 수 있음.
        // 앱 종료 때만 정리.
        if (isQuitting)
            _ = CloseAsync();
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        _ = CloseAsync();
    }

    public async void SendText(string message)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
            return;

        await websocket.SendText(message);
    }
}
