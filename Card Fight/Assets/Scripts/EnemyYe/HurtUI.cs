using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtUI : MonoBehaviour
{
    public GameObject bloodRed;            // 红色血条
    private Vector2 originalSize;          // 血条原始宽高
    private RectTransform bloodRect;

    void Awake()
    {
        bloodRect = bloodRed.GetComponent<RectTransform>();
        originalSize = bloodRect.sizeDelta;
    }
    public void UpdateHealthBar(int current, int max)
    {
        float ratio = Mathf.Clamp01((float)current / max);
        bloodRect.sizeDelta = new Vector2(originalSize.x * ratio, originalSize.y);
    }

}
