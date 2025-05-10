using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Shield : MonoBehaviour
{
    private Vector3 shieldForward;

    void Start()
    {
        shieldForward = transform.forward;  
    }

    // 判断攻击是否来自盾牌的前面
    public bool IsAttackFromFront(Vector3 attackDirection)
    {
        // 计算攻击方向与盾牌正面方向的点积
        float dotProduct = Vector3.Dot(attackDirection.normalized, shieldForward.normalized);

        // 如果点积大于0，说明攻击来自盾牌前面
        return dotProduct > 0;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 获取攻击方向（敌人的攻击的朝向向量）
        Vector3 attackDirection = other.transform.position - transform.position;

        // 判断攻击是否来自盾牌的前面
        if (IsAttackFromFront(attackDirection))
        {
            // 拦截攻击
            Debug.Log("攻击被盾牌挡住");
            // 可以添加其他拦截的行为，例如减伤、反弹等
        }
        else
        {
            // 攻击命中背部
            Debug.Log("攻击成功，命中背部");
            // 进行伤害处理等
        }
    }
}

public class EnemyAttack : MonoBehaviour
{
    public GameObject target; // 玩家或其他目标

    void Attack()
    {
        Vector3 attackDirection = (target.transform.position - transform.position).normalized;

        // 调用盾牌的 IsAttackFromFront 方法来判断攻击方向
        Shield shield = target.GetComponentInChildren<Shield>();  // 假设盾牌是玩家的子物体
        if (shield != null && shield.IsAttackFromFront(attackDirection))
        {
            // 如果攻击来自盾牌的前方，可以被挡住
            Debug.Log("攻击被挡住");
        }
        else
        {
            // 否则攻击可以从背后命中
            Debug.Log("攻击成功");
        }
    }
}

