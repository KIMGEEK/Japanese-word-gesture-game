using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public GameObject gameOverPanel;

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // ∞‘¿” ∏ÿ√„
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}

