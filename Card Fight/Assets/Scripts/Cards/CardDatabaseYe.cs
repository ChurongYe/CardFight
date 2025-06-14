using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum Suit { Red, Green, Blue, Special }
public enum SpecialCardType { Wild, Summon, Double, Haste }

[Serializable]
public class CardData
{
    public Suit suit;
    public int number; // 1~9 for normal, -1 for special
    public SpecialCardType specialType;
    public Sprite sprite;
    public bool IsSpecial => suit == Suit.Special;
}

public class CardDatabaseYe : MonoBehaviour
{
    public List<CardData> allCards; // 预设好的所有卡牌

    public List<CardData> GetShuffledNormalCards()
    {
        List<CardData> normalCards = allCards.Where(c => !c.IsSpecial).ToList();
        return normalCards.OrderBy(_ => UnityEngine.Random.value).ToList();
    }

    public CardData GetRandomCardFromPool(List<CardData> pool)
    {
        if (pool.Count == 0) return null;
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }
}
