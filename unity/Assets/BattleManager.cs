using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private string currentWord = "";

    public BattleController battleController;  // ���� ��� ��ũ��Ʈ ����
    public string answerWord = "����������";   // ���� ����(���ϴ� �ܾ�� ��ü)

    void Awake()
    {
        Instance = this;
    }

    public void AddLetter(string letter)
    {
        currentWord += letter;
        Debug.Log("���� �ܾ�: " + currentWord);

        // ���� 5���� á�� �� �ڵ� ����
        if (currentWord.Length >= answerWord.Length)
        {
            CheckAnswer();
        }
    }

    void CheckAnswer()
    {
        var mc = Object.FindFirstObjectByType<MagicCircleController>();

        if (currentWord == answerWord)
        {
            Debug.Log("����! ���� ����");
            battleController.PlayerAttack();

            if (mc != null)
                mc.ResetSelectionLock();
        }
        else
        {
            Debug.Log("����! ���� �ݰ�!");

            // ���� ����
            battleController.MonsterAttack();

            // 3�ʰ� ���� ����
            if (mc != null)
                mc.LockForSeconds(3f);
        }

        // ���� ������ ���� �ʱ�ȭ
        currentWord = "";

        // ���� �� �� �ʱ�ȭ
        if (mc != null) mc.ClearLines();
    }

}

