using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shibie : MonoBehaviour
{
    private AllValue allValue;
    public GameObject attackArea;
    public float AttackTime = 0.5f;
    public float attackCooldown = 1.5f;
    public int maxHP = 3;
    private HurtUI hurtUI;
    public float DieTime = 0.5f;
    private int currentHP;
    bool ifDie = false;
    private GameObject bar;
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
        allValue = FindObjectOfType<AllValue >();
        Player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent2D>();
        currentHP = maxHP;
        //血条
        GameObject barPrefab = Resources.Load<GameObject>("EnemyHealthBar");
        if (barPrefab != null)
        {
            bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0); // 头顶位置
            hurtUI = bar.GetComponent<HurtUI>();
            hurtUI.UpdateHealthBar(currentHP, maxHP);
        }
        else
        {
            Debug.LogError("找不到 EnemyHealthBar 预制体，请确保它在 Resources 文件夹内！");
        }

    }

    void Update()
    {
        knockbackDistance = allValue.impactForce;
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
        bar.transform.localScale = transform.localScale;
    }

    void MoveToTargetWithStopNearEdge(Transform target)
    {
        if (ifDie) return;
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
            FaceTarget(currentTarget);
            agent.destination = enemyApproachPoint;
        }
        if (dist <= stopThreshold)
        {
            agent.destination = transform.position; // 停止移动
            TryAttack();
        }
        // 如果靠近目标玩家则攻击
        if (target.CompareTag("Player") && Vector2.Distance(transform.position, target.position) < 0.8f)
        {
            agent.destination = transform.position; // 停止移动
            TryAttack();
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
    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
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
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine(AttackTime));
        }
    }

    IEnumerator AttackRoutine(float time)
    {
        isAttacking = true;

        // 攻击前摇阶段（动画播放）
        yield return new WaitForSeconds(0.5f); // 攻击准备时间（前摇）

        if (attackArea != null)
            attackArea.SetActive(true); // 实际造成伤害的阶段

        yield return new WaitForSeconds(time); // 持续攻击时间

        if (attackArea != null)
            attackArea.SetActive(false); // 攻击结束

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

            // 扣血
            int damage = allValue.PlayerDamage;
            currentHP -= damage;
            if (hurtUI != null)
                hurtUI.UpdateHealthBar(currentHP, maxHP);

            if (currentHP <= 0)
            {
                StartCoroutine(Die());// 死亡处理
                return;
            }

            // 击退效果
            Vector2 direction = (transform.position - Player.transform.position).normalized;
            float knockbackDuration = 0.2f;

            StartCoroutine(Knockback(direction, knockbackDistance, knockbackDuration));
        }
        else if (collision.gameObject.CompareTag("SummonTrigger") && !isAttacked)
        {
            isAttacked = true;

            // 扣血
            int damage = allValue.PlayerDamage;
            currentHP -= damage;
            if (hurtUI != null)
                hurtUI.UpdateHealthBar(currentHP, maxHP);

            if (currentHP <= 0)
            {
                StartCoroutine(Die());// 死亡处理
                return;
            }

            // 击退效果
            Vector2 direction = (transform.position - Player.transform.position).normalized;
            float knockbackDuration = 0.2f;

            StartCoroutine(Knockback(direction, 0, knockbackDuration));
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
    IEnumerator Die()
    {
        ifDie = true;
        agent.destination = transform.position; // 停止移动
        GetComponent<Collider2D>().enabled = false;
        // 死亡动画、掉落物品等
        yield return new WaitForSeconds(DieTime);
        Debug.Log(gameObject.name + " 死亡");
        Destroy(gameObject);
    }
}