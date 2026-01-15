using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushEnemy : EnemyManager 
{
    [Header("攻击检测")]
    public float disappearRange = 5f;
    public float attackCooldown = 2f;
    public float attackDistance = 10f;
    public GameObject AmbushProfeb;
    public LayerMask wallLayer;

    private float attackTimer;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2D;

    private enum State { Hidden, Attacking }
    private State currentState = State.Hidden;

    private Vector3 lastDisappearPosition;
    private bool isJustAppeared = false;

    protected override void Start()
    {
        base.Start();
        agent.enabled = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();

        attackTimer = attackCooldown;
        spriteRenderer.enabled = false;
        collider2D.enabled = false;

        StartCoroutine(WaitForTargetAndAppear());
    }
    IEnumerator WaitForTargetAndAppear()
    {
        // 等待直到 currentTarget 不为空
        while (currentTarget == null)
            yield return null;

        // 继续正常流程
        StartCoroutine(AppearAtValidPosition());
    }

    protected override void Update()
    {
        base.Update();
        if (currentTarget == null) return;

        float distToPlayer = Vector2.Distance(transform.position, currentTarget.transform.position);

        if (currentState == State.Attacking)
        {
            if (distToPlayer <= disappearRange && currentTarget.CompareTag("Player"))
            {
                StartCoroutine(DisappearAndRespawn());
                return;
            }

            attackTimer -= Time.deltaTime;
            FacePlayer();

            if (!isJustAppeared && attackTimer <= 0f)
            {
                if (CanAttackPlayer())
                {
                    TryAttack();
                    attackTimer = attackCooldown;
                }
                else
                {
                    StartCoroutine(AppearAtValidPosition());
                }
            }
        }
        else if (currentState == State.Hidden)
        {
            if (distToPlayer > disappearRange)
            {
                StartCoroutine(AppearAtLastPosition());
            }
        }
    }

    void FacePlayer()
    {
        Vector3 scale = transform.localScale;
        scale.x = currentTarget.position.x < transform.position.x ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    IEnumerator AppearAtValidPosition()
    {
        Vector2 offset;
        Vector3 candidatePos = Vector3.zero;
        int maxTries = 20;
        int tryCount = 0;

        while (tryCount < maxTries)
        {
            float minDistance = 8f; // 防止靠太近
            float randomDistance = Random.Range(minDistance, attackDistance);
            Vector2 direction = Random.insideUnitCircle.normalized;
            offset = direction * randomDistance;
            candidatePos = currentTarget.position + (Vector3)offset;

            Vector2 dir = (currentTarget.position - candidatePos).normalized;
            float dist = Vector2.Distance(candidatePos, currentTarget.position);

            bool wallBlocked = Physics2D.Raycast(candidatePos, dir, dist, wallLayer);
            bool overlapped = Physics2D.OverlapCircle(candidatePos, 0.5f, wallLayer);

            if (!wallBlocked && !overlapped)
            {
                break;
            }
            tryCount++;
        }

        transform.position = candidatePos;
        spriteRenderer.enabled = true;
        collider2D.enabled = true;
        currentState = State.Attacking;
        gameObject.tag = "Enemy"; //恢复为敌人 Tag
        isJustAppeared = true;
        yield return new WaitForSeconds(0.5f);
        TryAttack();
        attackTimer = attackCooldown;
        yield return new WaitForSeconds(0.1f); // 防止一帧触发两次
        isJustAppeared = false;
    }

    IEnumerator AppearAtLastPosition()
    {
        transform.position = lastDisappearPosition;
        spriteRenderer.enabled = true;
        collider2D.enabled = true;
        currentState = State.Attacking;
        gameObject.tag = "Enemy"; //恢复为敌人 Tag
        isJustAppeared = true;
        yield return new WaitForSeconds(0.5f);
        TryAttack();
        attackTimer = attackCooldown;
        yield return new WaitForSeconds(0.1f);
        isJustAppeared = false;
    }

    IEnumerator DisappearAndRespawn()
    {
        lastDisappearPosition = transform.position;

        spriteRenderer.enabled = false;
        collider2D.enabled = false;
        currentState = State.Hidden;
        gameObject.tag = "Untagged"; //设置为默认 Tag
        yield return null;
    }

    bool CanAttackPlayer()
    {
        if (currentTarget == null) return false;

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist > attackDistance) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, wallLayer);
        return hit.collider == null;
    }

    void TryAttack()
    {
        if (currentTarget == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;

        if (AmbushProfeb != null)
        {
            GameObject aoe = Instantiate(AmbushProfeb, transform.position, Quaternion.identity);
            aoe.transform.parent = transform;
            aoe.SetActive(true);

            Rigidbody2D rb = aoe.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = dir * 10f;
            }
            else
            {
                Debug.LogWarning("预制体没有 Rigidbody2D");
            }
        }
    }
}