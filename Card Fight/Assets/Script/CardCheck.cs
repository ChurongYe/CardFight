using UnityEngine;
using UnityEngine.EventSystems;

public class CardCheck : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isSelected = false;
    public Vector3 originalLocalPos;
    public Transform originalParent;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        originalLocalPos = rectTransform.localPosition;
        originalParent = transform.parent;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = !isSelected;

        if (isSelected)
        {
            rectTransform.localPosition = originalLocalPos + new Vector3(0, 50, 0); // 上移
        }
        else
        {
            rectTransform.localPosition = originalLocalPos; // 回到原位
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isSelected = false;
        rectTransform.localPosition = originalLocalPos;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        CardSlotController slotCtrl = FindObjectOfType<CardSlotController>();
        if (slotCtrl != null)
        {
            slotCtrl.TrySwap(this);
        }
    }

    public void ResetToOrigin()
    {
        transform.SetParent(originalParent);
        rectTransform.localPosition = originalLocalPos;
    }
}
