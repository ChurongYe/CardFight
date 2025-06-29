using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Core.CardValue;
using static PlayerController;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshAgent2D))]
public class EnemyManager : MonoBehaviour, IHurtable, IStunnable
{
    private PlayerValue playerValue;
    [Header("通用属性")]
    public int maxHP = 15;
    protected int currentHP;
    public float knockbackDistance = 0.5f;
    public float dieDelay = 0.5f;
    public GameObject attackArea;
    public bool IfsetAttackRadius;
    public bool ifattacking = false;
    public bool IfneedWalk = true;
    public bool CanKnockback = true;

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
    public GameObject player;

    public float detectionRangePlayer = 20f;
    public float detectionRangeSummon = 2f;

    protected bool isStunned = false;
    protected float stunEndTime = -1f;

    public Animator animator;

    protected virtual void Start()
    {
        playerValue = FindObjectOfType<PlayerValue>();
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent2D>();
        currentHP = maxHP;

        // 初始化血条
        GameObject barPrefab = Resources.Load<GameObject>("EnemyHealthBar");
        if (barPrefab != null)
        {
            GameObject bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0);
            hurtUI = bar.GetComponent<HurtUI>();
            hurtUI.UpdateHealthBar(currentHP, maxHP);
        }
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (hurtUI != null)
            hurtUI.transform.localScale = transform.localScale;
        if (isDead) return;

        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                agent.enabled = true;
                ShowStunEffect(false);
            }
            return; // 被眩晕不能移动或攻击
        }

        if (isBeingHurt) return;
        if (ifattacking) return;
        SearchForTarget();//寻找目标
        if (!IfneedWalk) return;
        UpdateEnemyLogic();//接近目标
                           // 控制移动动画
        if (agent != null && animator != null && agent.enabled && !ifattacking && !isStunned)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
    protected virtual void SearchForTarget()
    {
        if (currentTarget == null)
            SearchForPlayer();//寻找目标
        else
        {
            if (currentTarget.CompareTag("Player"))
                SearchForSummon(); // 优先切目标
        }
    }
    protected virtual void UpdateEnemyLogic()
    {
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
        }
    }
    protected virtual bool IsInAttackRange(Transform target)
    {
        if (attackArea == null || target == null) return false;
        if(!IfsetAttackRadius)
        {
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
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        //不在距离范围内，直接返回 false
        if (distanceToTarget < approachMinRadius || distanceToTarget > approachMaxRadius)
            return false;

        //增加这段：进行视线检测（防止隔墙攻击）
        Vector2 start = transform.position;
        Vector2 end = target.position;

        // LayerMask 设置成你的“墙体”层
        int obstacleLayerMask = LayerMask.GetMask("Wall", "Obstacle");

        // 如果中间打到墙，就不能攻击
        RaycastHit2D hit = Physics2D.Linecast(start, end, obstacleLayerMask);
        if (hit.collider != null)
        {
            return false; // 被墙挡住了
        }

        return true;
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
        //// 防止太贴近玩家
        //if (target.CompareTag("Player") && Vector2.Distance(transform.position, target.position) < 0.4f)
        //{
        //    agent.destination = transform.position;
        //    OnTryAttack();
        //}
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
            //CreateApproachPoint(target.position, 1.5f, 2.5f);
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
            //CreateApproachPoint(target.position, 1.5f, 2.5f);
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        hurtUI?.ShowDamage(damage, isCrit);
        hurtUI?.UpdateHealthBar(currentHP, maxHP);

        playerValue.TryLifeSteal(damage);

        if (currentHP <= 0 && !isDead)
        {
            StartCoroutine(Die());
        }
    }

    protected virtual IEnumerator Die()
    {
        isDead = true;
        if (agent)
        { agent.destination = transform.position; }
        GetComponent<Collider2D>().enabled = false;
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        yield return new WaitForSeconds(dieDelay);
        Destroy(gameObject);
    }

    protected IEnumerator Knockback(Vector2 direction, float distance, float duration)
    {
        if (distance > 0f && CanKnockback)
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
        }
        isAttacked = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacked && (collision.CompareTag("weaponSprite") || collision.CompareTag("SummonTrigger")
            || collision.CompareTag("PlayerFire") || collision.CompareTag("Fireball") || collision.CompareTag("Lighting")
            || collision.CompareTag("OneLight")))
        {
            //加入击退冷却判断
            if (Time.time - lastKnockbackTime < knockbackCooldown)
                return;

            isAttacked = true;
            lastKnockbackTime = Time.time;
            // 判断攻击类型
            if (collision.CompareTag("weaponSprite")&& PlayerController .currentAttackMode == AttackMode.Melee)
            {
                if ((CardValue.attackType & AttackType.NormalMelee) != 0)
                {
                    CombatManager.Instance.DealDamage(gameObject);
                }
                if ((CardValue.attackType & AttackType.Fire) != 0)
                {
                    CombatManager.Instance.DealFireDamage(gameObject);
                }
            }
            if (collision.CompareTag("weaponSprite") && PlayerController.currentAttackMode == AttackMode.Ranged)
            {
                if ((CardValue.attackType & AttackType.NormalRanged) != 0)
                {
                    CombatManager.Instance.DealDamage(gameObject);
                }
            }
            if(collision.CompareTag("PlayerFire"))
            {
                if ((CardValue.attackType & AttackType.PlayerFire) != 0)
                {
                    CombatManager.Instance.DealPlayerFireDamage(gameObject);
                }
            }
            if (collision.CompareTag("Fireball"))
            {
                CombatManager.Instance.DealFireballDamage(gameObject);
            }
            if (collision.CompareTag("Lighting"))
            {
                CombatManager.Instance.DealLightingDamage(gameObject);
            }
            if (collision.CompareTag("OneLight"))
            {
                CombatManager.Instance.DealOneLightDamage(gameObject);
            }
            if (collision.CompareTag("SummonTrigger"))
            {
                CombatManager.Instance.DealDamage(gameObject);
            }
            Vector2 dir = (transform.position - player.transform.position).normalized;
            float kb = collision.CompareTag("weaponSprite") ? knockbackDistance : 0f;
            float duration = 0.2f;
            StartCoroutine(Knockback(dir, kb, duration));
        }
    }
    public void DealBurningDamage(GameObject target, int tickCount = 4, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseAtk = playerValue.GetBaseAttack();
            int burnDamage = Mathf.CeilToInt(baseAtk * 0.4f); // 初始伤害为攻击力的40%

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, burnDamage, tickCount, tickInterval));
        }
    }

    private IEnumerator ApplyBurning(IHurtable hurtable, int startDamage, int tickCount, float interval)
    {
        int damage = startDamage;

        for (int i = 0; i < tickCount; i++)
        {
            if (hurtable == null) yield break;

            hurtable.TakeDamage(damage, false); // 火焰伤害不能暴击
            damage = Mathf.Max(1, damage - 1);  // 每跳递减，最少为1

            yield return new WaitForSeconds(interval);
        }
    }
    public virtual void Stun(float duration)
    {
        if (isDead || duration <= 0f) return;

        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.enabled = false;

        // 可选：播放眩晕动画或粒子特效
        ShowStunEffect(true);
    }
    protected virtual void ShowStunEffect(bool show)
    {
        // 示例：你可以在敌人头上显示眩晕标志
        //Transform effect = transform.Find("StunEffect");
        //if (effect != null) effect.gameObject.SetActive(show);
    }
}