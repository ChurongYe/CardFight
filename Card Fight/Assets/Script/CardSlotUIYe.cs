using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotUIYe : MonoBehaviour
{
    public Image cardIcon;            // ��������ʾ����ͼƬ
    public Image cooldownMask;        // ������ȴ���֣�Image.type = Filled��

    private CardMechanicsYe.Card card;
    private CardMechanicsYe.CardSlot slot;

    /// <summary>
    /// ���ÿ������ݲ����� UI ��ʾ������ʾͼ�����ȴ��
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
        // ������������� cardIcon.sprite = card.GetSprite(); ����ʾ��ӦͼƬ
    }

    private void Update()
    {
        if (slot == null || !slot.IsCoolingDown)
        {
            cooldownMask.fillAmount = 0f;
            return;
        }

        cooldownMask.fillAmount = slot.cooldown / 2f; // ���������ȴΪ 2 ��
    }

    // ���Ⱪ¶��������
    public CardMechanicsYe.Card GetCard() => card;
    public CardMechanicsYe.Suit GetSuit() => card?.suit ?? CardMechanicsYe.Suit.Red;
    public int GetNumber() => card?.number ?? 0;
}