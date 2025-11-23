using System.Collections.Generic;
using UnityEngine;

public class MagicCircleInput : MonoBehaviour
{
    private List<string> buffer = new();

    public void AddLetter(string letter)
    {
        buffer.Add(letter);
    }

    public void ClearInput()
    {
        buffer.Clear();
    }

    public string GetResultString()
    {
        return string.Join("", buffer);
    }
}
