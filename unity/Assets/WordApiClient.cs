using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 서버에서 내려오는 단어 DTO
/// (JSON 필드 이름과 반드시 일치해야 함)
/// </summary>
[Serializable]
public class WordDto
{
    public int id;
    public string japanese;      // 일본어 단어 (히라가나)
    public string korean;        // 한글 뜻
    public int level;            // 난이도 (1~3)
    public string[] candidates;  // 히라가나 후보군 (5글자)
}

/// <summary>
/// JsonUtility는 최상위 JSON 배열을 직접 파싱할 수 없기 때문에
/// 배열을 감싸는 Wrapper 클래스가 필요함
/// </summary>
[Serializable]
public class WordDtoArrayWrapper
{
    public WordDto[] items;
}

public class WordApiClient : MonoBehaviour
{
    [Header("FastAPI 서버 주소")]
    [Tooltip("끝에 슬래시 없이 입력 (예: http://127.0.0.1:8000)")]
    public string baseUrl = "http://127.0.0.1:8000";

    /// <summary>
    /// 지정한 레벨의 단어 목록을 서버에서 가져온다.
    /// 사용 예:
    /// StartCoroutine(LoadWordsByLevel(1, OnWordsLoaded));
    /// </summary>
    public IEnumerator LoadWordsByLevel(int level, Action<List<WordDto>> onCompleted)
    {
        string url = $"{baseUrl}/words/level/{level}";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"[WordApiClient] 요청 실패: {req.error}");
                onCompleted?.Invoke(null);
                yield break;
            }

            // FastAPI에서 JSON 배열 형태로 반환한다고 가정:
            // [ { ... }, { ... } ]
            string rawJson = req.downloadHandler.text;

            // JsonUtility는 배열을 직접 파싱할 수 없으므로
            // 임시로 객체로 감싸서 파싱한다.
            string wrappedJson = "{\"items\":" + rawJson + "}";

            WordDtoArrayWrapper wrapper = null;

            try
            {
                wrapper = JsonUtility.FromJson<WordDtoArrayWrapper>(wrappedJson);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[WordApiClient] JSON 파싱 실패: {e.Message}\n원본 JSON: {rawJson}"
                );
                onCompleted?.Invoke(null);
                yield break;
            }

            var list = new List<WordDto>();
            if (wrapper != null && wrapper.items != null)
                list.AddRange(wrapper.items);

            onCompleted?.Invoke(list);
        }
    }
}
