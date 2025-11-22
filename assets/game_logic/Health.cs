using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHP = 300;
    public int currentHP;

    public Slider hpBar; // UI HP바 연결
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
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        //gameObject.SetActive(false);
    }
}
