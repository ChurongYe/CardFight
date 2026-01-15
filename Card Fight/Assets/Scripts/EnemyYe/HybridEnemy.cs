using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridEnemy : EnemyManager
{
    [Header("通用设置")]
    public float attackCooldown = 2f;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    [Header("近战攻击设置")]
    public float meleeAttackTime = 0.5f;
    public float meleeSwingAngle = 185f;
    public GameObject WeaponPivot;

    [Header("跳跃攻击设置")]
    public float jumpDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float jumpDistance = 2f;
    public float smashRange = 1.5f;
    public LayerMask obstacleLayer;

    [Header("瞬移设置")]
    public float teleportDistance = 8f;     // 玩家离敌人超过这个距离才瞬移
    public float teleportOffset = 2f;       // 瞬移后离玩家的随机偏移

    protected override void Start()
    {
        base.Start();
        currentHP = maxHP;
    }


    protected override void Update()
    {
        base.Update();

        if (currentTarget == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);
        // === 1. 瞬移逻辑（优先级最高）===
        if (distanceToPlayer > teleportDistance)
        {
            StartCoroutine(TeleportNearPlayer());
            return;
        }

        // === 2. 攻击冷却 ===
        if (Time.time - lastAttackTime < attackCooldown) return;

        // === 3. 攻击模式选择 ===
        float rand = Random.value;

        if (currentHP > maxHP * 0.5f)
        {
            // 血量 > 50%：近战 / 跳跃 各 50%
            if (rand < 0.5f) StartCoroutine(MeleeAttack());
            else StartCoroutine(JumpAndSmash());
        }
        else
        {
            // 血量 ≤ 50%：跳跃 70%，近战 30%
            if (rand < 0.7f) StartCoroutine(JumpAndSmash());
            else StartCoroutine(MeleeAttack());
        }
    }

    // ========== 近战攻击 ==========
    private IEnumerator MeleeAttack()
    {
        IfneedWalk = true;
        agent.enabled = true;
        isAttacking = true;
        ifattacking = true;
        lastAttackTime = Time.time;

        FaceTarget(currentTarget);
        Vector3 center = WeaponPivot.transform.position;

        //float timer = 0f;
        bool attackRight = currentTarget.position.x >= transform.position.x;
        animator.SetTrigger("IsAttacking");
        yield return new WaitForSeconds(0.8f);
        if (attackArea != null) attackArea.SetActive(true);
        //while (timer < meleeAttackTime)
        //{
        //    timer += Time.deltaTime;
        //    float t = Mathf.Clamp01(timer / meleeAttackTime);
        //    float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

        //    float currentAngle;
        //    if (attackRight)
        //        currentAngle = Mathf.Lerp(meleeSwingAngle / 2f, -meleeSwingAngle / 2f, easedT);
        //    else
        //        currentAngle = Mathf.Lerp(-meleeSwingAngle / 2f, -meleeSwingAngle / 2f + 180, easedT);

        //    Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        //    Vector3 offset = attackRight ? rotation * Vector3.right : rotation * Vector3.left;
        //    Vector3 rotatedPos = center + offset;

        //    if (attackArea != null)
        //    {
        //        attackArea.transform.position = rotatedPos;
        //        attackArea.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        //    }
        //    yield return null;
        //}
        yield return new WaitForSeconds(0.5f);
        if (attackArea != null) attackArea.SetActive(false);
        isAttacking = false;
        ifattacking = false;
    }

    // ========== 跳跃 + 砸击 ==========
    private IEnumerator JumpAndSmash()
    {
        IfneedWalk = false;
        agent.enabled = false;
        isAttacking = true;
        ifattacking = true;
        lastAttackTime = Time.time;

        FaceTarget(currentTarget);
        animator.SetTrigger("IsJump");
        yield return new WaitForSeconds(0.1f);
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
        // 落地砸击
        if (attackArea != null)
        {
            attackArea.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            attackArea.SetActive(false);
        }

        isAttacking = false;
        ifattacking = false;
    }

    private Vector2 FindJumpTarget()
    {
        Vector2 toPlayer = currentTarget.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer <= smashRange + 0.5f)
        {
            Vector2 preciseTarget = (Vector2)currentTarget.position + new Vector2(0, 0.2f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distanceToPlayer, obstacleLayer);
            if (hit.collider == null) return preciseTarget;
        }

        return (Vector2)transform.position + toPlayer.normalized * jumpDistance;
    }

    // ========== 瞬移 ==========
    private IEnumerator TeleportNearPlayer()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1f); // 瞬移前的蓄力动画时间（可替换成特效）
        animator.SetTrigger("Teleport");
        Vector2 randomOffset = Random.insideUnitCircle.normalized * teleportOffset;
        transform.position = currentTarget.position + (Vector3)randomOffset;

        isAttacking = false;
        agent.Warp(transform.position);// 强制同步 agent 的位置
    }
}