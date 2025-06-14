using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Button OpenButton;
    public GameObject UpgradePanel;
    public Button toggleButton;              // 按钮
    public TMP_Text buttonText;

    private bool isPaused = false;

    void Start()
    {
        toggleButton.onClick.AddListener(TogglePause);
        UpdateButtonText();
        OpenButton.onClick.AddListener(() =>
        {
            UpgradePanel.SetActive(true);    // 打开升级面板
            gameObject.SetActive(false);     // 隐藏当前 GamePanel
        });
    }
    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        buttonText.text = isPaused ? "Continue" : "Stop";
    }
}
