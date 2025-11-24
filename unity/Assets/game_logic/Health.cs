using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public int maxHP = 300;
    public int currentHP = 300;

    public Slider hpBar; // UI HP바 연결
    public TextMeshProUGUI hpText; // 숫자 표시

    public bool IsDead => currentHP <= 0; // 죽었는지 확인

    public GameOverController gameOverController;
    public bool isPlayer;

    void Start()
    {
        currentHP = maxHP;
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        UpdateBar();

        if (currentHP == 0)
            Die();
    }

    public void UpdateBar()
    {
        if (hpBar != null)
            hpBar.value = (float)currentHP / maxHP;

        if (hpText != null)
            hpText.text = $"{currentHP} / {maxHP}";
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        //gameObject.SetActive(false);

        if (isPlayer)   // 플레이어가 죽었다면
        {
            if (gameOverController != null)
                gameOverController.ShowGameOver();   // ← GameOver 호출
            else
                Debug.LogError("GameOverController 연결 안됨!");
        }
    }
    public void ResetHealth()
    {
        currentHP = maxHP;
        UpdateBar();
    }
}
