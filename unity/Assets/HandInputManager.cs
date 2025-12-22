using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandInputManager : MonoBehaviour
{
    public static HandInputManager Instance { get; private set; }

    [Header("WebSocket Client (같은 오브젝트에 붙여도 되고, 자식이어도 됨)")]
    [SerializeField] private HandWebSocketClient wsClient;

    /// <summary>
    /// 씬이 다시 로드될 때(재도전/스테이지 리셋) 구독자들에게 알려줌
    /// </summary>
    public event Action OnSceneReloaded;

    public HandWebSocketClient Client => wsClient;

    private async void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        // 참조 자동 확보
        if (wsClient == null)
            wsClient = GetComponentInChildren<HandWebSocketClient>(true);

        // 씬 로드 훅
        SceneManager.sceneLoaded += HandleSceneLoaded;

        // 첫 연결 보장
        if (wsClient != null)
            await wsClient.EnsureConnected();
        else
            Debug.LogWarning("[HandInputManager] HandWebSocketClient를 찾지 못했습니다. 이 오브젝트(또는 자식)에 붙여주세요.");
    }

    private async void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드될 때마다 연결 보장
        if (wsClient != null)
            await wsClient.EnsureConnected();

        // 커서/입력 상태 리셋 요청 브로드캐스트
        OnSceneReloaded?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public async Task ForceReconnect()
    {
        if (wsClient == null) return;
        await wsClient.CloseAsync();
        await wsClient.EnsureConnected();
    }
}
