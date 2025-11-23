using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    public Health playerHealth;
    public Health monsterHealth;

    public AttackAnimation playerAnim;
    public AttackAnimation monsterAnim;

    private bool busy = false;

    public void PlayerAttack()
    {
        if (!busy)
            StartCoroutine(PlayerAttackRoutine());
    }

    IEnumerator PlayerAttackRoutine()
    {
        busy = true;
        yield return StartCoroutine(playerAnim.PlayAttack());
        monsterHealth.TakeDamage(100);

        if (monsterHealth.IsDead)
        {
            monsterAnim.PlayDead();
            Debug.Log("몬스터 사망!");
        }

        busy = false;
    }

    public void MonsterAttack()
    {
        if (!busy)
            StartCoroutine(MonsterAttackRoutine());
    }

    IEnumerator MonsterAttackRoutine()
    {
        busy = true;
        yield return StartCoroutine(monsterAnim.PlayAttack());
        playerHealth.TakeDamage(100);

        if (playerHealth.IsDead)
        {
            playerAnim.PlayDead();
            Debug.Log("플레이어 사망!");
        }

        busy = false;
    }
}
