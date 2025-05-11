using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : MonoBehaviour
{
    private float wanderRadius = 20f;  
    private float minDistanceFromPlayer = 3f;
    private float detectionRange = 30f;
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
        float stopDistance = 1f;

        Collider2D enemyCollider = target.GetComponent<Collider2D>();
        float enemyRadius = enemyCollider != null ? Mathf.Max(enemyCollider.bounds.extents.x, enemyCollider.bounds.extents.y) : 0.5f;

        float desiredDistance = enemyRadius + stopDistance;
        float currentDistance = Vector2.Distance(transform.position, target.position);

        Vector2 targetPoint = target.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Summon"))
            {
                float angle = Random.Range(30f, 60f) * Mathf.Deg2Rad;
                Vector2 dir = ((Vector2)transform.position - (Vector2)target.position).normalized;
                dir = new Vector2(
                    dir.x * Mathf.Cos(angle) - dir.y * Mathf.Sin(angle),
                    dir.x * Mathf.Sin(angle) + dir.y * Mathf.Cos(angle)
                );
                targetPoint = (Vector2)target.position + dir * desiredDistance;
                break;
            }
        }

        if (currentDistance > desiredDistance)
        {
            MoveToTarget(targetPoint, true);
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
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        currentTarget = closestEnemy;
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("’ŸªΩŒÔπ•ª˜¡Àµ–»À£°");
        }
    }

    void Wander()
    {
        if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            if (!isIdling)
            {
                Debug.Log("yes");
                StartCoroutine(Idle());
            }
        }
        else
        {
            Debug.Log("no");
            isIdling = false; 
            MoveToTarget(wanderTarget,false );
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
        if (ifEnemy)
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            rb.velocity = dir * moveSpeedToEnemy;
        }
        else
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            rb.velocity = dir * moveSpeed;
        }
    }

    void PickNewWanderTarget()
    {
        float distanceFromPlayer = Random.Range(minDistanceFromPlayer, wanderRadius);
        Vector2 randomOffset = Random.insideUnitCircle * distanceFromPlayer;
        wanderTarget = (Vector2)Player.transform.position + randomOffset;
    }
}