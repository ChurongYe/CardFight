using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shibie : MonoBehaviour
{
    private WeaponImpact weaponImpact;
    public float AttackTime = 1f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 1f;
    private float knockbackDistance = 1f;

    private float detectionRangePlayer = 20f;
    private float detectionRangeSummon = 2f;

    private float lastAttackTime = -Mathf.Infinity;
    private Transform currentTarget;
    private Vector2 enemyApproachPoint;
    private bool hasApproachPoint = false;
    private bool isAttacking = false;
    private bool isAttacked = false;

    private GameObject Player;
    private NavMeshAgent2D agent;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent2D>();
        weaponImpact = FindObjectOfType<WeaponImpact>();
    }

    void Update()
    {
        knockbackDistance = weaponImpact.impactForce;
        if (isAttacking || isAttacked) return;

        if (currentTarget == null)
        {
            SearchForPlayer();
        }

        if (currentTarget != null)
        {
            if (currentTarget.CompareTag("Summon"))
            {
                MoveToTargetWithStopNearEdge(currentTarget);
            }
            else if (currentTarget.CompareTag("Player"))
            {
                MoveToTargetWithStopNearEdge(currentTarget);
                SearchForSummon(); // 优先攻击召唤物
            }
        }
    }

    void MoveToTargetWithStopNearEdge(Transform target)
    {
        float stopThreshold = 0.15f;
        float maxApproachRange = 2f;

        // 重设接近点
        if (!hasApproachPoint || Vector2.Distance(target.position, enemyApproachPoint) > maxApproachRange)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(0.4f, 0.6f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            enemyApproachPoint = (Vector2)target.position + offset;
            hasApproachPoint = true;
        }

        // 移动到接近点
        float dist = Vector2.Distance(transform.position, enemyApproachPoint);
        if (dist > stopThreshold)
        {
            agent.destination = enemyApproachPoint;
        }

        // 如果靠近目标玩家则攻击
        if (target.CompareTag("Player") && Vector2.Distance(transform.position, target.position) < 0.8f)
        {
            agent.destination = transform.position; // 停止移动
            TryAttack();
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
                    continue;

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
            CreateApproachPoint();
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
                    continue;

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
            CreateApproachPoint();
        }
    }

    void CreateApproachPoint()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(1.5f, 2.5f);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        enemyApproachPoint = (Vector2)currentTarget.position + offset;
        hasApproachPoint = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("weaponSprite") && !isAttacked)
        {
            isAttacked = true;
            Vector2 direction = (transform.position - Player.transform.position).normalized;
            //float knockbackDistance = 1f;
            float knockbackDuration = 0.2f;

            StartCoroutine(Knockback(direction, knockbackDistance, knockbackDuration));
        }
    }

    IEnumerator Knockback(Vector2 direction, float distance, float duration)
    {
        if (agent.enabled)
            agent.enabled = false;

        Vector2 start = transform.position;
        Vector2 target = start + direction * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        if (agent != null)
        {
            // 强制同步 agent 的位置
            agent.Warp(transform.position); 

            agent.enabled = true;

            if (hasApproachPoint)
                agent.destination = enemyApproachPoint;
        }
        isAttacked = false;
    }
}