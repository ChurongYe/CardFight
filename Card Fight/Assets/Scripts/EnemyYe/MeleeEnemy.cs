using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : EnemyManager
{
    [Header("攻击")]
    public float AttackTime = 0.5f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    protected override void OnTryAttack()
    {
        if (!isAttacking)
            TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown && currentTarget != null)
        {
            FaceTarget(currentTarget);
            Vector2 dir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * 1f;
            Vector3 newWorldPos = transform.position + offset;

            if (attackArea != null)
                attackArea.transform.SetPositionAndRotation(newWorldPos, Quaternion.Euler(0, 0, angle));

            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine(AttackTime));
        }
    }

    IEnumerator AttackRoutine(float time)
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);
        if (attackArea != null) attackArea.SetActive(true);
        yield return new WaitForSeconds(time);
        if (attackArea != null) attackArea.SetActive(false);
        isAttacking = false;
    }
}
