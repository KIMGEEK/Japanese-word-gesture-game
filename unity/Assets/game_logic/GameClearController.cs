using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the clear panel UI when the player clears a stage.
/// Provides methods to show the clear panel, load the next level,
/// and return to the home screen by resetting progress.
/// </summary>
public class GameClearController : MonoBehaviour
{
    public GameObject clearPanel;
    public WordQuizManager quiz;
    public BattleController battle;

    void Start()
    {
        // Ensure the clear panel is hidden at the start of the scene.
        if (clearPanel != null)
            clearPanel.SetActive(false);
    }

    /// <summary>
    /// Activates the clear panel and pauses the game.
    /// </summary>
    public void ShowClear()
    {
        Time.timeScale = 0f;
        if (clearPanel != null)
            clearPanel.SetActive(true);
    }

    /// <summary>
    /// Called by the "Next Level" button.
    /// Resumes the game, resets health and monster state,
    /// and requests the next level from the quiz manager.
    /// </summary>
    public void LoadNextLevel()
    {
        // Hide the clear panel and resume time.
        if (clearPanel != null)
            clearPanel.SetActive(false);

        Time.timeScale = 1f;

        // Reset the player's health and update the UI.
        if (battle != null && battle.playerHealth != null)
        {
            battle.playerHealth.ResetHealth();
            battle.playerHealth.UpdateBar();
        }

        // Reset the monster's state if a battle controller exists.
        if (battle != null)
            battle.ResetMonster();

        // Request the next level from the quiz manager.
        if (quiz != null)
            quiz.StartCoroutine(quiz.LoadNextLevelInternal());
    }

    /// <summary>
    /// Called by a "Home" button to reset progress and return to the start scene.
    /// </summary>
    public void ReturnHome()
    {
        // Hide the clear panel and resume time.
        if (clearPanel != null)
            clearPanel.SetActive(false);
        Time.timeScale = 1f;

        // Reset the quiz level to the beginning.
        WordQuizManager.targetLevel = 1;

        // Load the StartScene to return to the home screen.
        SceneManager.LoadScene("StartScene");
    }
}