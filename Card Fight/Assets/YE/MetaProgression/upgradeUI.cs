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

    public int availablePoints = 10; // ���õ������ɴӴ浵��ȡ��

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);          // �ر��������
            gamePanelToReturn.SetActive(true);    // �ָ�������
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
                    RefreshUI(); // ˢ�� UI
                }
            });
        }
    }
}