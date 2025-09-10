using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 三个固定槽位，每次根据当前花色的候选池（1~3 或 1~4）独立随机抽取填充。
/// 抽取时允许重复，4号一旦解锁与其他3个按钮拥有相同出现概率。
/// </summary>
public class CardUpgradeUI : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Button button;
        public Text label;
    }

    [Header("固定三个槽位（Inspector 里拖 0..2 三个 Slot）")]
    public Slot[] slots = new Slot[3];

    // 外部设置的测试花色
    [Header("测试用花色")]
    public Suit testSuit = Suit.Red;

    private readonly Dictionary<Suit, int[]> levels = new Dictionary<Suit, int[]>
    {
        { Suit.Red,   new int[4] },
        { Suit.Blue,  new int[4] },
        { Suit.Green, new int[4] },
    };

    private Suit currentSuit;
    private int[] curLevels;
    private int[] slotToIndex = new int[3];

    void Start()
    {
        // 绑定按钮点击
        for (int i = 0; i < slots.Length; i++)
        {
            int si = i;
            if (slots[si].button != null)
            {
                slots[si].button.onClick.RemoveAllListeners();
                slots[si].button.onClick.AddListener(() => OnSlotClick(si));
            }
        }

        // **测试用：游戏开始就显示 UI**
        Show(testSuit);
    }

    public void Show(Suit suit)
    {
        currentSuit = suit;
        curLevels = levels[suit];

        List<int> pool = new List<int> { 0, 1, 2 };
        bool unlocked4 = curLevels[0] >= 1 && curLevels[1] >= 1 && curLevels[2] >= 1;
        if (unlocked4) pool.Add(3);

        for (int i = 0; i < slots.Length; i++)
        {
            int pick = pool[Random.Range(0, pool.Count)];
            slotToIndex[i] = pick;

            if (slots[i].button != null) slots[i].button.gameObject.SetActive(true);
            if (slots[i].label != null) slots[i].label.text = $"{GetSuitName(suit)}{pick + 1}  {curLevels[pick]}级";
        }

        gameObject.SetActive(true);
    }

    private void OnSlotClick(int slot)
    {
        int idx = slotToIndex[slot];
        curLevels[idx]++;
        Debug.Log($"{currentSuit} {idx + 1} 被点，等级提升到 {curLevels[idx]}级");

        gameObject.SetActive(false);
    }

    private string GetSuitName(Suit suit)
    {
        switch (suit)
        {
            case Suit.Red: return "红";
            case Suit.Blue: return "蓝";
            case Suit.Green: return "绿";
        }
        return "";
    }

    public int GetLevel(Suit s, int index) => levels[s][index];
    public void ResetLevels(Suit s) => levels[s] = new int[4];
}