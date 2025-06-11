using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMechanicsYe : MonoBehaviour
{
    public enum Suit { Red, Green, Blue }
    public enum SpecialType { None, Wild, Summon, Double, Accelerate }

    public class Card
    {
        public Suit suit;
        public int number;
        public SpecialType specialType;

        public Card(Suit suit, int number)
        {
            this.suit = suit;
            this.number = number;
            this.specialType = SpecialType.None;
        }

        public Card(SpecialType specialType)
        {
            this.specialType = specialType;
            this.suit = Suit.Red;
            this.number = -1;
        }

        public bool IsSpecial() => specialType != SpecialType.None;
    }

    public class CardDeck
    {
        private List<Card> cards = new List<Card>();

        public CardDeck()
        {
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                for (int group = 0; group < 3; group++)
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        cards.Add(new Card(suit, i));
                    }
                }
            }
        }

        public void AddSpecialCard(SpecialType type)
        {
            cards.Add(new Card(type));
        }

        public Card DrawRandomCard()
        {
            if (cards.Count == 0) return null;
            int index = UnityEngine.Random.Range(0, cards.Count);
            Card drawn = cards[index];
            cards.RemoveAt(index);
            return drawn;
        }
    }

    public class CardSlot
    {
        public Card card;
        public float cooldown;
        public bool IsCoolingDown => cooldown > 0f;

        public void TickCooldown(float deltaTime)
        {
            if (IsCoolingDown)
            {
                cooldown -= deltaTime;
                if (cooldown < 0f) cooldown = 0f;
            }
        }

        public bool IsEmpty() => card == null;
    }

    public class CardManager : MonoBehaviour
    {
        public int initialSlotCount = 3;
        public int maxSlotCount = 8;
        public float slotCooldown = 2f;

        public List<CardSlot> slots = new List<CardSlot>();
        public CardDeck deck = new CardDeck();
        public List<Card> selectedCards = new List<Card>();

        private void Start()
        {
            for (int i = 0; i < initialSlotCount; i++)
            {
                AddSlot();
            }
        }

        private void Update()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty() && slot.IsCoolingDown)
                {
                    slot.TickCooldown(Time.deltaTime);
                    if (!slot.IsCoolingDown)
                    {
                        DrawToSlot(i);
                    }
                }
            }
        }

        public bool AddSlot()
        {
            if (slots.Count >= maxSlotCount)
            {
                Debug.Log("卡槽已满");
                return false;
            }

            CardSlot newSlot = new CardSlot();
            newSlot.card = deck.DrawRandomCard();
            newSlot.cooldown = 0;
            slots.Add(newSlot);

            SortSlots();
            return true;
        }

        private void DrawToSlot(int index)
        {
            slots[index].card = deck.DrawRandomCard();
            SortSlots();
        }

        private void SortSlots()
        {
            slots.Sort((a, b) =>
            {
                if (a.card == null) return 1;
                if (b.card == null) return -1;
                if (a.card.suit == b.card.suit)
                    return a.card.number.CompareTo(b.card.number);
                return a.card.suit.CompareTo(b.card.suit);
            });
        }

        public void PlaySelectedCards()
        {
            if (selectedCards.Count == 0) return;

            var specialCards = selectedCards.FindAll(c => c.IsSpecial());
            var normalCards = selectedCards.FindAll(c => !c.IsSpecial());

            if (specialCards.Count > 1)
            {
                Debug.Log("不能同时使用多个特殊卡");
                return;
            }

            int total = 0;
            if (IsValidStraight(normalCards) || IsValidSameNumber(normalCards))
            {
                total = GetCardSum(normalCards);
            }
            else
            {
                Debug.Log("不满足特殊组合");
                return;
            }

            if (specialCards.Count == 1)
            {
                var sp = specialCards[0].specialType;
                switch (sp)
                {
                    case SpecialType.Wild:
                        break;
                    case SpecialType.Summon:
                    case SpecialType.Accelerate:
                        total += GetCardSum(normalCards);
                        break;
                    case SpecialType.Double:
                        total *= 2;
                        break;
                }
            }

            Debug.Log($"触发效果，最终值为：{total}");
            RemovePlayedCards();
        }

        private bool IsValidStraight(List<Card> cards)
        {
            if (cards.Count < 3) return false;
            cards.Sort((a, b) => a.number.CompareTo(b.number));
            Suit suit = cards[0].suit;
            for (int i = 1; i < cards.Count; i++)
            {
                if (cards[i].suit != suit || cards[i].number != cards[i - 1].number + 1)
                    return false;
            }
            return true;
        }

        private bool IsValidSameNumber(List<Card> cards)
        {
            if (cards.Count < 2) return false;
            int num = cards[0].number;
            Suit suit = cards[0].suit;
            foreach (var c in cards)
            {
                if (c.number != num || c.suit != suit)
                    return false;
            }
            return true;
        }

        private int GetCardSum(List<Card> cards)
        {
            int sum = 0;
            foreach (var c in cards)
            {
                if (!c.IsSpecial()) sum += c.number;
            }
            return sum;
        }

        private void RemovePlayedCards()
        {
            foreach (var played in selectedCards)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].card == played)
                    {
                        slots[i].card = null;
                        slots[i].cooldown = slotCooldown;
                        break;
                    }
                }
            }
            selectedCards.Clear();
        }

        public void ToggleCardSelection(Card card)
        {
            if (card.IsSpecial())
            {
                var existing = selectedCards.Find(c => c.IsSpecial());
                if (existing != null && existing != card)
                {
                    selectedCards.Remove(existing);
                }
            }

            if (selectedCards.Contains(card))
                selectedCards.Remove(card);
            else
                selectedCards.Add(card);
        }
    }
}