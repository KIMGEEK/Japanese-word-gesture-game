using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private string currentWord = "";

    public BattleController battleController;  // 전투 담당 스크립트 연결
    public string answerWord = "あいえおう";   // 예시 정답(원하는 단어로 교체)

    void Awake()
    {
        Instance = this;
    }

    public void AddLetter(string letter)
    {
        currentWord += letter;
        Debug.Log("현재 단어: " + currentWord);

        // 글자 5개가 찼을 때 자동 판정
        if (currentWord.Length >= answerWord.Length)
        {
            CheckAnswer();
        }
    }

    void CheckAnswer()
    {
        var mc = FindObjectOfType<MagicCircleController>();

        if (currentWord == answerWord)
        {
            Debug.Log("정답! 공격 실행");
            battleController.PlayerAttack();

            if (mc != null)
                mc.ResetSelectionLock();
        }
        else
        {
            Debug.Log("오답! 적이 반격!");

            // 몬스터 공격
            battleController.MonsterAttack();

            // 3초간 선택 금지
            if (mc != null)
                mc.LockForSeconds(3f);
        }

        // 다음 문제를 위해 초기화
        currentWord = "";

        // 오답 시 선 초기화
        if (mc != null) mc.ClearLines();
    }

}

