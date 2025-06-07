using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitAttackEnemy : EnemyManager
{
    [Header("攻击设置")]
    public float orbitStartDistance = 3f;   // 进入旋转的距离阈值
    public float orbitRadius = 4f;
    public float orbitSpeed = 90f;          // 每秒角度
    public float attackInterval = 2f;
    public float attackRange = 1.5f;
    public GameObject attackPivot;
    public float randomRadiusOffset = 0.3f;
    public float randomAngleJitter = 15f;

    private float currentAngle;
    private float attackTimer;
    private bool ifagent = false;
    protected override void Start()
    {
        base.Start();
        currentAngle = Random.Range(0f, 360f);
    }

    protected override void Update()
    {
        base.Update();

        if (currentTarget == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (distanceToTarget < orbitStartDistance)
        {
            ifagent = true;
            ifattacking = true;

            // 重置速度和路径，避免冲刺
            agent.velocity = Vector3.zero;
            agent.enabled = false;

            OrbitAroundPlayer();

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackInterval;
            }
        }
        else
        {
            ResetAgent();
            agent.enabled = true;
            ifattacking = false;
        }
        attackPivot.transform.localScale = -transform.localScale;
    }
    void ResetAgent()
    {
        if(ifagent)
        {
            agent.Warp(transform .position );
            ifagent = false;
        }
    }
    void OrbitAroundPlayer()
    {
        float radiusOffset = Random.Range(-randomRadiusOffset, randomRadiusOffset);
        float jitter = Random.Range(-randomAngleJitter, randomAngleJitter) * Time.deltaTime;

        currentAngle += (orbitSpeed + jitter) * Time.deltaTime;
        float angleRad = currentAngle * Mathf.Deg2Rad;
        float effectiveRadius = orbitRadius + radiusOffset;

        // 计算目标位置（全部用 Vector2）
        Vector2 targetCenter = currentTarget.position;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * effectiveRadius;
        Vector2 targetPos = targetCenter + offset;

        // 检测路径是否被墙体遮挡
        RaycastHit2D hit = Physics2D.Linecast(transform.position, targetPos, LayerMask.GetMask("Wall"));
        if (hit.collider == null)
        {
            // 无障碍，缓动移动
            Vector2 newPos = Vector2.Lerp((Vector2)transform.position, targetPos, Time.deltaTime * 5f);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            // 有墙体，向反方向轻微偏移
            Vector2 safeDir = ((Vector2)transform.position - hit.point).normalized;
            Vector2 newPos = (Vector2)transform.position + safeDir * Time.deltaTime;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }

        FaceTarget(currentTarget);
    }

    void Attack()
    {
        Debug.Log("敌人攻击玩家！");
        if (attackArea && currentTarget)
        {
            // 让攻击轴（attackPivot）朝向目标
            Vector2 direction = currentTarget.position - attackPivot.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            attackPivot.transform.rotation = Quaternion.Euler(0, 0, angle);
            StartCoroutine(ActivateAttackArea());
        }
    }
    IEnumerator ActivateAttackArea()
    {
        attackArea.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 攻击有效时间，0.2 秒可调整
        attackArea.SetActive(false);
    }
}