using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string name;              // ���ƣ����� "��������"
    public string description;       // ������������� UI ��ʾ
    public StatType stat;            // Ӱ�����������

    public int level;                // ��ǰ�ȼ�
    public int maxLevel = 10;        // ���������ȼ�

    public float baseValue = 1f;     // ��ʼ�ӳ���ֵ
    public float valuePerLevel = 0.5f; // ÿ������ֵ

    public int cost => (level + 1) * 2; // �������ģ����ݵ�ǰ�ȼ���

    // ���ص�ǰ�ӳ���ֵ������Ӧ�õ�������ԣ�
    public float GetCurrentValue()
    {
        return baseValue + level * valuePerLevel;
    }

    // �Ƿ�������
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    // ֧�ֵ����Լӵ�����
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