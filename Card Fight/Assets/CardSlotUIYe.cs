using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotUIYe : MonoBehaviour
{
    public Image cardIcon;            // 可用于显示卡牌图片
    public Image cooldownMask;        // 用于冷却遮罩（Image.type = Filled）

    private CardMechanicsYe.Card card;
    private CardMechanicsYe.CardSlot slot;

    /// <summary>
    /// 设置卡槽数据并更新 UI 显示（仅显示图标和冷却）
    /// </summary>
    public void Setup(CardMechanicsYe.CardSlot slot)
    {
        this.slot = slot;
        this.card = slot.card;

        if (card == null)
        {
            cardIcon.gameObject.SetActive(false);
            cooldownMask.fillAmount = 0f;
            return;
        }

        cardIcon.gameObject.SetActive(true);
        // 这里你可以设置 cardIcon.sprite = card.GetSprite(); 来显示对应图片
    }

    private void Update()
    {
        if (slot == null || !slot.IsCoolingDown)
        {
            cooldownMask.fillAmount = 0f;
            return;
        }

        cooldownMask.fillAmount = slot.cooldown / 2f; // 假设最大冷却为 2 秒
    }

    // 对外暴露卡牌数据
    public CardMechanicsYe.Card GetCard() => card;
    public CardMechanicsYe.Suit GetSuit() => card?.suit ?? CardMechanicsYe.Suit.Red;
    public int GetNumber() => card?.number ?? 0;
}