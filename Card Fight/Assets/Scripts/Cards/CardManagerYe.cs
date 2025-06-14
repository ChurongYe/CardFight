using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManagerYe : MonoBehaviour
{
    public CardDatabaseYe database;
    public GameObject cardPrefab;
    public Transform slotParent;
    public int maxSlots = 8;
    private List<CardSlotYe> slots = new List<CardSlotYe>();
    private List<CardData> cardPool;

    private List<CardDisplayYe> selectedCards = new List<CardDisplayYe>();

    void Start()
    {
        cardPool = database.GetShuffledNormalCards();
        StartCoroutine(Shuffle());
    }
    IEnumerator Shuffle()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < slotParent.childCount; i++)
        {
            CardSlotYe slot = slotParent.GetChild(i).GetComponent<CardSlotYe>();
            slots.Add(slot);

            if (i < 3)
                slot.SetLocked(false); // 前3个可用
            else
                slot.SetLocked(true);  // 后5个锁定
        }
        FillSlots(3); // 初始3个卡槽
    }
    void FillSlots(int count)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if (slot.currentCard == null && !slot.isCoolingDown && !slot.isLocked)
            {
                if (cardPool.Count == 0) break;

                int randomIndex = Random.Range(0, cardPool.Count);
                CardData card = cardPool[randomIndex];

                GameObject go = Instantiate(cardPrefab, slot.transform);
                CardDisplayYe display = go.GetComponent<CardDisplayYe>();
                display.SetCard(card);

                // 禁用拖拽、点击等（如果未来卡槽解锁，这个可以解除）
                display.SetInteractable(true);

                slot.currentCard = display;
                cardPool.RemoveAt(randomIndex);
            }
        }
    }
    public void SortSlotCards()//排序
    {
        var filledSlots = slots.Where(s => s.currentCard != null).ToList();

        filledSlots.Sort((a, b) =>
        {
            var ca = a.currentCard.data;
            var cb = b.currentCard.data;
            int suitCompare = ca.suit.CompareTo(cb.suit);
            return suitCompare != 0 ? suitCompare : ca.number.CompareTo(cb.number);
        });

        for (int i = 0; i < filledSlots.Count; i++)
        {
            filledSlots[i].transform.SetSiblingIndex(i); // UI层级排序
        }
    }

    public void OnCardClicked(CardDisplayYe card)
    {
        if (card.data.IsSpecial)
        {
            var sameSpecial = selectedCards.FirstOrDefault(c => c.data.IsSpecial);
            if (sameSpecial != null && sameSpecial != card)
            {
                //sameSpecial.ToggleSelect();
                selectedCards.Remove(sameSpecial);
            }
        }

        if (!card.isSelected)
        {
            //card.ToggleSelect();
            selectedCards.Add(card);
        }
        else
        {
            //card.ToggleSelect();
            selectedCards.Remove(card);
        }
    }

    public void TryPlaySelectedCards()
    {
        var valid = ValidateCombination(selectedCards);
        if (valid > 0)
        {
            int sum = selectedCards.Sum(c => c.data.number);

            if (selectedCards.Any(c => c.data.specialType == SpecialCardType.Double))
                sum *= 2;
            if (selectedCards.Any(c => c.data.specialType == SpecialCardType.Haste))
                sum += sum; // 或其他加速逻辑

            // Do attack logic with sum

            foreach (var card in selectedCards)
            {
                var slot = card.transform.parent.GetComponent<CardSlotYe>();
                Destroy(card.gameObject);
                slot.currentCard = null;
                slot.StartCooldown(() => RefillSlot(slot));
            }
            selectedCards.Clear();
        }
    }

    void RefillSlot(CardSlotYe slot)
    {
        CardData card = database.GetShuffledNormalCards().FirstOrDefault();
        if (card == null) return;
        GameObject go = Instantiate(cardPrefab, slot.transform);
        CardDisplayYe display = go.GetComponent<CardDisplayYe>();
        display.SetCard(card);
        slot.currentCard = display;
    }

    int ValidateCombination(List<CardDisplayYe> cards)
    {
        if (cards.Count < 3) return 0;

        var normalCards = cards.Where(c => !c.data.IsSpecial).OrderBy(c => c.data.number).ToList();
        var specialWilds = cards.Where(c => c.data.specialType == SpecialCardType.Wild).ToList();

        if (normalCards.Count + specialWilds.Count < 3) return 0;

        bool sameSuit = normalCards.All(c => c.data.suit == normalCards[0].data.suit);
        if (!sameSuit) return 0;

        var numbers = normalCards.Select(c => c.data.number).ToList();
        numbers.AddRange(Enumerable.Repeat(-1, specialWilds.Count)); // wild 填空
        numbers.Sort();

        int filled = FillSequenceWithWilds(numbers);

        return filled >= 3 ? filled : 0;
    }

    int FillSequenceWithWilds(List<int> nums)
    {
        int maxCount = 1, current = 1, wilds = nums.Count(n => n == -1);
        for (int i = 1; i < nums.Count; i++)
        {
            if (nums[i] == -1) continue;
            if (nums[i] == nums[i - 1]) continue;

            if (nums[i] == nums[i - 1] + 1)
                current++;
            else if (wilds > 0)
            {
                wilds--;
                current++;
            }
            else
                current = 1;

            maxCount = Mathf.Max(maxCount, current);
        }
        return maxCount;
    }
}
