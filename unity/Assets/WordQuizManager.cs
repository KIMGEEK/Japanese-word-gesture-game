using System.Collections.Generic;
using UnityEngine;

public class WordQuizManager : MonoBehaviour
{
    public WordApiClient apiClient;
    public MagicCircleInput magicInput;
    public int targetLevel = 1;

    public BattleController battleController;   // ← 직접 연결
    public MagicCircleController magicCircleController;

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
        if (wordQueue.Count == 0)
        {
            Debug.Log("모든 단어 퀴즈 완료!");
            return;
        }

        current = wordQueue.Dequeue();
        Debug.Log($"새 문제 시작: {current.japanese}");

        magicInput.ClearInput();
        magicCircleController.ClearLines();
    }

    public void OnSubmit()
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

        NextWord();
    }
}
