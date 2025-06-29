using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;

        if (droppedObj != null && droppedObj.TryGetComponent<Card>(out var droppedCard))
        {
            var holder = FindObjectOfType<HorizontalCardHolder>();
            var selectedCards = holder.GetSelectedCards();

            // 组合是否合法
            if (holder.ValidateCombination(selectedCards) > 0)
            {
                // 合法 → 直接调用 TryPlaySelectedCards（包含：加成、出牌、清空、补位）
                holder.TryPlaySelectedCards();
            }
            else
            {
                // 不合法 → 取消选中 + 回到原位
                foreach (var card in selectedCards)
                {
                    card.Deselect();
                    card.ReturnToOriginalPosition();
                }
            }
        }
    }
}
