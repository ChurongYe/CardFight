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
            Destroy(card.gameObject); // �� card.transform.parent.gameObject �������Ƕ���� Slot ��
        }
    }
}
