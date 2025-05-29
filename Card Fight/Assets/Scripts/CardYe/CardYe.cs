using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardYe : MonoBehaviour
{
    // -------------------- 近战攻击方式 --------------------
    public enum MeleeType
    {
        Normal,
        FlameSlash,
        Whirlwind
    }
    // -------------------- 远程攻击方式 --------------------
    public enum RangedType
    {
        Normal,
        Arrow,
        Boomerang,
    }
    // -------------------- 召唤物--------------------
    public GameObject[] Summon;
    private List<ISummonUnit> activeSummons = new List<ISummonUnit>();

    // -------------------- 攻击方式切换 --------------------
    public MeleeType currentMelee = MeleeType.Normal;
    public RangedType currentRanged = RangedType.Normal;
    private enum AttackSlot { Melee, Ranged }
    private AttackSlot currentSlot = AttackSlot.Melee;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            currentSlot = currentSlot == AttackSlot.Melee ? AttackSlot.Ranged : AttackSlot.Melee;
            Debug.Log("切换到攻击槽位：" + currentSlot);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseAttack();
        }
    }
    public void UseAttack()
    {
        if (currentSlot == AttackSlot.Melee)
            PerformMeleeAttack(currentMelee);
        else
            PerformRangedAttack(currentRanged);
    }
    // 可由卡牌调用
    public void SetMelee(MeleeType newType) => currentMelee = newType;
    public void SetRanged(RangedType newType) => currentRanged = newType;

    // -------------------- 攻击行为 --------------------
    #region 近战
    void PerformMeleeAttack(MeleeType type)
    {
        switch (type)
        {
            case MeleeType.Normal:
                Debug.Log("普通近战攻击");
                break;
            case MeleeType.FlameSlash:
                Debug.Log("火焰斩击");
                break;
            case MeleeType.Whirlwind:
                Debug.Log("旋风斩");
                break;
        }
    }
    #endregion
    #region 远程
    void PerformRangedAttack(RangedType type)
    {
        switch (type)
        {
            case RangedType.Normal:
                Debug.Log("普通远程攻击");
                break;
            case RangedType.Arrow:
                Debug.Log("射箭");
                break;
            case RangedType.Boomerang:
                Debug.Log("投掷回旋镖");
                break;
        }
    }
    #endregion
    #region 召唤
    // 调用方式：卡牌确定召唤物索引 -> 拖到场景中鼠标释放位置 -> 生成
    public void SummonAttack(int summonType, Vector3 dropPosition)
    {
        StartCoroutine(SummonAttackCoroutine(summonType, dropPosition));
    }

    private IEnumerator SummonAttackCoroutine(int summonType, Vector3 dropPosition)
    {
        if (summonType < 0 || summonType >= Summon.Length)
        {
            Debug.LogWarning("召唤索引错误: " + summonType);
            yield break;
        }

        yield return new WaitForSeconds(1f); // 等待动画

        GameObject summon = Instantiate(Summon[summonType], dropPosition, Quaternion.identity);

        ISummonUnit unit = summon.GetComponent<ISummonUnit>();
        if (unit != null)
        {
            unit.SetManager(this);    
            activeSummons.Add(unit);  
        }
        else
        {
            Debug.LogWarning("召唤物没有实现 ISummonUnit 接口！");
        }
    }

    public void RemoveSummon(ISummonUnit unit)
    {
        if (activeSummons.Contains(unit))
        {
            activeSummons.Remove(unit);
        }
    }
    public float buffMultiplier;
    public void MultiplyAllSummons(float multiplier)
    {
        foreach (var unit in activeSummons)
        {
            unit.MultiplyStats(buffMultiplier);
        }
    }
    #endregion
}
