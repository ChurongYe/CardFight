using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shibie : MonoBehaviour
{
    public float AttackTime = 1f;
    private float detectionRangePlayer = 20f;
    private float detectionRangeSummon = 1f;
    private float moveSpeed = 6f;
    public float attackCooldown = 1.5f;
    private float attackDamage = 1;

    private GameObject Player;

    private float lastAttackTime = -Mathf.Infinity;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private Vector2 enemyApproachPoint;
    private bool hasApproachPoint = false;
    private bool isAttacking = false;
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (isAttacking) return;

        if (currentTarget == null)
        {
            SearchForPlayer();
        }
        if (currentTarget != null && currentTarget .CompareTag("Summon"))
        {
            MoveToTargetWithStopNearEdge(currentTarget);
        }
        else if(currentTarget != null && currentTarget.CompareTag("Player"))
        {
            MoveToTargetWithStopNearEdge(currentTarget);
            SearchForSummon();
        }
    }
    void MoveToTargetWithStopNearEdge(Transform target)
    {
        float stopThreshold = 0.15f;
        float maxApproachRange = 2f;

        if (!hasApproachPoint || Vector2.Distance(target.position, enemyApproachPoint) > maxApproachRange)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(0.4f, 0.6f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            enemyApproachPoint = (Vector2)target.position + offset;
            hasApproachPoint = true;
        }

        float dist = Vector2.Distance(transform.position, enemyApproachPoint);

        if (dist > stopThreshold)
        {
            MoveToTarget(enemyApproachPoint);
        }
        else
        {
            rb.velocity = Vector2.zero;
            TryAttack();
        }
    }

    void SearchForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRangePlayer);
        float closestDistance = Mathf.Infinity;
        Transform targetEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Renderer renderer = hit.GetComponentInChildren<Renderer>();
                if (renderer == null || !renderer.isVisible)
                    continue; // 不可见则跳过

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetEnemy = hit.transform;
                }
            }
        }

        if (targetEnemy != null)
        {
            currentTarget = targetEnemy;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(1.5f, 2.5f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            enemyApproachPoint = (Vector2)currentTarget.position + offset;
            hasApproachPoint = true;
        }
    }
    void SearchForSummon()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRangeSummon);
        float closestDistance = Mathf.Infinity;
        Transform targetEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Summon"))
            {
                Renderer renderer = hit.GetComponentInChildren<Renderer>();
                if (renderer == null || !renderer.isVisible)
                    continue; // 不可见则跳过

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetEnemy = hit.transform;
                }
             
            }
        }

        if (targetEnemy != null)
        {
            currentTarget = targetEnemy;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(1.5f, 2.5f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            enemyApproachPoint = (Vector2)currentTarget.position + offset;
            hasApproachPoint = true;
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine(AttackTime));
        }
    }
    IEnumerator AttackRoutine(float time)
    {
        isAttacking = true;
        Debug.Log($"{gameObject.name} 正在攻击目标！");
        // 加入动画、音效等攻击表现
        yield return new WaitForSeconds(time);
        isAttacking = false;
    }

    void MoveToTarget(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }
}