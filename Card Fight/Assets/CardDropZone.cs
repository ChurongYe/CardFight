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

            foreach (var card in selectedCards)
            {
                // 隐藏视觉但不销毁
                if (card.cardVisual != null)
                {
                    card.cardVisual.SetEmpty(); // 设置为空视觉
                }

                //回到原位 + 取消选中状态
                card.Deselect();
            }

            // 补位（让后面的卡牌补上来）
            holder.RefreshLayout();
        }
    }
}
