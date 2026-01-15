using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseEnemy : EnemyManager
{
    [Header("冲刺设置")]
    public float dashForce = 20f;
    public float dashCooldown = 3f;
    public float dashRange = 15f;
    public float knockbackForce = 10f;
    public float wallDetectDistance = 0.5f;
    public LayerMask wallLayer;
    public float dashPauseTime = 1f;

    private bool isDashing = false;
    private float dashTimer = 0f;

    protected override void Update()
    {
        base.Update();

        if (currentTarget == null || isDashing) return;

        dashTimer -= Time.deltaTime;
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (dashTimer <= 0f && distanceToTarget <= dashRange)
        {
            ifattacking = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;

            StartCoroutine(DashLoop());
            dashTimer = dashCooldown;
        }

        if (distanceToTarget > dashRange)
        {
            agent.enabled = true;
            ifattacking = false;
        }
    }

    IEnumerator DashLoop()
    {
        isDashing = true;
        int dashCount = 0;

        while (dashCount < 5)
        {
            if (currentTarget == null) break;

            Vector2 dashDirection = ((Vector2)(currentTarget.position - transform.position)).normalized;

            // 翻转朝向
            if (dashDirection.x > 0)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else
                transform.localScale = new Vector3(1f, 1f, 1f);

            bool hasKnockedBack = false;

            while (true)
            {
                transform.position += (Vector3)(dashDirection * dashForce * Time.deltaTime);

                // 撞墙检测
                RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dashDirection, wallDetectDistance, wallLayer);
                if (wallHit.collider != null)
                {
                    dashCount++;
                    yield return new WaitForSeconds(dashPauseTime);
                    break; // 重新计算方向，继续下次冲刺
                }

                // 撞击玩家一次
                if (!hasKnockedBack && Vector2.Distance(transform.position, currentTarget.position) < 1f)
                {
                    if (currentTarget.CompareTag("Player"))
                    {
                        Vector2 knockbackDir = ((Vector2)(currentTarget.position - transform.position)).normalized;
                        currentTarget.GetComponent<PlayerController>().CantMove(2f, knockbackDir);
                        hasKnockedBack = true;
                    }
                }

                yield return null;
            }
        }

        // 冲刺3次后消失
        Destroy(gameObject,1f);

        isDashing = false;
    }
}