using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaManager : MonoBehaviour
{
    public static MetaManager Instance;
    public UpgradeDatabase database;

    public int totalUpgradePoints = 0;

    private void Awake()
    {
        Instance = this;
        LoadProgress();
    }
    public List<UpgradeData> upgrades => database.upgradeList;
    public void AddPoints(int damageDealt)
    {
        int points = Mathf.FloorToInt(damageDealt / 100); // ÿ���100�˺���1��
        totalUpgradePoints += points;
        SaveProgress();
    }

    public void UpgradeStat(UpgradeData data)
    {
        if (totalUpgradePoints >= data.cost)
        {
            totalUpgradePoints -= data.cost;
            data.level++;
            SaveProgress();
        }
    }

    public void SaveProgress() { /* ʹ�� PlayerPrefs/JSON ���� upgrades �� totalUpgradePoints */ }
    public void LoadProgress() { /* �������� */ }
}
