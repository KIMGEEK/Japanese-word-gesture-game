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

    private List<Vector3> selectedPositions = new List<Vector3>();
    private string lastLetter = ""; // 중복 방지용

    public MagicCircleInput input;  // 추가

    public void OnSelectLetter(string letter)
    {
        input.AddLetter(letter);
    }

    private bool isLocked = false;

    void Start()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
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
            return;  // 잠금 상태면 아무것도 안 함

        if (lastLetter == item.letter)
            return;  // 같은 글자 연속 선택 방지

        lastLetter = item.letter;

        Debug.Log($"선택됨: {item.letter}");

        AddLinePoint(item.rect);

        BattleManager.Instance.AddLetter(item.letter);
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
        lastLetter = "";     // 이번 문제에서 사용된 마지막 글자 초기화
        isLocked = false;    // 잠금 해제
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
}

[System.Serializable]
public class LetterItem
{
    public string letter;
    public RectTransform rect;
}
