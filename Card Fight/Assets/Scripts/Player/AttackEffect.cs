using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    private Animator animator;
    private PlayerValue playerValue;
    private PlayerController playerController;

    public float baseLength = 1.1f;
    private float speed;
    public AttackDirection direction; //保存传入的方向

    public void SetDirection(AttackDirection dir) //外部调用设置方向
    {
        direction = dir;
    }
    public Weapon weapon;
    //private void Awake()
    //{
    //    Debug.Log("AttackEffect Awake，ID = " + GetInstanceID());
    //}
    public void SetWeapon(Weapon w)
    {
        weapon = w;
    }
    void Start()
    {
        DisableAllColliders();

        playerValue = FindObjectOfType<PlayerValue>();
        playerController = FindObjectOfType<PlayerController>();
        animator = GetComponent<Animator>();

        speed = playerValue.currentAttackSpeed;
        animator.SetFloat("AttackSpeedMultiplier", speed);

        Destroy(gameObject, baseLength / speed);
    }

    void Update()
    {
        if (playerController.moveInput != Vector2.zero)
        {
            Destroy(gameObject);
        }
    }

    //供动画事件调用的函数：
    public void EnableDirectionCollider()
    {

        if (weapon != null)
        {
            weapon.EnableDirectionHitbox((int)direction);
        }
        else
        {

        }
    }

    public void DisableAllColliders()
    {
        if (weapon != null)
        {
            weapon.DisableAllHitboxesImmediate(); // 改为 public 非协程版本
        }
    }
}