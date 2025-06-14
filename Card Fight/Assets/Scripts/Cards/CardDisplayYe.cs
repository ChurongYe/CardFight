using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplayYe : MonoBehaviour
{
    public CardData data;
    public Image cardImage;
    public bool isSelected;

    public void SetCard(CardData card)
    {
        data = card;
        cardImage.sprite = card.sprite;
        isSelected = false;
    }

    //public void ToggleSelect()
    //{
    //    isSelected = !isSelected;
    //    // UI highlight here
    //}
    public void SetInteractable(bool interactable)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = interactable;
        GetComponent<CanvasGroup>().interactable = interactable;
        // 如果你还有拖拽脚本/点击逻辑，也在这里做启用/禁用
    }
}
