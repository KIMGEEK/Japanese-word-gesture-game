using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

// 서버가 보내주는 JSON 형식: { "x": 0.123, "y": 0.456 }
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

    /// <summary>
    /// 외부에서 손가락 위치를 구독할 수 있도록 하는 이벤트
    /// (0~1 사이의 정규화된 좌표라고 가정)
    /// </summary>
    public event Action<Vector2> OnHandPositionReceived;

    private async void Start()
    {
        await Connect();
    }

    private async Task Connect()
    {
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
            // 서버에서 온 메시지를 문자열로 변환
            string json = Encoding.UTF8.GetString(bytes);
            // Debug.Log("[HandWebSocketClient] 받은 메시지: " + json);

            try
            {
                var msg = JsonUtility.FromJson<HandPositionMessage>(json);
                if (msg != null)
                {
                    var pos = new Vector2(msg.x, msg.y);
                    OnHandPositionReceived?.Invoke(pos);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[HandWebSocketClient] JSON 파싱 실패: " + ex.Message);
            }
        };

        await websocket.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // WebGL이 아닌 경우에는 매 프레임마다 메시지 큐 처리
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    // 필요하다면 서버로 데이터도 보낼 수 있음
    public async void SendText(string message)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
            return;

        await websocket.SendText(message);
    }
}
