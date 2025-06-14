
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine.WSA;
using Unity.VisualScripting;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private HorizontalCardHolder holder;
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Slot State")]
    public bool currentSprite = false;
    public bool isCoolingDown = false;
    public bool isLocked = false; // 新增：是否锁定
    public Image lockOverlay; // UI图层：用于显示锁的遮罩图或透明层
    public float cooldownTime = 2f;
    private float cooldownTimer = 5f;
    public Image cooldownOverlay;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!instantiateVisual)
            return;

        visualHandler = FindObjectOfType<VisualCardsHandler>();
        holder = FindObjectOfType<HorizontalCardHolder>();
        //cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        StartCoroutine(FillSlots());
    }
    IEnumerator FillSlots()
    {
        yield return new WaitForSeconds(0.1f);

        // 如果是锁定的卡槽，显示卡背面，不执行抽卡逻辑
        if (isLocked)
        {
            GameObject visual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform);
            cardVisual = visual.GetComponent<CardVisual>();
            cardVisual.Initialize(this);
            cardVisual.SetCardBack(); //你需要实现这个方法，用来显示卡背面
            currentSprite = true;
            yield break;
        }

        if (!currentSprite && !isCoolingDown)
        {
            if (holder.cardPool.Count == 0)
            {
                Debug.LogWarning("cardPool is empty! 无法生成卡牌");
                yield break;
            }

            int randomIndex = Random.Range(0, holder.cardPool.Count);
            CardData card = holder.cardPool[randomIndex];

            GameObject VisualPrefab = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform);
            cardVisual = VisualPrefab.GetComponent<CardVisual>();
            cardVisual.Initialize(this);
            cardVisual.SetCard(card);
            currentSprite = true;

            holder.cardPool.RemoveAt(randomIndex);
        }
    }
    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }
    /// <summary>
    /// ////////////////////////////////////////////////
    /// </summary>
    /// <param name="locked"></param>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        //if (lockOverlay != null)
        //    lockOverlay.gameObject.SetActive(locked);
    }

    public void StartCooldown(System.Action onCooldownEnd)
    {
        StartCoroutine(CooldownCoroutine(onCooldownEnd));
    }

    private IEnumerator CooldownCoroutine(System.Action onCooldownEnd)
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownTimer); // 冷却时间
        isCoolingDown = false;
        onCooldownEnd?.Invoke();
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////
    /// </summary>
    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isLocked) return;
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += (cardVisual.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (isLocked) return;
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {
        if(cardVisual != null)
        Destroy(cardVisual.gameObject);
    }
}
