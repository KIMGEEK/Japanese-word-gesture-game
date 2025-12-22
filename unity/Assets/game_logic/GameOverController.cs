using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        StartCoroutine(RetryRoutine());
    }

    private IEnumerator RetryRoutine()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ✅ WebSocket 오브젝트를 Destroy/Close 하지 않습니다.
        // DontDestroyOnLoad 매니저가 씬 로드 시 EnsureConnected + Reset 브로드캐스트를 합니다.
        var scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);

        yield break;
    }
}
