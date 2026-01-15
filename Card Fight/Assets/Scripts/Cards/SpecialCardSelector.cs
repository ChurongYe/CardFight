using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialCardSelector : MonoBehaviour
{
    public CardData specialCardData; // 当前要选择的特殊卡
    public GameObject confirmPanel; // 包含“加入卡组”和“丢弃”按钮的面板

    private HorizontalCardHolder holder;

    void Start()
    {
        holder = FindObjectOfType<HorizontalCardHolder>();
    }

    // 玩家点击这张特殊卡牌
    public void OnSpecialCardClicked()
    {
        var emptySlot = holder.cards.FirstOrDefault(c =>
            c.cardVisual != null && c.cardVisual.IsEmpty() && !c.isCoolingDown);

        if (emptySlot != null)
        {
            emptySlot.cardVisual.SetCard(specialCardData);
            emptySlot.currentSprite = true;
            confirmPanel.SetActive(false); // 隐藏面板
        }
        else
        {
            confirmPanel.SetActive(true); // 显示加入卡组/丢弃选项
        }
    }

    // 玩家点击“加入卡组”
    public void OnClickAddToDeck()
    {
        holder.ReturnToCardPool(specialCardData);
        confirmPanel.SetActive(false);
        Debug.Log($"已将特殊卡加入卡组：{specialCardData}");
    }

    // 玩家点击“丢弃”
    public void OnClickDiscard()
    {
        confirmPanel.SetActive(false);
        Debug.Log($"已丢弃特殊卡：{specialCardData}");
    }
}