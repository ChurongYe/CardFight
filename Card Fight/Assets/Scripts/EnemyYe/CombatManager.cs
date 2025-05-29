using Core;
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
}
