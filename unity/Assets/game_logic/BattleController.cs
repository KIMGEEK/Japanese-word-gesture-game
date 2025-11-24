using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    public Health playerHealth;
    public Health monsterHealth;

    public AttackAnimation playerAnim;
    public AttackAnimation monsterAnim;

    private bool busy = false;

    public GameClearController clearController;
    public Transform monsterTransform;
    private Vector3 monsterStartPos;
    public WordQuizManager quizManager;

    void Start()
    {
        monsterStartPos = monsterTransform.position;
    }

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
            quizManager.OnLevelClear();
            if (clearController != null)
                clearController.ShowClear();
            else
                Debug.LogError("GameClearController 연결 안 됨!");

            busy = false;
            yield break;
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

    public void ResetMonster()
    {
        monsterHealth.ResetHealth();        // HP 회복
        monsterAnim.ResetPose();             // 위치 / 크기 / idle 복구
        monsterHealth.UpdateBar();
    }

}
