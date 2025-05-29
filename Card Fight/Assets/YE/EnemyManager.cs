using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshAgent2D))]
public class EnemyManager : MonoBehaviour, IHurtable
{
    [Header("通用属性")]
    public int maxHP = 15;
    protected int currentHP;
    public float knockbackDistance = 0.5f;
    public float dieDelay = 0.5f;
    public GameObject attackArea;

    [Header("靠近目标参数")]
    [SerializeField] protected float stopThreshold = 0.1f;   // 靠近到这个范围就停止
    [SerializeField] protected float approachMinRadius = 0.4f;  // 靠近点最小半径
    [SerializeField] protected float approachMaxRadius = 0.6f;  // 靠近点最大半径
    [SerializeField] protected float extraStopBuffer = 0.1f;  // 额外安全距离
    [SerializeField] protected float knockbackCooldown = 0.5f;
    protected float lastKnockbackTime = -Mathf.Infinity;

    protected bool isDead = false;
    protected bool isAttacked = false;
    protected bool isBeingHurt = false;

    protected NavMeshAgent2D agent;
    protected Transform currentTarget;
    protected Vector2 enemyApproachPoint;
    protected bool hasApproachPoint = false;

    protected HurtUI hurtUI;
    protected GameObject player;

    public float detectionRangePlayer = 20f;
    public float detectionRangeSummon = 2f;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent2D>();
        currentHP = maxHP;

        // 设置停止距离
        CalculateStopThresholdFromAttackArea();

        // 初始化血条
        GameObject barPrefab = Resources.Load<GameObject>("EnemyHealthBar");
        if (barPrefab != null)
        {
            GameObject bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0);
            hurtUI = bar.GetComponent<HurtUI>();
            hurtUI.UpdateHealthBar(currentHP, maxHP);
        }
    }

    protected virtual void Update()
    {
        if (isDead || isBeingHurt) return;

        UpdateEnemyLogic();
        if (hurtUI != null)
            hurtUI.transform.localScale = transform.localScale;
    }

    protected virtual void UpdateEnemyLogic()
    {
        if (currentTarget == null)
            SearchForPlayer();

        if (currentTarget != null)
        {
            //先判断是否在攻击范围内
            if (IsInAttackRange(currentTarget))
            {
                FaceTarget(currentTarget);
                agent.destination = transform.position;
                OnTryAttack();
            }
            else
            {
                MoveToTargetWithStopNearEdge(currentTarget);
            }

            if (currentTarget.CompareTag("Player"))
                SearchForSummon(); // 优先切目标
        }
    }
    protected virtual bool IsInAttackRange(Transform target)
    {
        if (attackArea == null || target == null) return false;

        float attackRange = 0.5f;
        Collider2D col = attackArea.GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            attackRange = box.size.x * 0.5f * attackArea.transform.lossyScale.x;
        }
        else if (col is CircleCollider2D circle)
        {
            attackRange = circle.radius * attackArea.transform.lossyScale.x;
        }

        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }
    protected void MoveToTargetWithStopNearEdge(Transform target)
    {
        if (!hasApproachPoint || Vector2.Distance(target.position, enemyApproachPoint) > approachMaxRadius)
        {
            CreateApproachPoint(target.position, approachMinRadius, approachMaxRadius);
        }

        float dist = Vector2.Distance(transform.position, enemyApproachPoint);
        if (dist > stopThreshold)
        {
            FaceTarget(target);
            agent.destination = enemyApproachPoint;
        }
        else
        {
            agent.destination = transform.position;
            OnTryAttack();
        }
        // 防止太贴近玩家
        if (target.CompareTag("Player") && Vector2.Distance(transform.position, target.position) < 0.4f)
        {
            agent.destination = transform.position;
            OnTryAttack();
        }
    }
    protected void CalculateStopThresholdFromAttackArea()
    {
        if (attackArea == null)
        {
            approachMinRadius = 0.3f;
            approachMaxRadius = 0.5f;
            return;
        }
        float approachMinRatio = 0.6f; // 最小接近距离 = 攻击距离 × 0.6
        float approachMaxRatio = 0.9f;// 最大接近距离 = 攻击距离 × 0.9
        float attackRange = 0.5f; // 默认攻击距离
        Collider2D col = attackArea.GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            attackRange = box.size.x * 0.5f * attackArea.transform.lossyScale.x;
        }
        else if (col is CircleCollider2D circle)
        {
            attackRange = circle.radius * attackArea.transform.lossyScale.x;
        }
        else
        {
            Debug.LogWarning("未识别的碰撞体类型，使用默认攻击距离");
        }

        // 按照比例设置靠近半径
        approachMinRadius = attackRange * approachMinRatio;
        approachMaxRadius = attackRange * approachMaxRatio;

        // 再给一个缓冲区设置停止阈值
        stopThreshold = attackRange + extraStopBuffer;
    }
    protected virtual void OnTryAttack() { }

    protected void FaceTarget(Transform target)
    {
        Vector3 scale = transform.localScale;
        scale.x = target.position.x < transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    protected void CreateApproachPoint(Vector2 center, float minRadius, float maxRadius)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minRadius, maxRadius);
        enemyApproachPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        hasApproachPoint = true;
    }

    protected void SearchForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRangePlayer);
        float closestDistance = Mathf.Infinity;
        Transform target = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var renderer = hit.GetComponentInChildren<Renderer>();
                if (renderer == null || !renderer.isVisible) continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    target = hit.transform;
                }
            }
        }

        if (target != null)
        {
            currentTarget = target;
            CreateApproachPoint(target.position, 1.5f, 2.5f);
        }
    }

    protected void SearchForSummon()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRangeSummon);
        float closestDistance = Mathf.Infinity;
        Transform target = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Summon"))
            {
                var renderer = hit.GetComponentInChildren<Renderer>();
                if (renderer == null || !renderer.isVisible) continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    target = hit.transform;
                }
            }
        }

        if (target != null)
        {
            currentTarget = target;
            CreateApproachPoint(target.position, 1.5f, 2.5f);
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        hurtUI?.ShowDamage(damage, isCrit);
        hurtUI?.UpdateHealthBar(currentHP, maxHP);

        if (currentHP <= 0 && !isDead)
        {
            StartCoroutine(Die());
        }
    }

    protected virtual IEnumerator Die()
    {
        isDead = true;
        if (agent) agent.destination = transform.position;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(dieDelay);
        Destroy(gameObject);
    }

    protected IEnumerator Knockback(Vector2 direction, float distance, float duration)
    {
        isBeingHurt = true;
        if (agent.enabled) agent.enabled = false;

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
        agent.Warp(transform.position);// 强制同步 agent 的位置
        agent.enabled = true;
        isBeingHurt = false;
        isAttacked = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacked && (collision.CompareTag("weaponSprite") || collision.CompareTag("SummonTrigger")))
        {
            //加入击退冷却判断
            if (Time.time - lastKnockbackTime < knockbackCooldown)
                return;

            isAttacked = true;
            lastKnockbackTime = Time.time;
            CombatManager.Instance.DealDamage(gameObject);

            Vector2 dir = (transform.position - collision.transform.position).normalized;
            float kb = collision.CompareTag("weaponSprite") ? knockbackDistance : 0f;
            float duration = 0.2f;

            StartCoroutine(Knockback(dir, kb, duration));
        }
    }
}