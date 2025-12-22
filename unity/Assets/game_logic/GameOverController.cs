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

        // WebSocket은 닫거나 파괴하지 않는다. HandInputManager가 자동으로 재연결한다.
        // 씬만 다시 로드한다.
        var currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);

        yield break;
    }

    public void ReturnHome()
    {
        // 시간 정지를 해제한다.
        Time.timeScale = 1f;

        // 단어 퀴즈 레벨을 처음으로 리셋한다. WordQuizManager는 static으로 targetLevel을 보유한다.
        // 다음 게임을 처음부터 시작하려면 targetLevel을 1로 설정하면 된다.
        WordQuizManager.targetLevel = 1;

        // 필요시 추가 리셋 로직을 여기서 수행할 수 있다. 예: PlayerPrefs.DeleteKey("SavedLevel");

        // 홈 화면(StartScene)으로 이동한다.
        SceneManager.LoadScene("StartScene");
    }
}
