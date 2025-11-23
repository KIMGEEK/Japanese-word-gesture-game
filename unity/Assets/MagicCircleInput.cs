using System.Collections.Generic;
using UnityEngine;

public class MagicCircleInput : MonoBehaviour
{
    private List<string> buffer = new();

    /// <summary>
    /// Letter 버튼 또는 제스처로 선택될 때 호출됨
    /// </summary>
    public void AddLetter(string letter)
    {
        buffer.Add(letter);
        Debug.Log($"선택됨 → {letter}");
    }

    public void ClearInput()
    {
        buffer.Clear();
    }

    /// <summary>
    /// 현재까지 연결한 글자들을 하나의 문자열로 반환
    /// 예: ["り","ん","ご"] → "りんご"
    /// </summary>
    public string GetResultString()
    {
        return string.Join("", buffer);
    }
}
