using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Core.PlayerValue playerValue;
    private enum AttackMode { Melee, Ranged }
    private AttackMode currentAttackMode = AttackMode.Melee;
    [Header("Movement")]
    private GameObject Face;

    private float walkSpeed = 10f;//*
    public float acceleration = 80f;
    private float moveThreshold = 0.01f; // 静止阈值

    public float dashSpeed = 25f;
    //private float dashCooldown = 1f;//*
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
    //private float attackCooldown = 0.3f;//*
    private float reducedMoveSpeed = 2f;
    private bool isSlowed = false;
    public float impactForce = 1f;
    public bool ifclear = false; //净化

    private bool canAttack = true;
    private Transform currentTarget;
    private bool Attacking = false;
    private bool wasMovingLastFrame = false;
    private bool shouldRefreshTarget = false;

    [Header("Health")]
    //private int playerHealth = 10;//*
    //private int currentHealth; //*
    private bool isInvincible;

    [Header("UI")]
    public Image HealthfillImage;

    [Header("Animator")]
    private Animator playerAnimator;
    private bool ifAttacking = false;
    private bool AorR = true ;
    void Start()
    {
        playerValue = FindObjectOfType<Core.PlayerValue>();
        Face = GameObject.FindWithTag("Face");
        rb = GetComponent<Rigidbody2D>();
        playerValue.OnMoveSpeedChanged += speed => walkSpeed = speed;
        playerAnimator = GetComponent<Animator>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            currentAttackMode = currentAttackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
            AorR = currentAttackMode == AttackMode.Melee ? true : false;
        }
        if (canMove)
        {
            HandleMovement();
        }
        HandleCombat();
        PlayerAnimation();
        UpdateHealth(playerValue.currentHP, playerValue.currentMaxHP);
    }
    public void UpdateHealth(float currentHP, float maxHP)
    {
        HealthfillImage.fillAmount = currentHP / maxHP;
    }
    void FixedUpdate()
    {
        if (!canMove)
        {
            float decelerationSpeed = 2000f;
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, decelerationSpeed * Time.deltaTime);
            currentVelocity = rb.velocity;
            moveInput = Vector2.zero;
            return;
        }
        if (moveInput != Vector2.zero)
        {
            ifAttacking = false;
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
    void PlayerAnimation()
    {
        if (playerAnimator == null)
        {
            Debug.LogError("playerAnimator 是 null！");
            return;
        }
        // 动画参数
        playerAnimator.SetFloat("MoveX", moveInput.x);
        playerAnimator.SetFloat("MoveY", moveInput.y);
        playerAnimator.SetFloat("Speed", moveInput.magnitude);
        playerAnimator.SetBool("Attack", ifAttacking);
        playerAnimator.SetBool("AorR", AorR);
    }
    public void CantMove(float time)
    {
        canMove = false;
        float decelerationSpeed = 2000f;
        rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, decelerationSpeed * Time.deltaTime);
        currentVelocity = rb.velocity;
        moveInput = Vector2.zero;
        StartCoroutine(Stoptime(time));
    }
    IEnumerator Stoptime(float time)
    {
        yield return new WaitForSeconds(time);
        canMove = true;
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
        bool isCurrentlyMoving = moveInput.magnitude > moveThreshold;

        if (wasMovingLastFrame && !isCurrentlyMoving)
        {
            Attacking = false;
            shouldRefreshTarget = true; //只有这时才触发目标刷新
        }
        wasMovingLastFrame = isCurrentlyMoving;
    }
    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        rb.velocity = direction *  dashSpeed;


        // 可加：播放特效或动画
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        float cooldownElapsed = 0f;
        while (cooldownElapsed < playerValue.currentDashCooldown)
        {
            cooldownElapsed += Time.deltaTime;
            yield return null;
        }
        canDash = true;
    }
    public void ApplySlow(float slowDuration) //减速效果
    {
        if (isSlowed || ifclear) return;
        StartCoroutine(SlowCoroutine(slowDuration));
    }

    IEnumerator SlowCoroutine(float slowDuration)
    {
        float originalMoveSpeed = walkSpeed;
        isSlowed = true;
        walkSpeed = reducedMoveSpeed;

        yield return new WaitForSeconds(slowDuration);

        walkSpeed = originalMoveSpeed;
        isSlowed = false;
    }
    void HandleCombat()
    {
        if (!Attacking)
        {
            mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - Face.transform.position);
            mouseDir.z = 0;
            Face.transform.right = mouseDir;
        }
        if (shouldRefreshTarget || currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            GameObject newTarget = FindNearestEnemy();
            if (newTarget != null)
            {
                currentTarget = newTarget.transform;
            }

            shouldRefreshTarget = false;
        }
        if (canAttack && moveInput.magnitude < moveThreshold)
        {
            // 如果没有目标或目标已死亡，则寻找新目标
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                GameObject newTarget = FindNearestEnemy();
                if (newTarget != null)
                {
                    currentTarget = newTarget.transform;
                }
            }

            if (currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, currentTarget.position);
                float attackRange = currentAttackMode == AttackMode.Melee ? 2f : 100f;

                if (distance <= attackRange)
                {
                    Face.transform.right = (currentTarget.position - Face.transform.position);

                    if (currentAttackMode == AttackMode.Melee)
                    {
                        StartCoroutine(MeleeAttack());
                    }
                    else
                    {
                        StartCoroutine(RangedAttack());
                    }
                }
            }
        }
    }
    IEnumerator MeleeAttack()
    {
        ifAttacking = true;
        Attacking = true;
        canAttack = false;
        if (currentTarget == null) yield break;

        weapon.TrySwing(); // 近战攻击逻辑
        yield return new WaitForSeconds(playerValue.currentAttackSpeed);
        canAttack = true;
        ifAttacking = false;
    }

    IEnumerator RangedAttack()
    {
        ifAttacking = true;
        Attacking = true;
        canAttack = false;

        if (currentTarget == null) yield break;

        Vector2 dirToTarget = (currentTarget.position - transform.position).normalized;

        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject knife = Instantiate(rangedWeaponPrefab, transform.position, rotation);
        knife.GetComponent<RangedKnife>().Launch(dirToTarget);

        yield return new WaitForSeconds(playerValue.currentAttackSpeed * 2.45f);
        canAttack = true;
        ifAttacking = false;
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

        playerValue.currentHP -= amount;

        StartCoroutine(HurtRoutine());
        if (playerValue.currentHP <= 0) StartCoroutine(Die());
    }
    public bool IsInvincible()
    {
        return isInvincible;
    }
    IEnumerator HurtRoutine()
    {
        CantMove(0.5f);
        isInvincible = true;
        rb.velocity = Vector2.zero;
        // 受伤动画
        yield return new WaitForSeconds(0.5f); // 无敌帧时长
        isInvincible = false;
    }
    IEnumerator Die()
    {
        //死亡动画
        yield return new WaitForSeconds(1f);
        Debug.Log("Player Died");
    }

}