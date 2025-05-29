using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string name;              // 名称，例如 "攻击提升"
    public string description;       // 简短描述，用于 UI 显示
    public StatType stat;            // 影响的属性类型

    public int level;                // 当前等级
    public int maxLevel = 10;        // 最大可升级等级

    public float baseValue = 1f;     // 初始加成数值
    public float valuePerLevel = 0.5f; // 每级增长值

    public int cost => (level + 1) * 2; // 升级消耗（根据当前等级）

    // 返回当前加成数值（用于应用到玩家属性）
    public float GetCurrentValue()
    {
        return baseValue + level * valuePerLevel;
    }

    // 是否还能升级
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    // 支持的属性加点类型
    public enum StatType
    {
        Attack,
        HP,
        Defense,
        Crit,
        MoveSpeed,
        DashCooldown
    }
}