using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Attack : MonoBehaviour
{
    [Header("Movement")]
    public GameObject Face;

    public float walkSpeed = 5f;
    public float acceleration = 20f;
    [SerializeField] private float moveThreshold = 0.01f; // 静止阈值

    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public Vector2 FaceVector;
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
    public float attackCooldown = 0.3f;
    private bool canAttack = true;

    [Header("Summon")]
    private bool isCharging = false;
    private float chargeTime = 0f;
    public float maxChargeTime = 2f;
    public float minEffectiveChargeTime = 0.5f;

    public GameObject summonPrefab;
    public float summonDuration = 5f;
    public float summonCooldown = 15f;
    private bool canSummon = true;

    [Header("Block")]
    public GameObject shield;

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
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (canMove) HandleMovement();
        HandleCombat();
        HandleTargetLock();
        HandleBlock();
    }
    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;    
            currentVelocity = Vector2.zero;
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
            rb.velocity = moveInput * walkSpeed;
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
    void HandleMovement()
    {
        if (!isDashing)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
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
            isCharging = true;
            chargeTime = 0f;
            canMove = false;
            canAttack = false;
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxChargeTime); // 限制最大蓄力时间
        }

        // 松开左键，触发蓄力攻击
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            float chargePercent = 0;
            bool isCharged = chargeTime >= minEffectiveChargeTime;
            if(isCharged)
            {
                chargePercent = chargeTime / maxChargeTime;
            }
            else
            {
                chargePercent = 0;
            }
            StartCoroutine(MeleeAttack(chargePercent));
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
        yield return new WaitForSeconds(attackCooldown);
        canMove = true;
        canAttack = true;
    }

    IEnumerator RangedAttack()
    {
        canAttack = false;
        canMove = false;
        Vector3 mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        GameObject knife = Instantiate(rangedWeaponPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, mouseDir));
        knife.GetComponent<RangedKnife>().Launch(mouseDir,this.transform);
        yield return new WaitForSeconds(attackCooldown);
        canMove = true;
    }

    IEnumerator SummonAttack()
    {
        canSummon = false;
        canMove = false;
        GameObject summon = Instantiate(summonPrefab, transform.position, Quaternion.identity);
        // summon.GetComponent<SummonAI>().Initialize(this.transform);
        yield return new WaitForSeconds(summonDuration);
        Destroy(summon);
        yield return new WaitForSeconds(summonCooldown - summonDuration);
        canMove = true;
        canSummon = true;
    }

    void HandleBlock()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            shield.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Space))
            shield.SetActive(false);
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
