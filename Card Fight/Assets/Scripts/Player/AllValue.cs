using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AllValue : MonoBehaviour
{
    private PlayerController playerController;
    private Weapon weapon;
    private CardYe cardYe;

    [Header("PlayerController")]
    public float walkSpeed = 7f;
    public float dashSpeed = 25f;
    public float dashCooldown = 1f;
    public float reducedMoveSpeed = 2f; // 蓄力时的移动速度
    public int playerHealth = 10;
    public bool ifClear = false; //净化

    [Header("PlayerAttack")]
    public float impactForce = 1f;
    public int PlayerDamage = 1;
    public float swingDuration = 0.2f;

    [Header("PlayerSummonAttack")]
    public float buffMultiplier = 1f;

    private void Start()
    {
        StartCoroutine(LateStart());
    }
    private IEnumerator LateStart()
    {
        yield return null; // 等待一帧，确保其他 Start() 执行完成

        playerController = FindObjectOfType<PlayerController>();
        weapon = FindObjectOfType<Weapon>();
        cardYe = FindObjectOfType<CardYe>();
        SetStartValue();
    }
    private void SetStartValue()
    {
        // 初始同步：PlayerController
        ChangePlayerwalkSpeed = walkSpeed;
        ChangePlayerdashSpeed = dashSpeed;
        ChangePlayerdashCooldown = dashCooldown;
        ChangePlayerReducedMoveSpeed = reducedMoveSpeed;
        ChangePlayerplayerHealth = playerHealth;
        ChangePlayerifClear = ifClear;
        //攻击频率
        ChangeWeaponswingDuration = swingDuration;
        //召唤物血量倍率
        if (cardYe != null)
        {
            cardYe.MultiplyAllSummons(buffMultiplier);
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
    public int ChangePlayerplayerHealth
    {
        get { return playerHealth; }
        set
        {
            playerHealth = value;

            if (playerController != null)
            {
                playerController.PlayerHealth = playerHealth;
            }
        }
    }
    public bool ChangePlayerifClear
    {
        get { return ifClear; }
        set
        {
            ifClear = value;

            if (playerController != null)
            {
                playerController.IfClear = ifClear;
            }
        }
    }
    #endregion
    #region playerAttack


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
    public float ChangesummonbuffMultiplier
    {
        get { return buffMultiplier; }
        set
        {
            buffMultiplier = value;

            if (cardYe != null)
            {
                cardYe.GetComponent<CardYe>().BuffMultiplier = buffMultiplier;
            }
        }
    }
    #endregion
}
