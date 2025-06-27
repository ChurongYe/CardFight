
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine.WSA;

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

        //只筛选非锁定、非空卡
        var unlockedCards = cards
            .Where(c => !c.isLocked && c.cardVisual != null && !c.cardVisual.IsEmpty())
            .ToList();

        //排序依据：先花色，再数字
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
    public void OnCardClicked(Card clicked)
    {
        if (clicked.isLocked) return;

        if (clicked.cardVisual != null && clicked.cardVisual.data.IsSpecial)
        {
            // 特殊卡只能选中一张
            var already = cards.FirstOrDefault(c =>
                c != clicked && c.selected && c.cardVisual != null && c.cardVisual.data.IsSpecial);
            if (already != null)
                already.Deselect();
        }

        // 切换选中状态
        if (!clicked.selected)
            clicked.Select();
        else
            clicked.Deselect();
    }
    public void ReturnToCardPool(CardData card)
    {
        if (card != null)
        {
            cardPool.Add(card);
            Debug.Log($"已将 {card} 放回卡池");
            Shuffle(cardPool);
        }
    }
    public void RefreshEmptyCards()//刷新卡牌
    {
        foreach (var card in cards)
        {
            if (card.cardVisual != null && card.cardVisual.IsEmpty() && !card.isCoolingDown)
            {
                var newCard = database.GetRandomCardFromPool(cardPool);
                if (newCard != null)
                {
                    card.cardVisual.SetCard(newCard);
                    card.currentSprite = true;

                    //恢复交互状态
                    card.ResetStateAfterRefresh();

                    Debug.Log($"刷新卡牌为：{newCard}");
                }
            }
        }
    }

    public void TryPlaySelectedCards()
    {
        var selected = GetSelectedCards();
        if (ValidateCombination(selected) >= 3)
        {
            int sum = selected.Sum(c => c.data.number);

            if (selected.Any(c => c.data.specialType == SpecialCardType.Double))
                sum *= 2;
            if (selected.Any(c => c.data.specialType == SpecialCardType.Haste))
                sum += sum;

            // 出牌逻辑
            foreach (var card in selected)
            {
                ReturnToCardPool(card.cardVisual.data); // 放回卡池
                card.cardVisual.SetEmpty();                    // 视觉隐藏
                card.Deselect();
            }

            RefreshLayout(); // 填补空位
        }
    }

    public int ValidateCombination(List<Card> cards)
    {
        // 如果没有卡牌或任意卡牌为空，不能出牌
        if (cards == null || cards.Count == 0 || cards.Any(c => c.cardVisual == null || c.cardVisual.IsEmpty()))
            return 0;

        var cardDatas = cards.Select(c => c.cardVisual.data).ToList();

        // 特殊情况：只选中一张普通卡，允许出牌
        if (cardDatas.Count == 1 && !cardDatas[0].IsSpecial)
            return 1;

        // 所有卡都必须是普通卡
        if (cardDatas.Any(c => c.IsSpecial))
            return 0;

        // 检查花色是否一致
        var firstSuit = cardDatas[0].suit;
        if (!cardDatas.All(c => c.suit == firstSuit))
            return 0;

        // 提取并排序数字
        var numbers = cardDatas.Select(c => c.number).OrderBy(n => n).ToList();

        // 检查数字是否连续
        for (int i = 1; i < numbers.Count; i++)
        {
            if (numbers[i] != numbers[i - 1] + 1)
                return 0;
        }

        // 满足连续、同花色、3张以上
        return cardDatas.Count;
    }

    //int FillSequenceWithWilds(List<int> nums)
    //{
    //    int maxCount = 0;

    //    for (int start = 0; start < nums.Count; start++)
    //    {
    //        int current = 1;
    //        int wilds = nums.Count(n => n == -1);
    //        int prev = nums[start];

    //        if (prev == -1) continue;

    //        for (int i = start + 1; i < nums.Count; i++)
    //        {
    //            int now = nums[i];

    //            if (now == prev) continue;

    //            if (now == prev + 1)
    //            {
    //                current++;
    //                prev = now;
    //            }
    //            else if (wilds > 0 && now > prev + 1)
    //            {
    //                // 尝试用 wild 填 gap
    //                int gap = now - prev - 1;
    //                if (wilds >= gap)
    //                {
    //                    wilds -= gap;
    //                    current += gap + 1;
    //                    prev = now;
    //                }
    //                else break;
    //            }
    //            else break;
    //        }

    //        maxCount = Mathf.Max(maxCount, current);
    //    }

    //    return maxCount;
    //}
    //public void AddSpecialCardToPool(CardData specialCard)//加特殊卡
    //{
    //    if (specialCard == null || !specialCard.IsSpecial)
    //    {
    //        Debug.LogWarning("试图添加的卡不是特殊卡！");
    //        return;
    //    }

    //    cardPool.Add(specialCard);
    //    Shuffle(cardPool); // 重新洗牌，使其随机出现
    //}

}
