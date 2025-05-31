using Core;
using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    private PlayerValue playerValue;

    private void Awake()
    {
        Instance = this;
        playerValue = FindObjectOfType<PlayerValue>();
    }

    public void DealDamage(GameObject target)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseAtk = playerValue.GetAttack();
            float critRate = playerValue.GetCritRate();
            bool isCrit = Random.value < critRate;

            int finalDamage = baseAtk;
            if (isCrit) finalDamage = Mathf.RoundToInt(baseAtk * 1.5f);

            hurtable.TakeDamage(finalDamage, isCrit);
        }
    }
    public void DealFireDamage(GameObject target, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseDamage = playerValue.GetAttack();

            // 根据灼烧等级乘以倍率
            float[] damageMultipliers = { 1f, 1.15f, 1.45f }; // 对应等级1~3
            float multiplier = damageMultipliers[Mathf.Clamp(CardValue.FireLevel - 1, 0, 2)];

            int burnDamage = Mathf.RoundToInt(baseDamage * multiplier);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, burnDamage, tickInterval));
        }
    }
    public void DealPlayerFireDamage(GameObject target, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseDamage = playerValue.GetAttack();

            // 根据灼烧等级乘以倍率
            float[] damageMultipliers = { 1f, 1.15f, 1.45f }; // 对应等级1~3
            float multiplier = damageMultipliers[Mathf.Clamp(CardValue.PlayerFireLevel - 1, 0, 2)];

            int burnDamage = Mathf.RoundToInt(baseDamage * multiplier);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, burnDamage, tickInterval));
        }
    }
    public void DealFireballDamage(GameObject target, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            float[] damageByLevel = { 10f, 20f, 30f }; // 1~3级对应伤害
            int damage = Mathf.RoundToInt(damageByLevel[Mathf.Clamp(CardValue.fireballLevel - 1, 0, 2)]);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, damage, tickInterval));
        }
    }


    private IEnumerator ApplyBurning(IHurtable hurtable, int startDamage, float interval)
    {
        // 每跳的百分比范围：{ min, max }
        Vector2[] percentRanges = new Vector2[]
        {
        new Vector2(1f, 1f),       // 第一次：100%
        new Vector2(0.5f, 0.7f),   // 第二次：50%-70%
        new Vector2(0.3f, 0.4f),   // 第三次：30%-40%
        new Vector2(0.1f, 0.2f)    // 第四次：10%-20%
        };

        foreach (var range in percentRanges)
        {
            if (hurtable == null) yield break;

            float randomPercent = Random.Range(range.x, range.y);
            int damage = Mathf.Max(1, Mathf.RoundToInt(startDamage * randomPercent));
            hurtable.TakeDamage(damage, false); // 火焰不能暴击

            yield return new WaitForSeconds(interval);
        }
    }
}
