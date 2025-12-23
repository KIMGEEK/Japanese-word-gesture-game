using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System.Collections;

public class AttackAnimation : MonoBehaviour
{
    [Header("공통")]
    public MaskableGraphic spriteRenderer;

    [Header("기본 상태")]
    public Sprite idleSprite;
    public Sprite[] attackFrames;
    public Sprite deadSprite;
    public Sprite hitSprite;   // Inspector에 새 스프라이트 연결

    [Header("공격 모션")]
    public Vector2 attackOffset = new Vector2(20, 0);
    public float moveInTime = 0.1f;     // 앞으로 나가는 시간
    public float moveBackTime = 0.08f;  // 돌아오는 시간
    public float attackScale = 1.05f;   // 공격 시 살짝 확대
    public float frameDelay = 0.12f;

    [Header("피격 모션")]
    public float hitMoveDistance = 12f; // 피격 시 뒤로 밀리는 거리
    public float hitDuration = 0.08f;   // 왕복 한 번 시간
    public Color hitFlashColor = Color.red;
    [Range(0f, 1f)]
    public float hitFlashAlpha = 0.45f;

    [Header("기타")]
    public bool isPlayer = false;

    public GameObject projectilePrefab;        // 프리팹 할당
    public Transform projectileSpawnPoint;     // 발사 위치
    public Transform projectileTarget;         // 목표 위치
    public float projectileSpeed = 12f;        // 이동 속도

    [Header("Death Adjustment")]
    public Vector2 deathOffset = new Vector2(0, -40);
    public float deathScale = 0.8f;

    // 내부 상태
    private Vector2 originalPosition;
    private Vector2 originalSize;
    private Vector3 originalScale;
    private Color originalColor;

    void Start()
    {
        var rt = spriteRenderer.rectTransform;
        originalPosition = rt.anchoredPosition;
        originalSize = rt.sizeDelta;
        originalScale = rt.localScale;
        originalColor = spriteRenderer.color;
    }

    // -------------------
    //  공격 모션
    // -------------------
    public IEnumerator PlayAttack()
    {
        var rt = spriteRenderer.rectTransform;

        // 기준값 다시 저장 (씬에서 위치를 손으로 조정했을 수 있으니)
        originalPosition = rt.anchoredPosition;
        originalScale = rt.localScale;

        Vector2 targetPos = originalPosition + attackOffset;
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * attackScale;

        // 1) 앞으로 나가면서 살짝 확대
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveInTime;
            float k = Mathf.Clamp01(t);

            rt.anchoredPosition = Vector2.Lerp(originalPosition, targetPos, k);
            rt.localScale = Vector3.Lerp(startScale, targetScale, k);

            yield return null;
        }

        // 첫 번째 프레임 또는 원하는 지점에서 투사체 생성:
        if (projectilePrefab != null && projectileSpawnPoint != null && projectileTarget != null)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            p.Initialize(projectileTarget.position, projectileSpeed);
        }

        // 2) 공격 프레임 재생
        for (int i = 0; i < attackFrames.Length; i++)
        {
            SetSprite(attackFrames[i]);
            yield return new WaitForSeconds(frameDelay);
        }

        // 3) 원래 자리로 돌아오면서 원래 크기로
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveBackTime;
            float k = Mathf.Clamp01(t);

            rt.anchoredPosition = Vector2.Lerp(targetPos, originalPosition, k);
            rt.localScale = Vector3.Lerp(targetScale, originalScale, k);

            yield return null;
        }

        rt.anchoredPosition = originalPosition;
        rt.localScale = originalScale;
        SetSprite(idleSprite);
    }

    // -------------------
    //  피격 모션
    // -------------------
    public IEnumerator PlayHit()
    {
        var rt = spriteRenderer.rectTransform;

        Vector2 startPos = rt.anchoredPosition;
        originalColor = spriteRenderer.color;

        // 플레이어는 왼쪽, 몬스터는 오른쪽으로 밀리는 느낌
        Vector2 dir = isPlayer ? Vector2.left : Vector2.right;
        Vector2 hitOffset = dir * hitMoveDistance;

        Color startColor = originalColor;
        Color flashColor = hitFlashColor;
        flashColor.a = hitFlashAlpha;

        SetSprite(hitSprite);

        // 1) 밀리면서 번쩍
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / hitDuration;
            float k = Mathf.Clamp01(t);

            // 약간 흔들리는 느낌
            float shake = Mathf.Sin(k * Mathf.PI * 3f) * 2f;
            rt.anchoredPosition = startPos + hitOffset * k + new Vector2(shake, 0);

            spriteRenderer.color = Color.Lerp(startColor, flashColor, k);

            yield return null;
        }

        // 2) 원래 위치/색으로 복귀
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / hitDuration;
            float k = Mathf.Clamp01(t);

            rt.anchoredPosition = Vector2.Lerp(startPos + hitOffset, startPos, k);
            spriteRenderer.color = Color.Lerp(flashColor, startColor, k);

            yield return null;
        }

        rt.anchoredPosition = startPos;
        spriteRenderer.color = originalColor;

        SetSprite(idleSprite);   // 원상태 복귀
    }

    // -------------------
    //  사망 모션
    // -------------------
    public void PlayDead()
    {
        var rt = spriteRenderer.rectTransform;

        originalPosition = rt.anchoredPosition;
        originalScale = rt.localScale;

        if (!isPlayer)
        {
            // 죽을 때 살짝 아래로 내려가고, 전체 비율 유지한 채 축소
            rt.anchoredPosition = originalPosition + deathOffset;
            rt.localScale = originalScale * deathScale;
        }

        SetSprite(deadSprite);
    }

    public void PlayIdle()
    {
        SetSprite(idleSprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;

        if (spriteRenderer is Image img)
            img.sprite = sprite;
        else if (spriteRenderer is SVGImage svg)
            svg.sprite = sprite;
    }

    public void ResetPose()
    {
        var rt = spriteRenderer.rectTransform;

        rt.anchoredPosition = originalPosition;
        rt.sizeDelta = originalSize;
        rt.localScale = originalScale;
        spriteRenderer.color = originalColor;

        PlayIdle();
    }
}
