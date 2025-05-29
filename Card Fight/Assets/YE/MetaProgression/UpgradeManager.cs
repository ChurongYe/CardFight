using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<UpgradeData> upgrades;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadUpgrades();
    }

    // ���ⲿ���ã�����ĳ��ӵ�
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

    // ����ӵ�
    public void SaveUpgrades()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            PlayerPrefs.SetInt("UpgradeLevel_" + i, upgrades[i].level);
        }
        PlayerPrefs.Save();
    }

    // ��ȡ�ӵ�
    public void LoadUpgrades()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            upgrades[i].level = PlayerPrefs.GetInt("UpgradeLevel_" + i, 0);
        }
    }

    // Ӧ���������üӵ㵽�������
    public void ApplyAllUpgradesToPlayer(Core.PlayerValue playerValue)
    {
        playerValue.ApplyAllPermanentUpgrades(upgrades);
    }

    // ��������������ⲿ���ã�������Ҷ���
    public void OnPlayerSpawned(Core.PlayerValue playerValue)
    {
        ApplyAllUpgradesToPlayer(playerValue);
    }
}