using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class upgradeUI : MonoBehaviour
{
    public UpgradeDatabase upgradeDatabase;
    public GameObject upgradeItemPrefab;
    public Transform upgradeListParent;
    public TMP_Text totalPointsText;
    public Button closeButton;
    public GameObject gamePanelToReturn;

    public int availablePoints = 10; // 可用点数（可从存档读取）

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);          // 关闭升级面板
            gamePanelToReturn.SetActive(true);    // 恢复主界面
        });
        RefreshUI();
    }

    void RefreshUI()
    {
        totalPointsText.text = $"Points:{availablePoints}";
        foreach (Transform child in upgradeListParent)
            Destroy(child.gameObject);

        foreach (var data in upgradeDatabase.upgradeList)
        {
            GameObject item = Instantiate(upgradeItemPrefab, upgradeListParent);
            item.transform.Find("Text_Name").GetComponent<TMP_Text>().text = data.name;
            item.transform.Find("Text_Level").GetComponent<TMP_Text>().text = $"Grade:{data.level}/{data.maxLevel}";
            item.transform.Find("Text_Cost").GetComponent<TMP_Text>().text = "Cost: " + data.cost;

            Button addBtn = item.transform.Find("Button_Add").GetComponent<Button>();
            addBtn.interactable = availablePoints >= data.cost && data.CanUpgrade();

            addBtn.onClick.AddListener(() =>
            {
                if (availablePoints >= data.cost && data.CanUpgrade())
                {
                    availablePoints -= data.cost;
                    data.level++;
                    RefreshUI(); // 刷新 UI
                }
            });
        }
    }
}