using UnityEngine;

public class WordApiTest : MonoBehaviour
{
    public WordApiClient api;

    void Start()
    {
        StartCoroutine(api.LoadWordsByLevel(1, words =>
        {
            if (words == null)
            {
                Debug.Log("단어 불러오기 실패");
                return;
            }

            foreach (var w in words)
            {
                Debug.Log($"[{w.id}] {w.japanese} - {w.korean}");
            }
        }));
    }
}
