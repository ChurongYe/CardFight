using Core;
using System.Collections;
using Unity.Burst.CompilerServices;
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
            int baseAtk = playerValue.GetcurrntAttack();
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
            int baseDamage = playerValue.GetBaseAttack();

            // �������յȼ����Ա���
            float[] damageMultipliers = { 1f, 1.15f, 1.45f }; // ��Ӧ�ȼ�1~3
            float multiplier = damageMultipliers[Mathf.Clamp(CardValue.FireLevel - 1, 0, 2)];

            int burnDamage = Mathf.RoundToInt(baseDamage * multiplier);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, burnDamage, tickInterval));
        }
    }
    public void DealPlayerFireDamage(GameObject target, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseDamage = playerValue.GetBaseAttack();

            // �������յȼ����Ա���
            float[] damageMultipliers = { 1f, 1.15f, 1.45f }; // ��Ӧ�ȼ�1~3
            float multiplier = damageMultipliers[Mathf.Clamp(CardValue.PlayerFireLevel - 1, 0, 2)];

            int burnDamage = Mathf.RoundToInt(baseDamage * multiplier);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, burnDamage, tickInterval));
        }
    }
    public void DealFireballDamage(GameObject target, float tickInterval = 0.5f)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            float[] damageByLevel = { 10f, 20f, 30f }; // 1~3����Ӧ�˺�
            int damage = Mathf.RoundToInt(damageByLevel[Mathf.Clamp(CardValue.fireballLevel - 1, 0, 2)]);

            target.GetComponent<MonoBehaviour>().StartCoroutine(ApplyBurning(hurtable, damage, tickInterval));
        }
    }
    public void DealLightingDamage(GameObject target)
    {
        if (target.TryGetComponent<IHurtable>(out var hurtable))
        {
            int baseAttack = playerValue.GetBaseAttack();

            // �ȼ���1 = 5%��2 = 10%��3 = 20%
            float multiplier = 0.05f;
            if (CardValue.AttackLighting == 2) multiplier = 0.10f;
            else if (CardValue.AttackLighting >= 3) multiplier = 0.2f;

            int bonusDamage = Mathf.RoundToInt(baseAttack * multiplier);
            int totalDamage = baseAttack + bonusDamage;
            hurtable.TakeDamage(totalDamage, false);

            // 30% ����ѣ��
            if (Random.value < 0.3f && target.TryGetComponent<IStunnable>(out var stunnable))
            {
                stunnable.Stun(CardValue. LightingPlus);
            }
        }
    }

    private IEnumerator ApplyBurning(IHurtable hurtable, int startDamage, float interval)
    {
        // ÿ���İٷֱȷ�Χ��{ min, max }
        Vector2[] percentRanges = new Vector2[]
        {
        new Vector2(1f, 1f),       // ��һ�Σ�100%
        new Vector2(0.5f, 0.7f),   // �ڶ��Σ�50%-70%
        new Vector2(0.3f, 0.4f),   // �����Σ�30%-40%
        };

        foreach (var range in percentRanges)
        {
            if (hurtable == null) yield break;

            float randomPercent = Random.Range(range.x, range.y);
            int damage = Mathf.Max(1, Mathf.RoundToInt(startDamage * randomPercent));
            hurtable.TakeDamage(damage, false); // ���治�ܱ���

            yield return new WaitForSeconds(interval);
        }
    }
}
