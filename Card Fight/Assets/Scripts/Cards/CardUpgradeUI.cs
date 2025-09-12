using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class CardUpgradeUI : MonoBehaviour
{

    [Header("Effect 構件（拖 Prefab）")]
    public GameObject redEffectPrefab;
    public GameObject blueEffectPrefab;
    public GameObject greenEffectPrefab;

    [Header("特效播放位置（3 个槽位）")]
    public Transform[] effectSlots; // 在场景放 3 个空物体作为特效位置

    [Header("所有技能（在 Inspector 配置每个花色的 1~4）")]
    public List<SkillButton> skillButtons = new List<SkillButton>();

    /// <summary>
    /// 打开并执行一次：随机从候选池里为三个槽位选技能（允许重复），播放特效，
    /// 然后在这三个结果中随机选一个作为“被触发的技能”，触发其 onClick 并 level++。
    /// </summary>
    //public void click()
    //{
    //    Show(Suit.Red);
    //}
    public void Show(Suit suit)
    {
        // 取出该花色所有技能条目
        var suitButtons = skillButtons.Where(sb => sb.suit == suit).ToList();
        if (suitButtons.Count == 0)
        {
            Debug.LogWarning($"CardUpgradeUI: 没有找到 {suit} 花色的技能配置。");
            return;
        }

        // 候选池规则：1~3 总是候选；当 1、2、3 的 level 都 >= 1 时，将 4 号加入候选池（如果存在）
        var basePool = suitButtons.Where(sb => sb.buttonIndex >= 1 && sb.buttonIndex <= 3).ToList();
        bool unlocked4 = basePool.All(sb => sb.level >= 1);
        if (unlocked4)
        {
            var b4 = suitButtons.FirstOrDefault(sb => sb.buttonIndex == 4);
            if (b4 != null) basePool.Add(b4);
        }

        if (basePool.Count == 0)
        {
            Debug.LogWarning("CardUpgradeUI: 候选池为空（可能未正确配置 1~3 的技能）。");
            return;
        }

        // 为每个槽位独立随机抽取一个技能（有放回，允许重复）
        var chosenForSlots = new List<SkillButton>();
        for (int i = 0; i < Mathf.Min(effectSlots.Length, 3); i++)
        {
            var pick = basePool[Random.Range(0, basePool.Count)];
            chosenForSlots.Add(pick);

            // 播放对应花色的特效在槽位位置
            var fxPrefab = GetEffectPrefab(suit);
            if (fxPrefab != null && effectSlots[i] != null)
            {
                var fx = Instantiate(fxPrefab, effectSlots[i].position, Quaternion.identity);
                Destroy(fx, 3f); // 播放后销毁（按特效长度调整）
            }
        }

        // 在这三个选项中再随机挑一个真正执行（模拟“自动点击”）
        var selected = chosenForSlots[Random.Range(0, chosenForSlots.Count)];
        selected.level++;
        Debug.Log($"[自动触发] {selected.displayName} 被触发，等级 -> {selected.level}");

        // 触发绑定的方法（例如 AttackFire）
        selected.onClick?.Invoke();
    }

    private GameObject GetEffectPrefab(Suit suit)
    {
        switch (suit)
        {
            case Suit.Red: return redEffectPrefab;
            case Suit.Blue: return blueEffectPrefab;
            case Suit.Green: return greenEffectPrefab;
        }
        return null;
    }

    // 辅助：外部能查询某个花色/索引的等级
    public int GetLevel(Suit s, int index1Based)
    {
        var btn = skillButtons.FirstOrDefault(x => x.suit == s && x.buttonIndex == index1Based);
        return btn != null ? btn.level : 0;
    }
}