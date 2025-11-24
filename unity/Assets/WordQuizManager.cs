using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordQuizManager : MonoBehaviour
{
    public WordApiClient apiClient;
    public MagicCircleInput magicInput;
    public int targetLevel = 1;

    public BattleController battleController;   // ← 직접 연결
    public MagicCircleController magicCircleController;
    public TextMeshProUGUI meaningText;

    private Queue<WordDto> wordQueue;
    private WordDto current;

    void Start()
    {
        StartCoroutine(apiClient.LoadWordsByLevel(targetLevel, words =>
        {
            if (words == null || words.Count == 0)
            {
                Debug.LogError("단어를 불러오지 못했습니다.");
                return;
            }

            wordQueue = new Queue<WordDto>(words);
            NextWord();
        }));
    }

    void NextWord()
    {
        //  모든 문제 끝났을 때도 UI가 반드시 초기화되도록 처리
        if (wordQueue == null || wordQueue.Count == 0)
        {
            Debug.Log($"레벨 {targetLevel}의 단어 퀴즈 완료!");

            return;
        }


        current = wordQueue.Dequeue();
        meaningText.text = current.korean;

        string[] answer = SplitToCharacters(current.japanese);
        string[] choices = BuildLetterChoices(answer);

        //  UI 슬롯 개수가 충분한지 검사 (IndexOutOfRange 방지)
        if (choices.Length > magicCircleController.LettersCount)
        {
            Debug.LogError($"단어 '{current.japanese}'의 글자 수가 UI 슬롯보다 많습니다." +
                           $"({choices.Length} > {magicCircleController.LettersCount})");
        }

        //  초기화 순서 매우 중요
        magicCircleController.ResetSelectionLock();
        magicCircleController.ClearLines();
        magicInput.ClearInput();

        //  글자를 UI에 적용
        magicCircleController.SetLetters(choices);
    }

    public void OnLevelClear()
    {
        // 몬스터가 죽으면 → 이 레벨은 끝난 것이다
        Debug.Log($"레벨 {targetLevel} 클리어 (몬스터 처치)");

        // 아래는 GameClearController 버튼이 눌렸을 때 호출하는 구조
        // 즉 NextLevelInternal()이 실행되게 준비만 함
    }

    public void OnSubmit()
    {
        StartCoroutine(HandleSubmit());
    }

    IEnumerator HandleSubmit()
    {
        string result = magicInput.GetResultString();
        bool correct = result == current.japanese;

        if (correct)
        {
            battleController.PlayerAttack();

            // 적이 죽었으면 이후 진행 중단
            if (battleController.monsterHealth.IsDead)
                yield break;
        }
        else
        {
            battleController.MonsterAttack();
            if (battleController.playerHealth.IsDead)
                yield break;
        }

        yield return new WaitForSeconds(1.0f);
        NextWord();
    }

    string[] BuildLetterChoices(string[] answer)
    {
        List<string> result = new List<string>(answer);

        // 부족한 글자 수 만큼 랜덤 문자 추가
        string jpPool = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめも";

        System.Random rng = new System.Random();

        while (result.Count < 5)
        {
            int idx = rng.Next(jpPool.Length);
            result.Add(jpPool[idx].ToString());
        }

        // UI가 너무 한쪽으로 정답 몰리면 안 되므로 shuffle
        for (int i = 0; i < result.Count; i++)
        {
            int r = rng.Next(i, result.Count);
            (result[i], result[r]) = (result[r], result[i]);
        }

        return result.ToArray();
    }

    public IEnumerator LoadNextLevelInternal()
    {
        targetLevel++;

        if (targetLevel > 3)
        {
            Debug.Log("모든 레벨 종료!");
            yield break;
        }

        yield return StartCoroutine(apiClient.LoadWordsByLevel(targetLevel, words =>
        {
            if (words == null || words.Count == 0)
            {
                Debug.LogError("단어 불러오기 실패");
                return;
            }

            wordQueue = new Queue<WordDto>(words);
            NextWord();
        }));
    }


    string[] SplitToCharacters(string text)
    {
        List<string> list = new List<string>();
        foreach (var c in text)
            list.Add(c.ToString());
        return list.ToArray();
    }
}
