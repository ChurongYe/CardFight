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
            // ���Ե�ǰ�϶����Ƶ�ԭslot
            if (slot == draggedCard.originalParent) continue;

            // �����ǰslot������һ�ſ��ƣ������� RectTransform ����ͣ�п��Ƹ���
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
            // ��Ŀ��slot���п��ƣ����н���
            if (targetSlot.childCount > 0)
            {
                Transform otherCard = targetSlot.GetChild(0);

                // ����һ�ſ��ƷŻ�ԭʼslot
                otherCard.SetParent(draggedCard.originalParent);
                otherCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                otherCard.GetComponent<CardCheck>().originalParent = draggedCard.originalParent;
            }

            // �Լ�����Ŀ��slot
            draggedCard.transform.SetParent(targetSlot);
            draggedCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            draggedCard.originalParent = targetSlot;
        }
        else
        {
            // û��Ŀ��slot����ԭλ
            draggedCard.ResetToOrigin();
        }
    }

    // UI������ײ���
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
