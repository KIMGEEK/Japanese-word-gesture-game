using System.Collections.Generic;
using UnityEngine;

public class WordQuizManager : MonoBehaviour
{
    public WordApiClient apiClient;              // 백엔드 API
    public MagicCircleInput magicInput;          // 히라가나 입력 시스템
    public int targetLevel = 1;                  // 현재 스테이지 레벨

    private Queue<WordDto> wordQueue;            // 단어 큐
    private WordDto current;                     // 현재 정답 단어

    public BattleManager battle; 

    void Start()
    {
        // 레벨 단어 로드
        StartCoroutine(apiClient.LoadWordsByLevel(targetLevel, words =>
        {
            if (words == null || words.Count == 0)
            {
                Debug.LogError("단어를 불러오지 못했습니다.");
                return;
            }

            // 단어 큐 생성
            wordQueue = new Queue<WordDto>(words);

            // 첫 퀴즈 시작
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
        Debug.Log($"새 문제 시작: 정답 = {current.japanese}");

        // 입력 초기화
        magicInput.ClearInput();
    }

    /// <summary>
    /// '선택 완료' 버튼에서 호출
    /// </summary>
    public void OnSubmit()
    {
        string userAnswer = magicInput.GetResultString();

        bool isCorrect = userAnswer == current.japanese;

        if (isCorrect)
        {
            Debug.Log("정답 → 플레이어 공격!");
            battle.PlayerAttack();     // ← BattleManager로 이벤트 전달
        }
        else
        {
            Debug.Log("오답 → 몬스터 공격!");
            battle.MonsterAttack();    // ← 오답 시 몬스터 반격
        }

        NextWord();
    }

}
