using UnityEngine;

public class CompleteButton : MonoBehaviour
{
    public WordQuizManager quiz;

    public void OnClick()
    {
        quiz.OnSubmit();
    }
}