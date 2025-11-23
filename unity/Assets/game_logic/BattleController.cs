using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    [Header("플레이어 / 몬스터 HP")]
    public Health playerHealth;
    public Health monsterHealth;

    [Header("플레이어 / 몬스터 애니메이션")]
    public AttackAnimation playerAnim;
    public AttackAnimation monsterAnim;

    private bool isBusy = false; // 애니메이션 중복 실행 방지

    // =============================
    // 플레이어 공격 (BattleManager에서 호출)
    // =============================
    public void PlayerAttack()
    {
        if (!isBusy)
            StartCoroutine(PlayerAttackRoutine());
    }

    IEnumerator PlayerAttackRoutine()
    {
        isBusy = true;

        Debug.Log("[Battle] 플레이어 공격!");

        // 플레이어 공격 애니메이션 재생
        yield return StartCoroutine(playerAnim.PlayAttack());

        // 데미지 적용
        monsterHealth.TakeDamage(100);

        // 사망 처리
        if (monsterHealth.IsDead)
        {
            monsterAnim.PlayDead();
            Debug.Log("몬스터 사망!");
        }

        isBusy = false;
    }

    //  몬스터 공격
    public void MonsterAttack()
    {
        if (!isBusy)
            StartCoroutine(MonsterAttackRoutine());
    }

    IEnumerator MonsterAttackRoutine()
    {
        isBusy = true;

        Debug.Log("[Battle] 몬스터 공격!");

        // 몬스터 공격 애니메이션 재생
        yield return StartCoroutine(monsterAnim.PlayAttack());

        // 데미지 적용
        playerHealth.TakeDamage(100);

        if (playerHealth.IsDead)
        {
            playerAnim.PlayDead();
            Debug.Log("플레이어 사망!");
        }

        isBusy = false;
    }
}
