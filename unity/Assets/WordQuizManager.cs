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
        if (wordQueue.Count == 0)
        {
            Debug.Log("모든 단어 퀴즈 완료!");

            // UI 및 입력 완전 초기화
            magicCircleController.ClearLines();
            magicInput.ClearInput();
            magicCircleController.ResetSelectionLock();

            // 글자 슬롯 비우기
            magicCircleController.SetLetters(new string[0]);
            return;
        }

        current = wordQueue.Dequeue();
        meaningText.text = current.korean;

        string[] letters = SplitToCharacters(current.japanese);

        //  UI 슬롯 개수가 충분한지 검사 (IndexOutOfRange 방지)
        if (letters.Length > magicCircleController.LettersCount)
        {
            Debug.LogError($"단어 '{current.japanese}'의 글자 수가 UI 슬롯보다 많습니다." +
                           $"({letters.Length} > {magicCircleController.LettersCount})");
        }

        //  초기화 순서 매우 중요
        magicCircleController.ResetSelectionLock();
        magicCircleController.ClearLines();
        magicInput.ClearInput();

        //  글자를 UI에 적용
        magicCircleController.SetLetters(letters);
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
            Debug.Log("정답! 플레이어 공격!");
            battleController.PlayerAttack();
        }
        else
        {
            Debug.Log("오답! 몬스터 공격!");
            battleController.MonsterAttack();
        }

        // 전투 애니메이션 기다리는 시간
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

    string[] SplitToCharacters(string text)
    {
        List<string> list = new List<string>();
        foreach (var c in text)
            list.Add(c.ToString());
        return list.ToArray();
    }
}
