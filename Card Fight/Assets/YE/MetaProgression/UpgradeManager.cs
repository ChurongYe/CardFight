using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    public UpgradeDatabase database;
    public List<UpgradeData> upgrades => database.upgradeList;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //非线性加点初始化
        foreach (var upgrade in database.upgradeList)
        {
            upgrade.GenerateNonLinearLevelValues();
        }

        LoadUpgrades();
    }

    // 给外部调用，升级某项加点
    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= upgrades.Count)
            return false;

        var upgrade = upgrades[index];

        if (!upgrade.CanUpgrade())
            return false;

        upgrade.level++;

        SaveUpgrades();

        return true;
    }

    // 保存加点
    public void SaveUpgrades()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            PlayerPrefs.SetInt("UpgradeLevel_" + i, upgrades[i].level);
        }
        PlayerPrefs.Save();
    }

    // 读取加点
    public void LoadUpgrades()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            upgrades[i].level = PlayerPrefs.GetInt("UpgradeLevel_" + i, 0);
        }
    }

    // 应用所有永久加点到玩家属性
    public void ApplyAllUpgradesToPlayer(Core.PlayerValue playerValue)
    {
        playerValue.ApplyAllPermanentUpgrades(upgrades);
    }

    // 这个方法可以让外部调用，传入玩家对象
    public void OnPlayerSpawned(Core.PlayerValue playerValue)
    {
        ApplyAllUpgradesToPlayer(playerValue);
    }
}