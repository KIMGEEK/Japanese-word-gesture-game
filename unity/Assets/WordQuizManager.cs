using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordQuizManager : MonoBehaviour
{
    public WordApiClient apiClient;
    public MagicCircleInput magicInput;
    public static int targetLevel = 1;

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
        if (wordQueue == null || wordQueue.Count == 0)
        {
            Debug.Log($"레벨 {targetLevel}의 단어 퀴즈 완료!");
            return;
        }

        current = wordQueue.Dequeue();
        meaningText.text = current.korean;

        // ✅ 서버에서 내려준 후보군 사용
        string[] choices = GetCandidatesOrFallback(current);

        // UI 슬롯 개수 검사
        if (choices.Length > magicCircleController.LettersCount)
        {
            Debug.LogError($"후보군 수가 UI 슬롯보다 많습니다. ({choices.Length} > {magicCircleController.LettersCount})");
        }

        // 초기화 순서
        magicCircleController.ResetSelectionLock();
        magicCircleController.ClearLines();
        magicInput.ClearInput();

        // 글자를 UI에 적용
        magicCircleController.SetLetters(choices);
    }

    public void OnLevelClear()
    {
        Debug.Log($"레벨 {targetLevel} 클리어 (몬스터 처치)");
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

    // -------------------------
    // candidates 파싱/검증
    // -------------------------

    [System.Serializable]
    private class StringArrayWrapper
    {
        public string[] items;
    }

    private string[] GetCandidatesOrFallback(WordDto dto)
    {
        // dto가 없으면 안전하게 fallback
        if (dto == null)
        {
            Debug.LogWarning("[WordQuizManager] dto가 null이라 fallback 사용");
            return BuildFallbackChoices(null);
        }

        // 1) 서버 candidates가 비어있으면 fallback
        if (dto.candidates == null || dto.candidates.Length == 0)
        {
            Debug.LogWarning($"[WordQuizManager] candidates가 비어있음: {dto.japanese}. fallback 사용");
            return BuildFallbackChoices(dto.japanese);
        }

        // 2) (권장) 후보군에 정답 글자들이 모두 포함되는지 검증
        if (!AnswerCharsIncluded(dto.japanese, dto.candidates))
        {
            Debug.LogWarning($"[WordQuizManager] candidates에 정답 글자가 충분히 포함되지 않음: {dto.japanese}. fallback 사용");
            return BuildFallbackChoices(dto.japanese);
        }

        // 3) 정상 반환
        return dto.candidates;
    }


    private bool AnswerCharsIncluded(string answer, string[] candidates)
    {
        if (string.IsNullOrEmpty(answer) || candidates == null) return false;

        // 정답의 각 글자가 candidates 중 하나라도 포함되는지 검사
        foreach (char c in answer)
        {
            string s = c.ToString();
            bool found = false;
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] == s) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }

    // 서버 candidates가 없거나 깨진 경우를 대비한 안전장치(기존 로직 "축소 버전")
    private string[] BuildFallbackChoices(string japanese)
    {
        // 정답 글자들 + 랜덤으로 5개 채우기
        List<string> result = new List<string>();
        if (!string.IsNullOrEmpty(japanese))
        {
            foreach (char c in japanese)
                result.Add(c.ToString());
        }

        string jpPool = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
        System.Random rng = new System.Random();

        while (result.Count < 5)
        {
            int idx = rng.Next(jpPool.Length);
            result.Add(jpPool[idx].ToString());
        }

        // shuffle
        for (int i = 0; i < result.Count; i++)
        {
            int r = rng.Next(i, result.Count);
            (result[i], result[r]) = (result[r], result[i]);
        }

        return result.ToArray();
    }
}
