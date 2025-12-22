using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // 게임 오버 패널 표시
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // 재도전 버튼
    public void Retry()
    {
        Time.timeScale = 1f;
        StartCoroutine(RetryRoutine());
    }

    private IEnumerator RetryRoutine()
    {
        // 오버 패널 숨김
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ✅ WebSocket은 닫거나 파괴하지 않는다. HandInputManager가 자동으로 재연결한다.
        // 씬만 다시 로드한다.
        var currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);

        yield break;
    }
}
