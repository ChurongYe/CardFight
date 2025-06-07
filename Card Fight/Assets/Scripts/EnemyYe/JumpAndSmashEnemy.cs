using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAndSmashEnemy : EnemyManager
{
    [Header("跳跃设置")]
    public float jumpInterval = 1.5f;
    public float jumpDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float jumpDistance = 2f;
    public LayerMask obstacleLayer;

    [Header("攻击设置")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public GameObject enemyPrefab;
    public float spawnRadius = 2f;

    [Header("动画与音效")]
    //public Animator animator;
    //public AudioClip jumpSound;

    private float lastJumpTime = -Mathf.Infinity;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isJumping = false;
    private bool isAttacking = false;

    protected override void Update()
    {
        agent.enabled = false;
        base.Update();
        if (currentTarget == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);

        if (!isJumping && Time.time - lastJumpTime > jumpInterval)
        {
            StartCoroutine(JumpTowardsPlayer());
        }
        else if (!isAttacking && distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
        {
            StartCoroutine(SmashAttack());
        }
    }

    private IEnumerator JumpTowardsPlayer()
    {
        FaceTarget(currentTarget);
        isJumping = true;
        lastJumpTime = Time.time;

        Vector2 startPos = transform.position;
        Vector2 targetPos = FindJumpTarget();

        float timer = 0f;
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            newPos.y += height;

            transform.position = newPos;
            yield return null;
        }

        transform.position = targetPos;

        //跳跃落地触发一次攻击判定
        if (attackArea != null)
        {
            attackArea.SetActive(true);
            yield return new WaitForSeconds(0.2f); // 攻击判定持续时间，可调
            attackArea.SetActive(false);
        }

        isJumping = false;
    }
    private Vector2 FindJumpTarget()
    {
        Vector2 toPlayer = currentTarget.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // 如果靠近玩家，则直接跳到玩家身上
        if (distanceToPlayer <= attackRange + 0.5f)
        {
            // 可加偏移量让敌人落在玩家上方一点（比如头顶）
            Vector2 offset = new Vector2(0, 0.2f);
            Vector2 preciseTarget = (Vector2)currentTarget.position + offset;

            // 检查有没有墙挡住
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (preciseTarget - (Vector2)transform.position).normalized, distanceToPlayer, obstacleLayer);
            if (hit.collider == null)
                return preciseTarget;
        }

        // 否则远距跳跃逻辑（避开障碍）
        Vector2[] directionsToTry = new Vector2[]
        {
        toPlayer.normalized,
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down
        };

        foreach (Vector2 dir in directionsToTry)
        {
            Vector2 target = (Vector2)transform.position + dir * jumpDistance;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, jumpDistance, obstacleLayer);
            if (hit.collider == null)
                return target;
        }

        // 被困住时原地跳
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return (Vector2)transform.position + randomDir * 0.5f;
    }


    private IEnumerator SmashAttack()
    {
        isAttacking = true;
        ifattacking = true;
        lastAttackTime = Time.time;

        FaceTarget(currentTarget);

        if (attackArea != null)
            attackArea.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (attackArea != null)
            attackArea.SetActive(false);

        isAttacking = false;
        ifattacking = false;
    }
    protected override IEnumerator Die()
    {
        yield return SpawnCopiesAround();
        yield return StartCoroutine(base.Die());  // 等待基类 Die 执行完
    }
    IEnumerator SpawnCopiesAround()
    {
        int spawnCount = Random.Range(3,5);

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0.5f, spawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;

            Vector3 spawnPos = transform.position + offset;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        yield return null;
    }
}