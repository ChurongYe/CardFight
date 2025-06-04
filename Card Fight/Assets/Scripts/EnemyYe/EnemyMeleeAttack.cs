using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : EnemyManager 
{
    [Header("¹¥»÷")]
    public float AttackTime = 0.5f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    protected override void OnTryAttack()
    {
        ifattacking = true;
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
            {
                attackArea.transform.position = newWorldPos;
                attackArea.transform.rotation = Quaternion.Euler(0, 0, angle - 60f); // ´Ó×ó²àÆðÊ¼½Ç¶È
            }

            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine(AttackTime, angle));
        }
    }

    IEnumerator AttackRoutine(float time, float targetAngle)
    {
        isAttacking = true;
        if (attackArea != null) attackArea.SetActive(true);

        float swingAngle = 120f; // »Ó¶¯·¶Î§
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / time);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float currentAngle = Mathf.Lerp(targetAngle - swingAngle / 2, targetAngle + swingAngle / 2, easedT);

            if (attackArea != null)
            {
                attackArea.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            }

            yield return null;
        }

        if (attackArea != null) attackArea.SetActive(false);
        isAttacking = false;
        ifattacking = false;
    }

}
