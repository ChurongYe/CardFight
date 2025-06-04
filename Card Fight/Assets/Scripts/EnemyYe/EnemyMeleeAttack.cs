using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : EnemyManager
{
    [Header("攻击")]
    public float AttackTime = 0.5f;
    public float attackCooldown = 1.5f;
    public GameObject WeaponPivot;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    protected override void OnTryAttack()
    {
        ifattacking = true;
        if (!isAttacking)
            StartCoroutine(AttackFlow());
    }

    private IEnumerator AttackFlow()
    {
        if (Time.time - lastAttackTime >= attackCooldown && currentTarget != null)
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            FaceTarget(currentTarget);
            float swingAngle = 185f;

            Vector3 center = WeaponPivot.transform.position;

            if (attackArea != null)
                attackArea.SetActive(true);

            // 判断玩家相对于敌人在哪一侧
            float targetOffsetX = currentTarget.position.x - transform.position.x;
            bool attackRight = true;
            if (targetOffsetX >= 0f)
            {
                attackRight = true;
            }
            else
            {
                attackRight = false;
            }

            float timer = 0f;
            while (timer < AttackTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / AttackTime);
                float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

                float currentAngle;

                if (attackRight)
                {
                    // 顺时针挥舞（右扫）
                    currentAngle = Mathf.Lerp(swingAngle / 2f, -swingAngle / 2f, easedT);
                }
                else
                {
                    // 逆时针挥舞（左扫）
                    currentAngle = Mathf.Lerp(-swingAngle / 2f, -swingAngle / 2f + 180, easedT);
                }

                Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
                Vector3 offset = attackRight ? rotation * Vector3.right   * 1f : rotation * Vector3.left * 1f;
                Vector3 rotatedPos = center + offset;

                if (attackArea != null)
                {
                    attackArea.transform.position = rotatedPos;
                    attackArea.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
                }

                yield return null;
            }

            if (attackArea != null)
                attackArea.SetActive(false);
        }

        isAttacking = false;
        ifattacking = false;
    }
}