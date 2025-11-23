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

    void UpdateBar()
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
    }
}
