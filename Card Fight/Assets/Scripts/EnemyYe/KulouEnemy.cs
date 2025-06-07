using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KulouEnemy : EnemyManager
{
    [Header("攻击")]
    public float AttackTime = 0.3f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;
    protected override void Update()
    {
        if (Vector2.Distance(transform.position, player.transform.position) < 8f)
        {
            IfneedWalk = true;
            if (attackArea != null) attackArea.SetActive(true);
        }
        base.Update();
    }

    protected override void OnTryAttack()
    {
        ifattacking = true;
        if (!isAttacking)
            StartCoroutine(TryAttack());
    }

    private IEnumerator TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown && currentTarget != null)
        {
            FaceTarget(currentTarget);
            //Vector2 dir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
            //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * 1f;
            //Vector3 newWorldPos = transform.position + offset;

            //if (attackArea != null)
            //    attackArea.transform.SetPositionAndRotation(newWorldPos, Quaternion.Euler(0, 0, angle));

            lastAttackTime = Time.time;

            // --- AttackRoutine合并进来了 ---
            isAttacking = true;

            yield return new WaitForSeconds(0.5f);
            if (attackArea != null) attackArea.SetActive(true);

            yield return new WaitForSeconds(AttackTime);
            if (attackArea != null) attackArea.SetActive(false);

            isAttacking = false;
            ifattacking = false;
        }
        else
        {
            // 攻击失败的情况也可以重置 ifattacking，否则状态卡住
            ifattacking = false;
        }
    }
}