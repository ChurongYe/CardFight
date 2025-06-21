
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class HorizontalCardHolder : MonoBehaviour
{

    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;
    public RectTransform handAreaRect;

    [Header("Spawn Settings")]
    //[SerializeField] private int cardsToSpawn = 8;
    public List<Card> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    public CardDatabaseYe database;
    public List<CardData> cardPool;
    public List<Card> slots = new List<Card>();
    private bool isSorting = false;

    void Start()
    {
        // 原始一套基础卡牌
        List<CardData> baseCards = database.GetShuffledNormalCards();

        // 乘以3倍
        cardPool = new List<CardData>();
        for (int i = 0; i < 3; i++)
        {
            cardPool.AddRange(baseCards);
        }

        // 洗牌
        Shuffle(cardPool);

        for (int i = 0; i < transform.childCount; i++)
        {
            Card slot = transform.GetChild(i).GetComponentInChildren<Card>();
            slots.Add(slot);

            if (i < 9)
                slot.SetLocked(false); // 前3个可用
            else
                slot.SetLocked(true);  // 后5个锁定
        }

        //for (int i = 0; i < cardsToSpawn; i++)
        //{
        //    Instantiate(slotPrefab, transform);
        //}

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        foreach (Card card in cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitUntil(() => cards.All(c => c.cardVisual != null));
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                {
                    cards[i].cardVisual.UpdateIndex(cards[i].transform.parent.GetSiblingIndex());
                }
                else
                {
                    Debug.LogWarning($"[Frame] card {cards[i].name} 的 cardVisual 是空！");
                }
            }

        }
    }
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void SortCardVisuals()
    {
        if (isSorting) return; // 防止重复调用

        StartCoroutine(SortRoutine());
    }
    private IEnumerator SortRoutine()
    {
        isSorting = true;

        // 同步最新顺序
        cards = transform.Cast<Transform>()
            .Select(t => t.GetComponentInChildren<Card>())
            .Where(c => c != null)
            .ToList();

        var unlockedCards = cards.Where(c => !c.isLocked && c.cardVisual != null).ToList();
        var sortedCards = unlockedCards
            .OrderBy(c => c.cardVisual.data.suit)
            .ThenBy(c => c.cardVisual.data.number)
            .ToList();

        bool hasSwapped;

        do
        {
            hasSwapped = false;

            for (int i = 0; i < sortedCards.Count; i++)
            {
                var targetCard = sortedCards[i];
                var currentCard = unlockedCards[i];

                if (targetCard == currentCard)
                    continue;

                int targetIndex = cards.IndexOf(targetCard);
                int currentIndex = cards.IndexOf(currentCard);

                if (targetIndex == -1 || currentIndex == -1)
                    continue;

                SwapCardsPositions(cards[currentIndex], cards[targetIndex]);

                // 更新 cards 顺序
                cards[currentIndex] = targetCard;
                cards[targetIndex] = currentCard;

                // 更新 unlockedCards 同步下次检查用
                unlockedCards = cards.Where(c => !c.isLocked && c.cardVisual != null).ToList();

                hasSwapped = true;
                yield return new WaitForSeconds(0.05f); // 每次交换暂停一点点
            }

        } while (hasSwapped);

        yield return new WaitForSeconds(0.25f);
        isSorting = false;
    }
    // 负责交换两张卡牌的父物体、局部位置、并触发视觉动画
    private void SwapCardsPositions(Card cardA, Card cardB)
    {
        Transform parentA = cardA.transform.parent;
        Transform parentB = cardB.transform.parent;

        // 交换父物体
        cardA.transform.SetParent(parentB);
        cardB.transform.SetParent(parentA);
        Debug.Log($"交换父物体：{cardA.name} <--> {cardB.name}");
        // 还原局部位置（参考你原先的逻辑，选中状态有偏移）
        cardA.transform.localPosition = cardA.selected ? new Vector3(0, cardA.selectionOffset, 0) : Vector3.zero;
        cardB.transform.localPosition = cardB.selected ? new Vector3(0, cardB.selectionOffset, 0) : Vector3.zero;

        // 触发视觉动画
        if (cardA.cardVisual != null) cardA.cardVisual.Swap(1);
        if (cardB.cardVisual != null) cardB.cardVisual.Swap(-1);

        // 更新索引
        if (cardA.cardVisual != null) cardA.cardVisual.UpdateIndex(cardA.transform.parent.GetSiblingIndex());
        if (cardB.cardVisual != null) cardB.cardVisual.UpdateIndex(cardB.transform.parent.GetSiblingIndex());
    }
    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }


    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (isSorting || isCrossing || selectedCard == null)
            return;

        // 判断 selectedCard 是否还在区域内
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, selectedCard.transform.position);
        if (!RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, Camera.main))
        {
            // 超出区域，不做交换
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            //如果目标卡是锁定的，禁止交换
            if (cards[i].isLocked)
                continue;
            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateIndex(transform.childCount);
            }
            else
            {
                Debug.LogWarning($"Card {card.name} 的 cardVisual 为空！");
            }
        }
    }
    public List<Card> GetSelectedCards()
    {
        return cards.Where(c => c.selected && !c.isLocked).ToList();
    }
    public void RefreshLayout()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardVisual.cardImage.color == new Color(1, 1, 1, 0))
            {
                // 从右边找第一个有 cardVisual 的卡牌
                for (int j = i + 1; j < cards.Count; j++)
                {
                    if (cards[j].cardVisual.cardImage.color != new Color(1, 1, 1, 0))
                    {
                        // 将右侧卡牌整体（Card）移到当前位置（i）
                        Transform slotI = transform.GetChild(i); // 第i个slot
                        Transform slotJ = transform.GetChild(j); // 第j个slot

                        Card cardI = cards[i];
                        Card cardJ = cards[j];

                        // 交换父物体
                        cardJ.transform.SetParent(slotI);
                        cardJ.transform.localPosition = cardJ.selected ? new Vector3(0, cardJ.selectionOffset, 0) : Vector3.zero;

                        cardI.transform.SetParent(slotJ);
                        cardI.transform.localPosition = cardI.selected ? new Vector3(0, cardI.selectionOffset, 0) : Vector3.zero;

                        // 交换列表中的卡牌位置
                        cards[i] = cardJ;
                        cards[j] = cardI;

                        break;
                    }
                }
            }
        }

        // 更新每张卡的视觉 index
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardVisual != null)
            {
                cards[i].cardVisual.UpdateIndex(i);
            }
        }

    }
}
