using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string name;                    // ����
    public string description;             // ����
    public StatType stat;                  // Ӱ������
    public int level = 0;                  // ��ǰ�ȼ�
    public int maxLevel = 10;              // ���ȼ�

    // ÿ���ļӳ�ֵ���������Զ�����
    public List<int> levelValues = new List<int>();

    // �����ֶΣ���¼�Ѿ�Ӧ�õ��������Եĵȼ���Ĭ��0��
    [NonSerialized]
    public int appliedLevel = 0;
    // ��ȡĳ���ӳ�
    public float GetLevelValue(int lvl)
    {
        if (lvl <= 0 || lvl > levelValues.Count) return 0;
        return levelValues[lvl - 1];
    }


    // ��������ʾ������д�� (��ǰ�ȼ�+1)*2
    public int cost => (level + 1) * 2;

    // ���ص�ǰ�ܼӳ�ֵ���ۼӵ���ǰ�ȼ���
    public int GetTotalBonusValue()
    {
        int total = 0;
        for (int i = 0; i < level && i < levelValues.Count; i++)
        {
            total += levelValues[i];
        }
        return total;
    }

    // �Ƿ�������
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    // �Զ����ɷ����Լӳ�ֵ
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