using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Button OpenButton;
    public GameObject UpgradePanel;
    void Start()
    {
        OpenButton.onClick.AddListener(() =>
        {
            UpgradePanel.SetActive(true);    // 打开升级面板
            gameObject.SetActive(false);     // 隐藏当前 GamePanel
        });
    }
}
