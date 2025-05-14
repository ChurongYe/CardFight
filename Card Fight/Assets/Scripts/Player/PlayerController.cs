using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private GameObject Face;
    private float walkSpeed = 10f;
    private float acceleration = 80f;
    private float moveThreshold = 0.01f; // ��ֹ��ֵ

    private float dashSpeed = 25f;
    private float dashCooldown = 1f;
    private float dashDuration = 0.2f;

    private Vector2 FaceVector;//�������
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
    private float attackCooldownMelee = 0.2f;
    public float attackCooldownRanged = 0.3f;
    private float reducedMoveSpeed = 2f; 
    private float originalMoveSpeed;
    private bool canAttack = true;
    bool ifCharge = false ;
    bool ifClick = true;
    private bool ifAttack = false;
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
    //��װ��ֵ
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
    public float AttackCooldownMelee
    {
        get { return attackCooldownMelee; }
        set { attackCooldownMelee = value; }
    }
    #endregion

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
            // �Զ��ж����һ�������
            if (Input.GetKeyDown(KeyCode.A)) lastHeldDirectionAD = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D)) lastHeldDirectionAD = Vector2.right;

            if (Input.GetKeyDown(KeyCode.W)) lastHeldDirectionWS = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S)) lastHeldDirectionWS = Vector2.down;

            float x = 0f;
            float y = 0f;

            // ������������
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) x = -1;
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) x = 1;
            else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) x = lastHeldDirectionAD.x;

            // ������������
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

        rb.velocity = direction * dashSpeed;

        // �ɼӣ�������Ч�򶯻�
        yield return new WaitForSeconds(dashDuration);

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


        // ��������
        playerAnimator.SetFloat("MoveX", moveInput.x);
        playerAnimator.SetFloat("MoveY", moveInput.y);
        playerAnimator.SetFloat("Speed", moveInput.magnitude);

        if (Input.GetMouseButtonDown(0) && ifClick)
        {
            canMove = false;
            chargeTime = 0f;
            ifAttack = true;
            ifClick = false;
        }

        if (Input.GetMouseButton(0) && ifAttack)
        {
            chargeTime += Time.deltaTime;
            if (chargeTime >= minEffectiveChargeTime)
            {
                // ����ʱ����
                canMove = true;
                walkSpeed = reducedMoveSpeed;
            }
            chargeTime = Mathf.Min(chargeTime, maxChargeTime); // �����������ʱ��
        }

        // �ɿ����������(����)����
        if (Input.GetMouseButtonUp(0) && canAttack)
        {
            ifClick = true;
            canMove = false;
            ifAttack = false;
            canAttack = false;
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
        ifCharge = false;
        ifAttack = false;
        canAttack = false;
        float stoptime = 0.2f;
        Debug.Log("�����ͷţ������ٷֱȣ�" + chargePercent);
        // ʹ�� chargePercent ���Ʒ�Χ������
        weapon.TrySwing(chargePercent);
        yield return new WaitForSeconds(stoptime);
        canMove = true;
        if (!ifBlock) walkSpeed = originalMoveSpeed;
        // ����������������ЧҲ�ɸ��� chargePercent ���仯
        yield return new WaitForSeconds(attackCooldownMelee );
        canAttack = true;
        // ����������ָ�ԭ�����ٶ�
    }

    IEnumerator RangedAttack()
    {
        CantMove(0.3f);
        canAttack = false;
        Vector3 mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        GameObject knife = Instantiate(rangedWeaponPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, mouseDir));
        knife.GetComponent<RangedKnife>().Launch(mouseDir, this.transform);
        yield return new WaitForSeconds(attackCooldownRanged);
    }

    IEnumerator SummonAttack()
    {
        canSummon = false;
        CantMove(0.5f);
        //�ٻ�����ʱ��
        yield return new WaitForSeconds(1f);
        float radius = 3f;
        Vector2 offset = Random.insideUnitCircle.normalized * radius;
        Vector3 summonPosition = transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject summon = Instantiate(summonPrefab, summonPosition, Quaternion.identity);

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
        CantMove(0.5f);
        isInvincible = true;
        rb.velocity = Vector2.zero;
        // TODO: Play hurt animation + show red outline
        yield return new WaitForSeconds(0.5f); // �޵�֡ʱ��
        isInvincible = false;
    }

    void Die()
    {
        // TODO: Play death animation, disable controls, etc.
        Debug.Log("Player Died");
        //gameObject.SetActive(false);
    }

}