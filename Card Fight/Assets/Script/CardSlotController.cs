using System.Collections.Generic;
using UnityEngine;

public class CardSlotController : MonoBehaviour
{
    public List<Transform> cardSlots = new List<Transform>();

    private void Start()
    {
        cardSlots.Clear();
        foreach (Transform child in transform)
        {
            cardSlots.Add(child);
        }
    }

    public void TrySwap(CardCheck draggedCard)
    {
        float closestDistance = float.MaxValue;
        Transform targetSlot = null;

        foreach (Transform slot in cardSlots)
        {
            // 忽略当前拖动卡牌的原slot
            if (slot == draggedCard.originalParent) continue;

            // 如果当前slot内已有一张卡牌，并且其 RectTransform 被悬停中卡牌覆盖
            if (slot.childCount > 0)
            {
                Transform otherCard = slot.GetChild(0);
                if (RectOverlapping(draggedCard.GetComponent<RectTransform>(), otherCard.GetComponent<RectTransform>()))
                {
                    targetSlot = slot;
                    break;
                }
            }
            else
            {
                float dist = Vector3.Distance(draggedCard.transform.position, slot.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetSlot = slot;
                }
            }
        }

        if (targetSlot != null)
        {
            // 若目标slot已有卡牌，进行交换
            if (targetSlot.childCount > 0)
            {
                Transform otherCard = targetSlot.GetChild(0);

                // 将另一张卡牌放回原始slot
                otherCard.SetParent(draggedCard.originalParent);
                otherCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                otherCard.GetComponent<CardCheck>().originalParent = draggedCard.originalParent;
            }

            // 自己换到目标slot
            draggedCard.transform.SetParent(targetSlot);
            draggedCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            draggedCard.originalParent = targetSlot;
        }
        else
        {
            // 没有目标slot，回原位
            draggedCard.ResetToOrigin();
        }
    }

    // UI矩形碰撞检测
    private bool RectOverlapping(RectTransform a, RectTransform b)
    {
        Rect rectA = GetWorldRect(a);
        Rect rectB = GetWorldRect(b);
        return rectA.Overlaps(rectB);
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }
}
