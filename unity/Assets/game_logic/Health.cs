using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 목숨(하트) 기반 Health.
/// 기존 HP바 코드와의 호환을 위해 UpdateBar()를 래퍼로 유지.
/// </summary>
public class Health : MonoBehaviour
{
    [Tooltip("최대 목숨 수 (하트 개수)")]
    public int maxLives = 3;

    [HideInInspector]
    public int currentLives = 3;

    [Tooltip("하트 Image 배열(왼쪽부터 0,1,2...)")]
    public Image[] heartImages;

    [Tooltip("활성 하트 스프라이트")]
    public Sprite activeHeart;

    [Tooltip("비활성 하트 스프라이트")]
    public Sprite inactiveHeart;

    public GameOverController gameOverController;
    public bool isPlayer;

    public bool IsDead => currentLives <= 0;

    private void Start()
    {
        currentLives = GetClampedMaxLives();
        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        // 하트 시스템: 호출 1번당 1목숨 감소 (amount는 무시)
        currentLives = Mathf.Max(0, currentLives - 1);

        UpdateHearts();

        if (IsDead)
            Die();
    }

    public void ResetHealth()
    {
        currentLives = GetClampedMaxLives();
        UpdateHearts();
    }

    // ✅ 기존 코드(BattleController/GameClearController 등) 호환용
    public void UpdateBar()
    {
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            var img = heartImages[i];
            if (img == null) continue;

            bool on = i < currentLives;
            img.enabled = true;
            img.sprite = on ? activeHeart : inactiveHeart;
        }
    }

    private int GetClampedMaxLives()
    {
        int limit = (heartImages != null && heartImages.Length > 0) ? heartImages.Length : maxLives;
        return Mathf.Clamp(maxLives, 0, limit);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");

        if (isPlayer)
        {
            if (gameOverController != null)
                gameOverController.ShowGameOver(); // ✅ 여기!
            else
                Debug.LogError("GameOverController 연결 안 됨!");
        }
    }
}
