using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System.Collections;

public class AttackAnimation : MonoBehaviour
{
    public MaskableGraphic spriteRenderer;
    public Vector2 attackOffset = new Vector2(20, 0);
    private Vector2 originalPosition;
    private Vector2 originalSize;

    public Sprite idleSprite;
    public Sprite[] attackFrames;
    public Sprite deadSprite;

    public float frameDelay = 0.12f;


    [Header("Death Adjustment")]
    public Vector2 deathOffset = new Vector2(0, -40);    
    public float deathScale = 0.8f;

    void Start()
    {
        originalPosition = spriteRenderer.rectTransform.anchoredPosition;
        originalSize = spriteRenderer.rectTransform.sizeDelta;
    }

    public IEnumerator PlayAttack()
    {
        // 위치 저장 (UI용)
        originalPosition = spriteRenderer.rectTransform.anchoredPosition;

        // 이동 후 공격
        spriteRenderer.rectTransform.anchoredPosition += attackOffset;

        for (int i = 0; i < attackFrames.Length; i++)
        {
            SetSprite(attackFrames[i]);
            yield return new WaitForSeconds(frameDelay);
        }

        // 원래 위치 복귀 + Idle 복귀
        spriteRenderer.rectTransform.anchoredPosition = originalPosition;
        SetSprite(idleSprite);
    }

    public void PlayDead()
    {
        // 원래 값들 저장
        originalPosition = spriteRenderer.rectTransform.anchoredPosition;
        originalSize = spriteRenderer.rectTransform.sizeDelta;

        // 죽는 위치로 이동
        spriteRenderer.rectTransform.anchoredPosition += deathOffset;

        // 크기 축소 적용
        spriteRenderer.rectTransform.sizeDelta = originalSize * deathScale;

        // 스프라이트 변경
        SetSprite(deadSprite);
    }

    public void PlayIdle()
    {
        SetSprite(idleSprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer is Image img)
            img.sprite = sprite;

        else if (spriteRenderer is SVGImage svg)
            svg.sprite = sprite;
    }

    public void ResetPose()
    {
        spriteRenderer.rectTransform.anchoredPosition = originalPosition;
        spriteRenderer.rectTransform.sizeDelta = originalSize;
        PlayIdle();
    }
}
