using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : MonoBehaviour, ISummonUnit
{
    private CardYe manager;
    public int baseHP = 100;
    public int baseATK = 20;
    public GameObject attackArea;         // 攻击区域
    public float AttackTime = 0.5f;
    private GameObject bar;    // 血条预制体
    private HurtUI hurtUI;
    private bool isInvincible = false;
    private float invincibleDuration = 0.5f;  // 无敌时间
    bool isDead = false;
    private int currentHP;
    private int currentMaxHP;
    private int currentATK;

    private float wanderRadius = 15f;  
    private float minDistanceFromPlayer = 3f;
    private float detectionRange = 20f;
    private float moveSpeed = 3f;
    private float moveSpeedToEnemy = 8f;
    public float attackCooldown = 1.5f;

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
        currentMaxHP = baseHP;
        currentHP = currentMaxHP;
        currentATK = baseATK;
        // 初始化血条状态
        InitHealthBar();
        if (attackArea != null) attackArea.SetActive(false);
    }
    //血量，攻击力控制
    public void MultiplyStats(float multiplier)
    {
        currentMaxHP = Mathf.RoundToInt(baseHP * multiplier);
        currentATK = Mathf.RoundToInt(baseATK * multiplier);
        Debug.Log($"Shibie 当前属性：HP={currentMaxHP}, ATK={currentATK}");
    }
    void InitHealthBar()
    {
        GameObject barPrefab = Resources.Load<GameObject>("SummonHealthBar");
        if (barPrefab != null)
        {
            bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0); // 调整血条高度
            hurtUI = bar.GetComponent<HurtUI>();
            if (hurtUI != null)
                hurtUI.UpdateHealthBar(currentMaxHP, currentMaxHP);
        }
        else
        {
            Debug.LogWarning("召唤物未赋值血条预制体");
        }
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
            // 面向敌人
            if (currentTarget != null)
            {
                FaceTarget(currentTarget);
                // 世界方向
                Vector2 dir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 以父物体为中心计算新的世界位置
                Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * 1f; // 半径可调
                Vector3 newWorldPos = transform.position + offset;

                // 设置 attackArea 的世界位置与旋转，不受缩放影响
                attackArea.transform.SetPositionAndRotation(newWorldPos, Quaternion.Euler(0, 0, angle));
            }


            // 启动攻击区
            if (attackArea != null)
                StartCoroutine(AttackRoutine(AttackTime));  // 0.5秒攻击时间举例
        }
    }
    void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 scale = transform.localScale;
        if (target.position.x < transform.position.x)
            scale.x = Mathf.Abs(scale.x);  // 面朝左
        else
            scale.x = -Mathf.Abs(scale.x); // 面朝右（反转）

        transform.localScale = scale;
    }
    IEnumerator AttackRoutine(float attackDuration)
    {
        // 攻击前摇阶段（动画播放）
        yield return new WaitForSeconds(0.5f);

        if (attackArea != null)
            attackArea.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackArea != null)
            attackArea.SetActive(false);
    }
    // 受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (hurtUI != null)
            hurtUI.UpdateHealthBar(currentHP, currentMaxHP);

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());
        }
    }
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        // 受伤动画
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }
    public bool IsInvincible()
    {
        return isInvincible;
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;

        GetComponent<Collider2D>().enabled = false;
        //动画
        this.enabled = false;
        // 延迟销毁
        Destroy(gameObject, 1.0f);
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

            FaceTarget(currentTarget);
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
            FaceTarget(currentTarget);
        }
    }

    void PickNewWanderTarget()
    {
        float distanceFromPlayer = Random.Range(minDistanceFromPlayer, wanderRadius);
        Vector2 randomOffset = Random.insideUnitCircle * distanceFromPlayer;
        wanderTarget = (Vector2)Player.transform.position + randomOffset;
    }
    public void SetManager(CardYe manager)
    {
        this.manager = manager; // 赋值
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.RemoveSummon(this);
        }
    }

}