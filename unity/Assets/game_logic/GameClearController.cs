using UnityEngine;

public class GameClearController : MonoBehaviour
{
    public GameObject clearPanel;
    public WordQuizManager quiz;  // ← WordQuizManager 연결
    public BattleController battle;

    void Start()
    {
        clearPanel.SetActive(false);
    }

    public void ShowClear()
    {
        Time.timeScale = 0f;
        clearPanel.SetActive(true);
    }

    public void LoadNextLevel()
    {
        clearPanel.SetActive(false);
        Time.timeScale = 1f;

        battle.ResetMonster();

        // 여기서 바로 다음 레벨 요청
        quiz.StartCoroutine(quiz.LoadNextLevelInternal());
    }
}
