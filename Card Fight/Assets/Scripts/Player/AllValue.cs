using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AllValue : MonoBehaviour
{
    private PlayerController playerController;
    private Weapon weapon;
    private Summon[] summons;

    [Header("PlayerMovement")]
    public float walkSpeed = 7f;
    public float dashSpeed = 25f;
    public float dashCooldown = 1f;
    public float reducedMoveSpeed = 2f; // 蓄力时的移动速度

    [Header("MeleeAttack")]
    public float maxChargeTime = 2f;
    public float swingDuration = 0.2f;
    // 冲击力的最小和最大值
    public float minImpactForce = 15f;
    public float maxImpactForce = 45f;

    [Header("RangedAttack")]
    public GameObject Knife;
    public float speedFactor = 1.0f;
    public float maxDistance = 10f;

    [Header("SummonAttack")]
    public float summonDuration = 7f;
    public float summonCooldown = 15f;
    //召唤物攻击力大小
    public float summonattackDamage = 1;

    private void Start()
    {
        StartCoroutine(LateStart());
    }
    private IEnumerator LateStart()
    {
        yield return null; // 等待一帧，确保其他 Start() 执行完成

        playerController = FindObjectOfType<PlayerController>();
        weapon = FindObjectOfType<Weapon>();
        summons = FindObjectsOfType<Summon>();

        SetStartValue();
    }
    private void SetStartValue()
    {
        // 初始同步：PlayerController
        ChangePlayerwalkSpeed = walkSpeed;
        ChangePlayerdashSpeed = dashSpeed;
        ChangePlayerdashCooldown = dashCooldown;
        ChangePlayerReducedMoveSpeed = reducedMoveSpeed;

        ChangePlayermaxChargeTime = maxChargeTime;
        ChangePlayersummonDuration = summonDuration;
        ChangePlayersummonCooldown = summonCooldown;
        ChangeWeaponminImpactForce = minImpactForce;
        ChangeWeaponmaxImpactForce = maxImpactForce;
        ChangeWeaponswingDuration = swingDuration;
        if (summons != null)
        {
            ChangesummonattackDamage = summonattackDamage;
        }
        if(Knife != null)
        {
            ChangerangedattackspeedFactor = speedFactor;
            ChangerangedattackmaxDistance = maxDistance;
        }
    }
    #region player
    public float ChangePlayerwalkSpeed
    {
        get { return walkSpeed; }
        set
        {
            walkSpeed = value;

            if (playerController != null)
            {
                playerController.WalkSpeed = walkSpeed;
            }
        }
    }
    public float ChangePlayerdashSpeed
    {
        get { return dashSpeed; }
        set
        {
            dashSpeed = value;

            if (playerController != null)
            {
                playerController.DashSpeed = dashSpeed;
            }
        }
    }
    public float ChangePlayerdashCooldown
    {
        get { return dashCooldown; }
        set
        {
            dashCooldown = value;

            if (playerController != null)
            {
                playerController.DashCooldown = dashCooldown;
            }
        }
    }
    public float ChangePlayerReducedMoveSpeed
    {
        get { return reducedMoveSpeed; }
        set
        {
            reducedMoveSpeed = value;

            if (playerController != null)
            {
                playerController.ReducedMoveSpeed = reducedMoveSpeed;
            }
        }
    }
    #endregion
    #region playerAttack
    public float ChangePlayermaxChargeTime
    {
        get { return maxChargeTime; }
        set
        {
            maxChargeTime = value;

            if (playerController != null)
            {
                playerController.MaxChargeTime = maxChargeTime;
            }
        }
    }
    public float ChangePlayersummonDuration
    {
        get { return summonDuration; }
        set
        {
            summonDuration = value;

            if (playerController != null)
            {
                playerController.SummonDuration = summonDuration;
            }
        }
    }
    public float ChangePlayersummonCooldown
    {
        get { return summonCooldown; }
        set
        {
            summonCooldown = value;

            if (playerController != null)
            {
                playerController.SummonCooldown = summonCooldown;
            }
        }
    }
    public float ChangeWeaponminImpactForce
    {
        get { return minImpactForce; }
        set
        {
            minImpactForce = value;

            if (weapon != null)
            {
                weapon.MinImpactForce = minImpactForce;
            }
        }
    }
    public float ChangeWeaponmaxImpactForce
    {
        get { return maxImpactForce; }
        set
        {
            maxImpactForce = value;

            if (weapon != null)
            {
                weapon.MaxImpactForce = maxImpactForce;
            }
        }
    }
    public float ChangeWeaponswingDuration
    {
        get { return swingDuration; }
        set
        {
            swingDuration = value;

            if (weapon != null)
            {
                weapon.SwingDuration = swingDuration;
            }
        }
    }
    public float ChangesummonattackDamage
    {
        get { return summonattackDamage; }
        set
        {
            summonattackDamage = value;

            if (summons != null)
            {
                foreach (var summon in summons)
                {
                    summon.GetComponent<Summon>().SummonattackDamage = summonattackDamage;
                }
            }
        }
    }
    public float ChangerangedattackspeedFactor
    {
        get { return speedFactor; }
        set
        {
            speedFactor = value;

            if (Knife != null)
            {
                Knife.GetComponent<RangedKnife>().SpeedFactor = speedFactor;
            }
        }
    }
    public float ChangerangedattackmaxDistance
    {
        get { return maxDistance; }
        set
        {
            maxDistance = value;

            if (Knife != null)
            {
                Knife.GetComponent<RangedKnife>().MaxDistance = maxDistance;
            }
        }
    }
    #endregion
}
