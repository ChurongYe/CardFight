using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string name;                    // 名称
    public string description;             // 描述
    public StatType stat;                  // 影响属性
    public int level = 0;                  // 当前等级
    public int maxLevel = 10;              // 最大等级

    // 每级的加成值，非线性自动生成
    public List<int> levelValues = new List<int>();

    // 新增字段，记录已经应用到基础属性的等级（默认0）
    [NonSerialized]
    public int appliedLevel = 0;
    // 获取某级加成
    public float GetLevelValue(int lvl)
    {
        if (lvl <= 0 || lvl > levelValues.Count) return 0;
        return levelValues[lvl - 1];
    }


    // 升级消耗示例，简单写成 (当前等级+1)*2
    public int cost => (level + 1) * 2;

    // 返回当前总加成值（累加到当前等级）
    public int GetTotalBonusValue()
    {
        int total = 0;
        for (int i = 0; i < level && i < levelValues.Count; i++)
        {
            total += levelValues[i];
        }
        return total;
    }

    // 是否还能升级
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    // 自动生成非线性加成值
    public void GenerateNonLinearLevelValues()
    {
        levelValues.Clear();
        for (int i = 1; i <= maxLevel; i++)
        {
            int value = i * i + 2; // 3, 6, 11, 18, 27, ...
            levelValues.Add(value);
        }
    }
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