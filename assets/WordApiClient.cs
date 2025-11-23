using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 백엔드에서 내려오는 단어 DTO (JSON 필드 이름에 맞춰야 함)
[Serializable]
public class WordDto
{
    public int id;
    public string japanese;
    public string korean;
    public int level;
}

// JsonUtility는 배열 직렬화를 바로 못 해서 래퍼 필요
[Serializable]
public class WordDtoArrayWrapper
{
    public WordDto[] items;
}

public class WordApiClient : MonoBehaviour
{
    [Header("FastAPI 서버 주소")]
    [Tooltip("맨 뒤에 슬래시 없음. 예) http://127.0.0.1:8000")]
    public string baseUrl = "http://127.0.0.1:8000";

    /// <summary>
    /// 지정한 레벨의 단어 리스트를 가져온다.
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

            // FastAPI가 JSON 배열을 직접 내려준다고 가정:  [ { ... }, { ... } ]
            string rawJson = req.downloadHandler.text;
            // JsonUtility는 배열을 바로 못 읽으니, 래퍼 객체로 감쌉니다.
            string wrappedJson = "{\"items\":" + rawJson + "}";

            WordDtoArrayWrapper wrapper = null;
            try
            {
                wrapper = JsonUtility.FromJson<WordDtoArrayWrapper>(wrappedJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WordApiClient] JSON 파싱 실패: {e.Message}\n원본: {rawJson}");
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
