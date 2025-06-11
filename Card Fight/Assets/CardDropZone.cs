using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;

        if (droppedObj != null && droppedObj.TryGetComponent<Card>(out var card))
        {
            Debug.Log("Card dropped into zone, destroying...");
            Destroy(card.gameObject); // 或 card.transform.parent.gameObject 如果卡牌嵌套在 Slot 里
        }
    }
}
