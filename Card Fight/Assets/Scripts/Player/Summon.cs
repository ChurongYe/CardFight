using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : MonoBehaviour
{
    private float wanderRadius = 15f;  
    private float minDistanceFromPlayer = 3f;
    private float detectionRange = 20f;
    private float moveSpeed = 3f;
    private float moveSpeedToEnemy = 8f;
    public float attackCooldown = 1.5f;
    private float attackDamage = 1;

    private GameObject Player;

    private float lastAttackTime = -Mathf.Infinity;
    private Vector2 wanderTarget;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private bool isIdling = false;
    private Vector2 enemyApproachPoint;
    private bool hasApproachPoint = false;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderTarget();
    }
    public float SummonattackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }
    void Update()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            currentTarget = null;
            hasApproachPoint = false;
            SearchForEnemy();
        }

        if (currentTarget != null)
        {
            MoveToTargetWithStopNearEdge(currentTarget);
        }
        else
        {
            Wander();
        }
    }
    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= detectionRange && target.gameObject.activeInHierarchy;
    }
    void MoveToTargetWithStopNearEdge(Transform target)
    {
        float stopThreshold = 0.3f;
        float maxApproachRange = 5f; 

        if (!hasApproachPoint || Vector2.Distance(target.position, enemyApproachPoint) > maxApproachRange)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(1.5f, 3f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            enemyApproachPoint = (Vector2)target.position + offset;
            hasApproachPoint = true;
        }

        float dist = Vector2.Distance(transform.position, enemyApproachPoint);

        if (dist > stopThreshold)
        {
            MoveToTarget(enemyApproachPoint, true);
        }
        else
        {
            rb.velocity = Vector2.zero;
            TryAttack();
        }
    }

    void SearchForEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Renderer renderer = hit.GetComponentInChildren<Renderer>();
                if (renderer == null || !renderer.isVisible)
                    continue; // 不可见则跳过

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(2f, 3f);
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
            Debug.Log("召唤物攻击了敌人！");

            // 面向敌人
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); 
        }
    }

    void Wander()
    {
        if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            if (!isIdling)
            {
                StartCoroutine(Idle());
            }
        }
        else
        {
            isIdling = false;
            MoveToTarget(wanderTarget, false);

            Vector2 dir = (wanderTarget - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); // 使召唤物朝向目标
        }
    }

    IEnumerator Idle()
    {
        isIdling = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        PickNewWanderTarget();
        isIdling = false;
    }

    void MoveToTarget(Vector2 target, bool ifEnemy)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = dir * (ifEnemy ? moveSpeedToEnemy : moveSpeed);

        // 使召唤物朝向目标
        if (ifEnemy)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    void PickNewWanderTarget()
    {
        float distanceFromPlayer = Random.Range(minDistanceFromPlayer, wanderRadius);
        Vector2 randomOffset = Random.insideUnitCircle * distanceFromPlayer;
        wanderTarget = (Vector2)Player.transform.position + randomOffset;
    }

}