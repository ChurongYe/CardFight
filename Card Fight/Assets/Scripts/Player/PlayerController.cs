using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private GameObject Face;
    private float walkSpeed = 7f;
    private float acceleration = 80f;
    private float moveThreshold = 0.01f; // 静止阈值

    private float dashSpeed = 25f;
    private float dashCooldown = 1f;
    private float dashDuration = 0.2f;

    private Vector2 FaceVector;//冲刺面向
    private Vector3 mouseDir;

    private Vector2 currentVelocity;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool canMove = true;

    private bool isDashing = false;
    private bool canDash = true;

    [Header("Combat")]
    public Weapon weapon;
    public GameObject rangedWeaponPrefab;
    private float attackCooldownMelee = 0.05f;
    private float attackCooldownRanged = 0.3f;
    private float reducedMoveSpeed = 2f; 
    private float originalMoveSpeed;
    private bool canAttack = true;
    bool ifCharge = false ;
    private float chargeTime = 0f;
    private float maxChargeTime = 2f;
    private float minEffectiveChargeTime = 0.5f;

    [Header("Summon")]
    public GameObject summonPrefab;
    private float summonDuration = 7f;
    private float summonCooldown = 15f;
    private bool canSummon = true;

    [Header("Block")]
    public GameObject shield;
    bool ifBlock = false;

    [Header("Targeting")]
    private Transform currentTarget;
    private bool isTargetLocked;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isInvincible;

    [Header("Animator")]
    private Animator playerAnimator;
    void Start()
    {
        Face = GameObject.FindWithTag("Face");
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        playerAnimator = GetComponent<Animator>();
        originalMoveSpeed = walkSpeed;
    }

    void Update()
    {
        if (canMove) HandleMovement();
        HandleCombat();
        HandleTargetLock();
        HandleBlock();
    }
    //封装数值
    #region Move
    public float WalkSpeed
    {
        get { return walkSpeed; }
        set { walkSpeed = value; }
    }
    public float DashSpeed
    {
        get { return dashSpeed; }
        set { dashSpeed = value; }
    }
    public float DashCooldown
    {
        get { return dashCooldown; }
        set { dashCooldown = value; }
    }
    public float ReducedMoveSpeed
    {
        get { return reducedMoveSpeed; }
        set { reducedMoveSpeed = value; }
    }
    #endregion
    #region Attack
    public float MaxChargeTime
    {
        get { return maxChargeTime; }
        set { maxChargeTime = value; }
    }
    public float SummonDuration
    {
        get { return summonDuration; }
        set { summonDuration = value; }
    }
    public float SummonCooldown
    {
        get { return summonCooldown; }
        set { summonCooldown = value; }
    }
    #endregion

    void FixedUpdate()
    {
        if (!canMove)
        {
            float decelerationSpeed = 30f;
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, decelerationSpeed * Time.deltaTime);
            currentVelocity = rb.velocity;
            moveInput = Vector2.zero;
            return;
        }
        if (moveInput != Vector2.zero)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, moveInput * walkSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.zero;
        }

        if (!isDashing)
        {
            //rb.velocity = moveInput * walkSpeed;
            rb.velocity = currentVelocity;
        }
    }
    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    public bool CanAttack
    {
        get { return canAttack; }
        set { canAttack = value; }
    }

    Vector2 lastHeldDirectionWS = Vector2.zero;
    Vector2 lastHeldDirectionAD = Vector2.zero;
    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (!isDashing)
        {
            // 自动判断最后一个方向键
            if (Input.GetKeyDown(KeyCode.A)) lastHeldDirectionAD = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D)) lastHeldDirectionAD = Vector2.right;

            if (Input.GetKeyDown(KeyCode.W)) lastHeldDirectionWS = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S)) lastHeldDirectionWS = Vector2.down;

            float x = 0f;
            float y = 0f;

            // 处理左右输入
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) x = -1;
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) x = 1;
            else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) x = lastHeldDirectionAD.x;

            // 处理上下输入
            if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) y = 1;
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) y = -1;
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) y = lastHeldDirectionWS.y;

            Vector2 rawInput = new Vector2(x, y);
            moveInput = rawInput.normalized;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && moveInput.magnitude > moveThreshold)
        {
            FaceVector = moveInput;
            StartCoroutine(Dash(FaceVector));
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && moveInput.magnitude <= moveThreshold)
        {
            FaceVector = mouseDir.normalized;
            StartCoroutine(Dash(FaceVector));
        }
    }
    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        isInvincible = true;

        rb.velocity = direction * dashSpeed;

        // 可加：播放特效或动画
        yield return new WaitForSeconds(dashDuration);

        isInvincible = false;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleCombat()
    {
        mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - Face.transform.position);
        mouseDir.z = 0;
        if (!isTargetLocked)
            Face.transform.right = mouseDir;
        else if (currentTarget)
            Face.transform.right = (currentTarget.position - Face.transform.position);


        // 动画参数
        playerAnimator.SetFloat("MoveX", moveInput.x);
        playerAnimator.SetFloat("MoveY", moveInput.y);
        playerAnimator.SetFloat("Speed", moveInput.magnitude);

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            chargeTime = 0f;
            canAttack = false;
        }

        if (Input.GetMouseButton(0))
        {
            chargeTime += Time.deltaTime;
            if (chargeTime >= minEffectiveChargeTime)
            {
                // 蓄力时减速
                walkSpeed = reducedMoveSpeed;
            }
            chargeTime = Mathf.Min(chargeTime, maxChargeTime); // 限制最大蓄力时间
        }

        // 松开左键，触发(蓄力)攻击
        if (Input.GetMouseButtonUp(0))
        {
            float chargePercent = 0;
            bool isCharged = chargeTime >= minEffectiveChargeTime;
            if (isCharged)
            {
                ifCharge = true;
                chargePercent = chargeTime / maxChargeTime;
            }
            else
            {
                ifCharge = false;
                chargePercent = 0;
            }
            StartCoroutine(MeleeAttack(chargePercent));
            ifCharge = false;
            // 蓄力结束后恢复原本的速度
            if (ifBlock) return; walkSpeed = originalMoveSpeed;
        }

        if (Input.GetMouseButtonDown(1) && canAttack)
        {
            StartCoroutine(RangedAttack());
        }

        if (Input.GetKeyDown(KeyCode.Q) && canSummon)
        {
            StartCoroutine(SummonAttack());
        }
    }

    IEnumerator MeleeAttack(float chargePercent)
    {

        Debug.Log("攻击释放，蓄力百分比：" + chargePercent);
        // 使用 chargePercent 控制范围与力度
        weapon.TrySwing(chargePercent);
        // 攻击动画或冲击力特效也可根据 chargePercent 来变化
        yield return new WaitForSeconds(attackCooldownMelee);
        canAttack = true;
    }

    IEnumerator RangedAttack()
    {
        canAttack = false;
        Vector3 mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        GameObject knife = Instantiate(rangedWeaponPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, mouseDir));
        knife.GetComponent<RangedKnife>().Launch(mouseDir, this.transform);
        yield return new WaitForSeconds(attackCooldownRanged);
    }

    IEnumerator SummonAttack()
    {
        canSummon = false;
        canMove = false;
        //召唤动画时间
        yield return new WaitForSeconds(1f);
        float radius = 3f;
        Vector2 offset = Random.insideUnitCircle.normalized * radius;
        Vector3 summonPosition = transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject summon = Instantiate(summonPrefab, summonPosition, Quaternion.identity);
        canMove = true;

        yield return new WaitForSeconds(summonDuration);

        Destroy(summon);

        yield return new WaitForSeconds(summonCooldown - summonDuration);

        canSummon = true;
    }

    void HandleBlock()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ifBlock = true;
            shield.SetActive(true);
            walkSpeed = reducedMoveSpeed;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ifBlock = false;
            shield.SetActive(false);
            if (ifCharge) return; walkSpeed = originalMoveSpeed;
        }
    }

    void HandleTargetLock()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isTargetLocked)
            {
                currentTarget = null;
                isTargetLocked = false;
            }
            else
            {
                GameObject nearest = FindNearestEnemy();
                if (nearest)
                {
                    currentTarget = nearest.transform;
                    isTargetLocked = true;
                }
            }
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                nearest = e;
                minDist = dist;
            }
        }
        return nearest;
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        StartCoroutine(HurtRoutine());
        if (currentHealth <= 0) Die();
    }
    public bool IsInvincible()
    {
        return isInvincible;
    }
    IEnumerator HurtRoutine()
    {
        canMove = false;
        isInvincible = true;
        rb.velocity = Vector2.zero;
        // TODO: Play hurt animation + show red outline
        yield return new WaitForSeconds(0.5f); // 无敌帧时长
        canMove = true;
        isInvincible = false;
    }

    void Die()
    {
        // TODO: Play death animation, disable controls, etc.
        Debug.Log("Player Died");
        //gameObject.SetActive(false);
    }

}