using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    public Health playerHealth;
    public Health monsterHealth;

    public AttackAnimation playerAnim;
    public AttackAnimation monsterAnim;

    private int maxTurns = 3;

    void Start()
    {
        StartCoroutine(TurnBattle());
    }

    IEnumerator TurnBattle()
    {
        for (int turn = 0; turn < maxTurns; turn++)
        {
            // Player 공격
            Debug.Log("Player → Monster 공격!");
            yield return StartCoroutine(playerAnim.PlayAttack());
            monsterHealth.TakeDamage(100);

            if (monsterHealth.IsDead)
            {
                monsterAnim.PlayDead();
                Debug.Log("몬스터 사망!");
                yield break;
            }

            yield return new WaitForSeconds(1f);

            // Monster 공격
            Debug.Log("Monster → Player 공격!");
            yield return StartCoroutine(monsterAnim.PlayAttack());
            playerHealth.TakeDamage(100);

            if (playerHealth.IsDead)
            {
                playerAnim.PlayDead();
                Debug.Log("플레이어 사망…");
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        Debug.Log("3턴 종료!");
    }
}
