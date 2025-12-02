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

        // 1) 플레이어 공격 모션
        yield return StartCoroutine(playerAnim.PlayAttack());

        // 2) 몬스터 데미지
        monsterHealth.TakeDamage(100);

        // 3) 몬스터 피격 연출 (죽었더라도 한 번 튕겼다가 쓰러지게)
        if (!monsterHealth.IsDead)
        {
            yield return StartCoroutine(monsterAnim.PlayHit());
            busy = false;
            yield break;
        }
        else
        {
            // 죽는 경우: 살짝 피격 모션 + 사망 모션
            yield return StartCoroutine(monsterAnim.PlayHit());
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
    }
    
    public void MonsterAttack()
    {
        if (!busy)
            StartCoroutine(MonsterAttackRoutine());
    }

    IEnumerator MonsterAttackRoutine()
    {
        busy = true;

        // 1) 몬스터 공격 모션
        yield return StartCoroutine(monsterAnim.PlayAttack());

        // 2) 플레이어 데미지
        playerHealth.TakeDamage(100);

        // 3) 플레이어 피격 연출
        if (!playerHealth.IsDead)
        {
            yield return StartCoroutine(playerAnim.PlayHit());
            busy = false;
            yield break;
        }
        else
        {
            playerAnim.PlayDead();

            if (playerHealth.isPlayer)
            {
                if (playerHealth.gameOverController != null)
                    playerHealth.gameOverController.ShowGameOver();
                else
                    Debug.LogError("GameOverController 연결 안 됨!");
            }

            busy = false;
            yield break;
        }
    }

    public void ResetMonster()
    {
        monsterHealth.ResetHealth();        // HP 회복
        monsterAnim.ResetPose();             // 위치 / 크기 / idle 복구
        monsterHealth.UpdateBar();
    }

}
