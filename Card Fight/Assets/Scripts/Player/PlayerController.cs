using Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static PlayerController;
using static Unity.Collections.AllocatorManager;
using static Unity.VisualScripting.Member;

public enum AttackDirection
{
    Up,
    Down,
    Left,
    Right
}
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Core.PlayerValue playerValue;
    public enum AttackMode { Melee, Ranged }
    public static AttackMode currentAttackMode = AttackMode.Melee;
    public SkillEffectConfig effectConfig;
    public Transform meleeEffectPoint;
    public Transform rangedEffectPoint;

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
    public Vector2 moveInput;
    private Rigidbody2D rb;
    private bool canMove = true;

    private bool isDashing = false;
    private bool canDash = true;
    private bool isKnockbacking = false;
    private float lastHorizontal = 1f;

    [Header("Combat")]
    public Weapon weapon;
    public GameObject rangedWeaponPrefab;
    //private float attackCooldown = 0.3f;//*
    private float reducedMoveSpeed = 2f;
    private bool isSlowed = false;
    public float impactForce = 1f;
    public bool ifclear = false; //净化

    [SerializeField] private bool canAttack = true;
    [SerializeField] private Transform currentTarget;
    private bool Attacking = false;
    private bool wasMovingLastFrame = false;
    private bool shouldRefreshTarget = false;
    public AttackDirection currentAttackDirection;

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

    private SpriteRenderer spriteRenderer;
    [Header("CardFire")]
    public GameObject PlayerFire;
    public Camera mainCamera; // 主摄像机
    public GameObject fireballPrefab;
    public float fireballCooldownTime = 6f;// 每个火球间隔时间
    private float fireballCooldownTimer = 0f;
    private bool isCoolingDown = false;
    //public int fireballCount = 5;

    [Header("Light")]
    public GameObject lightningBeamPrefab;
    bool isLightningCoolingDown = false;
    float lightningCooldownTimer = 0f;
    float lightningCooldownTime = 0f;

    [Header("Shield")]
    private GameObject bar;    // 盾牌量预制体
    private HurtUI hurtUI;
    private int currentMaxShield;

    void Start()
    {
        playerValue = FindObjectOfType<Core.PlayerValue>();
        Face = GameObject.FindWithTag("Face");
        rb = GetComponent<Rigidbody2D>();
        playerValue.OnMoveSpeedChanged += speed => walkSpeed = speed;
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(currentAttackMode != AttackMode.Melee)
        {
            weapon.DisableAllHitboxesImmediate(); //避免转变碰撞体没有消失
        }
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
        UpdateHealth(PlayerValue.currentHP, playerValue.currentMaxHP);
        //
        Fire();
        Lighting();
        if (PlayerValue.currentShield <= 0)
        {
            Destroy(bar, 0.5f);
            // TODO: 护盾破裂视觉效果
        }
    }
    public bool Ifball = false;
    void Fire()
    {
        if(ifAttacking && CardValue.PlayerFire && currentAttackMode == AttackMode.Melee)
        {
            PlayerFire.SetActive(true);
        }
        else
        {
            PlayerFire.SetActive(false );
        }
        if (CardValue.fireball && ifAttacking && !Ifball && !isCoolingDown && currentAttackMode == AttackMode.Melee)
        {
            TrySummonFireballs();
        }
        // 攻击时才推进冷却
        if (isCoolingDown && ifAttacking && currentAttackMode == AttackMode.Melee)
        {
            fireballCooldownTimer += Time.deltaTime;
            if (fireballCooldownTimer >= fireballCooldownTime)
            {
                fireballCooldownTimer = 0f;
                isCoolingDown = false;
                Ifball = false;
            }
        }
    }

    void Lighting()
    {
        if (CardValue.OneLight && ifAttacking && !isLightningCoolingDown && currentAttackMode == AttackMode.Ranged)
        {
            TryCastLightningBeam();
        }

        if (isLightningCoolingDown && ifAttacking && currentAttackMode == AttackMode.Ranged)
        {
            lightningCooldownTimer += Time.deltaTime;
            if (lightningCooldownTimer >= lightningCooldownTime)
            {
                lightningCooldownTimer = 0f;
                isLightningCoolingDown = false;
            }
        }
    }
    public void UpdateHealth(float currentHP, float maxHP)
    {
        HealthfillImage.fillAmount = currentHP / maxHP;
    }
    void FixedUpdate()
    {
        if (isKnockbacking)
            return; // 正在击退时，不处理其他移动
        if (!canMove)
        {
            float decelerationSpeed = 2000f;
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, decelerationSpeed * Time.deltaTime);
            currentVelocity = rb.velocity;
            moveInput = Vector2.zero;
            return;
        }
        if (moveInput.x != 0)
        {
            lastHorizontal = Mathf.Sign(moveInput.x); // 1：向右，-1：向左
        }
        if (moveInput != Vector2.zero)
        {
            ifAttacking = false;
            canAttack = true;//
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
        // Idle 朝向控制（如果 idle 没有移动时，使用最后方向）
        playerAnimator.SetFloat("IdleDirection", lastHorizontal);
        playerAnimator.SetBool("IfMelee", currentAttackMode == AttackMode.Melee);
        playerAnimator.SetBool("Attack", ifAttacking);
        playerAnimator.SetFloat("AttackX", currentAttackDirection == AttackDirection.Left ? -1 :
                                   currentAttackDirection == AttackDirection.Right ? 1 : 0);
        playerAnimator.SetFloat("AttackY", currentAttackDirection == AttackDirection.Up ? 1 :
                                     currentAttackDirection == AttackDirection.Down ? -1 : 0);
        playerAnimator.SetFloat("AttackSpeedMultiplier", playerValue.currentAttackSpeed);
        playerAnimator.SetBool("AorR", AorR);
    }
    // 动画事件：通用触发
    public void PlayAttackEffect()
    {
        string effectKey = GetCurrentEffectKey();
        Transform spawnPoint = (currentAttackMode == AttackMode.Melee) ? meleeEffectPoint : rangedEffectPoint;

        GameObject prefab = effectConfig.GetEffect(effectKey);
        if (prefab != null)
        {
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            AttackEffect effect = prefab.GetComponent<AttackEffect>();
            if (effect != null)
            {
                effect.SetWeapon(GetComponent<Weapon>());
                effect.SetDirection(currentAttackDirection);
            }
        }
        else
        {
            Debug.LogWarning("特效未找到：" + effectKey);
        }
    }
    // 组合出特效 key
    private string GetCurrentEffectKey()
    {
        string type = currentAttackMode == AttackMode.Melee
            ? (CardValue.FireLevel == 0 ? "Melee_Normal" : "Melee_Fire")
            : (CardValue.AddLighting ? "Ranged_Electric" : "Ranged_Normal");

        string direction = currentAttackDirection.ToString(); // Up / Down / Left / Right

        return $"{type}_{direction}";
    }
    public void CantMove(float time, Vector2 knockbackDirection)
    {
        StartCoroutine(KnockbackAndStop(knockbackDirection, time));
    }
    IEnumerator KnockbackAndStop(Vector2 direction, float stopTime)
    {
        isKnockbacking = true;
        canMove = false;

        float knockbackDuration = 0.1f;
        float knockbackPower = 30f;

        rb.velocity = direction.normalized * knockbackPower;

        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacking = false;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(stopTime - knockbackDuration);

        canMove = true;
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
            else
            {
                currentTarget = null;
            }
            shouldRefreshTarget = false;
        }

        if (canAttack && moveInput.magnitude < moveThreshold)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                GameObject newTarget = FindNearestEnemy();
                if (newTarget != null)
                {
                    currentTarget = newTarget.transform;
                }
                else
                {
                    currentTarget = null;
                }
            }

            if (currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, currentTarget.position);
                float attackRange = currentAttackMode == AttackMode.Melee ? 5f : 100f;

                // **新增射线检测，确保当前目标没被墙挡住**
                Vector2 origin = transform.position;
                Vector2 targetPos = currentTarget.position;
                RaycastHit2D hit = Physics2D.Raycast(origin, targetPos - origin, distance, LayerMask.GetMask("Wall"));

                if (hit.collider != null)
                {
                    // 当前目标被墙挡住，清空目标，下次重新找
                    currentTarget = null;
                    return; // 不攻击，等下一帧找目标
                }

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

        //方向判断
        AttackDirection direction = GetAttackDirection(currentTarget.position);
        currentAttackDirection = direction; // 用于动画事件获取方向

        //前冲
        Vector2 attackDir = (currentTarget.position - transform.position).normalized;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + attackDir * 0.3f;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / 0.1f));
            yield return null;
        }

        playerValue.ResetLifeStealFlag();
        yield return new WaitForSeconds(playerValue.currentAttackSpeed);

        canAttack = true;
        ifAttacking = false;
    }
    private AttackDirection GetAttackDirection(Vector2 targetPos)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? AttackDirection.Right : AttackDirection.Left;
        else
            return dir.y > 0 ? AttackDirection.Up : AttackDirection.Down;
    }

    IEnumerator RangedAttack()
    {
        ifAttacking = true;
        Attacking = true;
        canAttack = false;

        if (currentTarget == null) yield break;

        // 方向判断
        AttackDirection direction = GetAttackDirection(currentTarget.position);
        currentAttackDirection = direction;

        yield return new WaitForSeconds(playerValue.currentAttackSpeed * 3f);

        canAttack = true;
        ifAttacking = false;
    }
    public void FireRangedWeapon()
    {
        if (currentTarget == null) return;

        // 向上偏移生成位置（例如：从角色头部或手部发射）
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        // 从新位置指向目标的方向
        Vector2 dirToTarget = ((Vector2)currentTarget.position - (Vector2)spawnPos).normalized;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject knife = Instantiate(rangedWeaponPrefab, spawnPos, rotation);
        knife.GetComponent<RangedKnife>().Launch(dirToTarget);
        playerValue.ResetLifeStealFlag(); // 吸血逻辑
    }
    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        Vector2 origin = transform.position;

        foreach (GameObject e in enemies)
        {
            Vector2 targetPos = e.transform.position;
            float dist = Vector2.Distance(origin, targetPos);

            if (dist < minDist)
            {
                // 从玩家位置向敌人发射射线，检测墙壁阻挡
                RaycastHit2D hit = Physics2D.Raycast(origin, targetPos - origin, dist, LayerMask.GetMask("Wall"));

                if (hit.collider == null)
                {
                    // 没有碰到墙壁，说明视线通畅
                    nearest = e;
                    minDist = dist;
                }
                else
                {
                    //Debug.Log("wall");
                }
            }
        }

        return nearest;
    }

    public void TakeDamage(int amount, GameObject enemy)
    {
        if (TryTriggerShield(enemy)) return; // 无敌护盾
        if (isInvincible) return;

        // [1] 临时护盾吸收伤害
        if (PlayerValue.currentShield > 0)
        {
            int absorbed = Mathf.Min(PlayerValue.currentShield, amount);
            PlayerValue.currentShield -= absorbed;
            amount -= absorbed;

            hurtUI?.UpdateHealthBar(PlayerValue.currentShield, currentMaxShield);//更新护盾量
            StartCoroutine(HurtRoutineShield());

            if (amount <= 0) return; // 全部伤害被护盾吸收
        }

        // [2] 预测血量是否会低于 5%，如果是，尝试触发低血护盾
        int predictedDamage = Mathf.Max(1, amount - PlayerValue.currentDefense);
        int predictedHP = PlayerValue.currentHP - predictedDamage;
        float predictedPercent = (float)predictedHP / playerValue.currentMaxHP;

        if (predictedPercent <= 0.05f)
        {
            TryTriggerLowHpShield(); // 注意此处调用在实际扣血前
            if (PlayerValue.currentShield > 0)
            {
                int absorbed = Mathf.Min(PlayerValue.currentShield, predictedDamage);
                PlayerValue.currentShield -= absorbed;
                predictedDamage -= absorbed;
                if (predictedDamage <= 0) return; // 护盾吸收完全部伤害后退出
            }
        }

        // [3] 正式扣血
        PlayerValue.currentHP -= predictedDamage;

        // [4] 反弹伤害
        if (CardValue.ThornsLevel > 0 && enemy != null)
        {
            float reflectPercent = 0f;
            switch (CardValue.ThornsLevel)
            {
                case 1: reflectPercent = 0.2f; break;
                case 2: reflectPercent = 0.4f; break;
                case 3: reflectPercent = 0.6f; break;
            }

            int reflectDamage = Mathf.Max(1, Mathf.RoundToInt(predictedDamage * reflectPercent));

            if (enemy.TryGetComponent<IHurtable>(out var hurtable))
            {
                hurtable.TakeDamage(reflectDamage, false);
            }
        }

        StartCoroutine(HurtRoutine());

        if (PlayerValue.currentHP <= 0)
            StartCoroutine(Die());
    }
    private void TryTriggerLowHpShield()
    {
        if (PlayerValue. hasTriggeredLowHpShield) return;
        float shieldPercent = 0f;
        switch (CardValue.TriggerLowHpShield)
        {
            case 1: shieldPercent = 0.3f; break;
            case 2: shieldPercent = 0.6f; break;
            case 3: shieldPercent = 1.0f; break;
            default: return; // 未解锁不触发
        }

        currentMaxShield = PlayerValue.currentShield = Mathf.RoundToInt(playerValue.baseMaxHP * shieldPercent);
        PlayerValue.hasTriggeredLowHpShield = true;
        InitShieldBar();

        // TODO: 加入护盾启动动画/音效/UI提示
        Debug.Log($"触发低血护盾，获得 {PlayerValue.currentShield} 点护盾值");
    }
    void InitShieldBar()
    {
        GameObject barPrefab = Resources.Load<GameObject>("ShieldBar");
        if (barPrefab != null)
        {
            bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0); // 调整血条高度
            hurtUI = bar.GetComponent<HurtUI>();
            if (hurtUI != null)
                hurtUI.UpdateHealthBar(PlayerValue.currentShield, currentMaxShield);
        }
    }
    public bool IsInvincible()
    {
        return isInvincible;
    }
    private bool TryTriggerShield(GameObject enemy)
    {
        float chance = 0f;

        switch (CardValue.ThornsShieldLevel)
        {
            case 1: chance = 0.10f; break;
            case 2: chance = 0.15f; break;
            case 3: chance = 0.30f; break;
        }

        // 只有原始 chance > 0 才考虑 Boss 加成
        if (chance > 0f && enemy != null && enemy.CompareTag("Boss"))
        {
            chance += 0.5f;
        }

        if (chance <= 0f) return false;

        // 随机判定是否触发
        if (Random.value < chance)
        {
            StartCoroutine(TriggerShield());
            return true;
        }

        return false;
    }
    private IEnumerator TriggerShield()
    {
        isInvincible = true;
        // TODO：可以加上护盾特效，比如开启护罩粒子效果
        Debug.Log("haha");
        yield return new WaitForSeconds(0.5f); // 无敌持续时间
        isInvincible = false;
    }

    IEnumerator HurtRoutine()
    {
        //CantMove(0.5f);
        isInvincible = true;
        rb.velocity = Vector2.zero;
        // 受伤动画
        Color originalColor = spriteRenderer.color;
        // 闪红色
        spriteRenderer.color = Color.red;
        // 停顿一帧（或更久）
        yield return new WaitForSeconds(0.3f);
        // 恢复原色
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.5f); // 无敌帧时长
        isInvincible = false;
    }
    IEnumerator HurtRoutineShield()
    {
        //CantMove(0.5f);
        isInvincible = true;
        rb.velocity = Vector2.zero;
        // 受伤动画
        Color originalColor = spriteRenderer.color;
        // 闪红色
        spriteRenderer.color = Color.blue;
        // 停顿一帧（或更久）
        yield return new WaitForSeconds(0.3f);
        // 恢复原色
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.3f); // 无敌帧时长
        isInvincible = false;
    }
    IEnumerator Die()
    {
        //死亡动画
        yield return new WaitForSeconds(1f);
        Debug.Log("Player Died");
    }
    ///////////////////////////Card///////////////////////////////////////////////////////
    void TrySummonFireballs()
    {
        StartCoroutine(SummonFireballs());
        Ifball = true;
        isCoolingDown = true;
        fireballCooldownTimer = 0f;
        // 每次触发后，随机冷却时间（3 到 6 秒之间）
        fireballCooldownTime = Random.Range(3f, 6f);
    }
    IEnumerator SummonFireballs()
    {
        // 找出屏幕内的敌人
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<Transform> visibleEnemies = new List<Transform>();

        foreach (var enemy in allEnemies)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(enemy.transform.position);

            // 判断是否在屏幕内
            if (screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width &&
                screenPos.y >= 0 && screenPos.y <= Screen.height)
            {
                visibleEnemies.Add(enemy.transform);
            }
        }
        for (int i = 0; i < CardValue.fireballLevel; i++)
        {
            Vector3 targetWorldPos;

            if (visibleEnemies.Count > 0)
            {
                // 有敌人：随机选择一个敌人位置
                Transform target = visibleEnemies[Random.Range(0, visibleEnemies.Count)];
                targetWorldPos = target.position;
            }
            else
            {
                // 没有敌人：使用屏幕中心附近随机点
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 randomOffset = Random.insideUnitCircle * 100f;
                Vector2 screenPos = screenCenter + randomOffset;
                targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            }

            targetWorldPos.z = 0; // 保持在2D层上

            // 生成位置：目标点上方偏右
            Vector3 spawnPos = targetWorldPos + new Vector3(2f, 8f, 0f);

            GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            fireball.GetComponent<Fireball>().Init(targetWorldPos);
            yield return new WaitForSeconds(0.2f);
        }
        //yield return null;

    }
    void TryCastLightningBeam()
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 左右方向分别检测
        Vector2[] directions = { Vector2.left, Vector2.right };
        int[] hitCounts = new int[2];
        Vector3[] origins = new Vector3[2]; // 用于记录每个方向下，命中最多敌人的起点（只保存位置）

        for (int i = 0; i < 2; i++)
        {
            foreach (var enemy in allEnemies)
            {
                Vector3 origin = enemy.transform.position;
                RaycastHit2D[] hits = Physics2D.RaycastAll(origin, directions[i], 100f);
                int count = 0;
                foreach (var hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                    {
                        count++;
                    }
                }

                if (count > hitCounts[i])
                {
                    hitCounts[i] = count;
                    origins[i] = origin; // 记录该起点（含Y值）
                }
            }
        }

        // 决定方向
        int chosenDirIndex = hitCounts[0] > hitCounts[1] ? 0 :
                             hitCounts[0] < hitCounts[1] ? 1 :
                             Random.Range(0, 2); // 相等时随机

        Vector2 chosenDir = directions[chosenDirIndex];
        Vector3 bestHitOrigin = origins[chosenDirIndex];

        // 获取屏幕中心 X 坐标
        Vector3 screenCenterWorld = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        screenCenterWorld.z = 0f;

        // 把 Y 设置为命中最多敌人的射线起点 Y
        screenCenterWorld.y = bestHitOrigin.y;

        SpawnLightningBeam(screenCenterWorld, chosenDir);
        // 设置冷却
        isLightningCoolingDown = true;
        lightningCooldownTimer = 0f;
        lightningCooldownTime = Random.Range(3f, 6f);
    }

    void SpawnLightningBeam(Vector3 spawnPos, Vector2 direction)
    {
        GameObject beam = Instantiate(lightningBeamPrefab, spawnPos, Quaternion.identity);

        // 设置方向
        beam.transform.right = direction.normalized;
    }

}