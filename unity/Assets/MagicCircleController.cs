using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MagicCircleController : MonoBehaviour
{
    [Header("손 커서 RectTransform")]
    public RectTransform handCursor;

    [Header("선택 가능한 글자들")]
    public List<LetterItem> letters = new List<LetterItem>();

    [Header("빛줄기 라인")]
    public LineRenderer lineRenderer;

    [Header("선택 이펙트 설정")]
    public float highlightInTime = 0.12f;
    public float highlightStayTime = 1f;
    public float highlightOutTime = 0.15f;
    public float highlightScale = 1.3f;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.6f);

    private List<Vector3> selectedPositions = new List<Vector3>();
    private string lastLetter = ""; // 중복 방지용

    public MagicCircleInput input;  // 추가
    public int LettersCount => letters.Count;
    private LetterItem currentHighlighted = null;


    // 선택방식으로 하다가 일단 안 씀
    //public void OnSelectLetter(string letter)
    //{
    //    input.AddLetter(letter);
    //}

    private bool isLocked = false;

    void Start()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        // 하이라이트 초기화
        foreach (var item in letters)
        {
            if (item.highlightImage != null)
            {
                item.highlightImage.gameObject.SetActive(false);
            }
        }

    }

    void Update()
    {
        foreach (var item in letters)
        {
            if (IsCursorOnLetter(item.rect))
            {
                TrySelectLetter(item);
            }
        }
    }

    public void SetLetters(string[] lettersToUse)
    {
        // 빈 배열 들어오면 초기화 처리
        if (lettersToUse == null || lettersToUse.Length == 0)
        {
            for (int i = 0; i < letters.Count; i++)
            {
                letters[i].letter = "";

                var tmp = letters[i].rect.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "";

                // 글자 초기화 시 하이라이트 꺼짐 초기화
                if (letters[i].highlightImage != null)
                    letters[i].highlightImage.gameObject.SetActive(false);
            }
            return;
        }

        // 정상적인 경우
        for (int i = 0; i < letters.Count; i++)
        {
            letters[i].letter = lettersToUse[i];

            var tmp = letters[i].rect.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = lettersToUse[i];
        }
    }

    bool IsCursorOnLetter(RectTransform letterRect)
    {
        // handCursor의 월드좌표 -> 화면좌표
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, handCursor.position);

        return RectTransformUtility.RectangleContainsScreenPoint(
            letterRect,
            screenPos,
            null
        );
    }

    public void ClearLines()
    {
        selectedPositions.Clear();
        lineRenderer.positionCount = 0;
    }

    void TrySelectLetter(LetterItem item)
    {
        if (isLocked)
            return;

        // 같은 글자면 또 실행하지 않음
        if (currentHighlighted == item)
            return;

        // 이전 선택된 글자가 있으면 하이라이트 제거
        if (currentHighlighted != null && currentHighlighted.highlightImage != null)
            currentHighlighted.highlightImage.gameObject.SetActive(false);

        // 지금 선택된 글자에 하이라이트 표시
        if (item.highlightImage != null)
            item.highlightImage.gameObject.SetActive(true);

        currentHighlighted = item;

        // 선택된 글자 처리
        lastLetter = item.letter;
        AddLinePoint(item.rect);
        input.AddLetter(item.letter);

        Debug.Log("선택됨: " + item.letter);
    }

    void AddLinePoint(RectTransform rect)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer가 연결되지 않았습니다.");
            return;
        }

        // UI -> 좌표
        Vector3 worldPos = rect.position;
        worldPos.z = 0f;  // UI와 LineRenderer의 깊이 통일
        selectedPositions.Add(worldPos);

        lineRenderer.positionCount = selectedPositions.Count;
        lineRenderer.SetPositions(selectedPositions.ToArray());
    }

    public void ResetSelectionLock()
    {
        lastLetter = "";
        isLocked = false;

        foreach (var item in letters)
        {
            if (item.highlightImage != null)
                item.highlightImage.gameObject.SetActive(false);
        }

        currentHighlighted = null;
    }

    public void LockForSeconds(float sec)
    {
        StartCoroutine(LockRoutine(sec));
    }

    IEnumerator LockRoutine(float sec)
    {
        isLocked = true;
        lastLetter = "";
        yield return new WaitForSeconds(sec);
        isLocked = false;
    }

    IEnumerator PlayHighlightEffect(Image img)
    {
        img.gameObject.SetActive(true);

        // 초기 상태
        img.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0f);
        img.rectTransform.localScale = Vector3.zero;

        float t = 0f;

        // 1) 등장 (scale 0 → highlightScale, alpha 0 → highlightColor.a)
        while (t < 1f)
        {
            t += Time.deltaTime / highlightInTime;
            float k = Mathf.Clamp01(t);

            img.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * highlightScale, k);
            img.color = new Color(
                highlightColor.r,
                highlightColor.g,
                highlightColor.b,
                Mathf.Lerp(0f, highlightColor.a, k)
            );

            yield return null;
        }

        // 2) 잠깐 유지
        yield return new WaitForSeconds(highlightStayTime);

        // 3) 사라지기 (scale 유지, alpha → 0)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / highlightOutTime;
            float k = Mathf.Clamp01(t);

            img.color = new Color(
                highlightColor.r,
                highlightColor.g,
                highlightColor.b,
                Mathf.Lerp(highlightColor.a, 0f, k)
            );

            yield return null;
        }

        img.gameObject.SetActive(false);
    }

    public void CancelAllSelection()
    {
        // 1) 라인 초기화
        selectedPositions.Clear();
        lineRenderer.positionCount = 0;

        // 2) 입력창 초기화
        input.ClearInput();   // MagicCircleInput 내부에 있어야 함

        // 3) 모든 하이라이트 끄기
        foreach (var item in letters)
        {
            if (item.highlightImage != null)
                item.highlightImage.gameObject.SetActive(false);
        }

        // 4) 선택 상태 초기화
        lastLetter = "";
        currentHighlighted = null;
        isLocked = false;

        Debug.Log("전체 선택 취소됨");
    }
}

[System.Serializable]
public class LetterItem
{
    public string letter;
    public RectTransform rect;

    public UnityEngine.UI.Image highlightImage;
}
