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

            // 先判断选中牌组是否合法（组合是否有效）
            if (holder.ValidateCombination(selectedCards) > 0)
            {
                // 合法，出牌：
                foreach (var card in selectedCards)
                {
                    if (card.cardVisual == null || card.cardVisual.IsEmpty())
                        continue;

                    holder.ReturnToCardPool(card.cardVisual.data);
                    card.cardVisual.SetEmpty();
                    card.Deselect();
                }

                holder.RefreshLayout();
            }
            else
            {
                // 不合法，全部回归原位，取消选中
                foreach (var card in selectedCards)
                {
                    card.Deselect();
                    card.ReturnToOriginalPosition(); // 回归原位并动画
                }
            }
        }
    }
}
