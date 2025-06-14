using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Button OpenButton;
    public GameObject UpgradePanel;
    public Button toggleButton;              // ��ť
    public TMP_Text buttonText;

    private bool isPaused = false;

    void Start()
    {
        toggleButton.onClick.AddListener(TogglePause);
        UpdateButtonText();
        OpenButton.onClick.AddListener(() =>
        {
            UpgradePanel.SetActive(true);    // ���������
            gameObject.SetActive(false);     // ���ص�ǰ GamePanel
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
