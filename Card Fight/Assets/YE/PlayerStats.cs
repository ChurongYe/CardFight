//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//  namespace Core
//{
//    public class PlayerStats : MonoBehaviour
//    {
//        [Header("基础属性")]
//        public int baseAttack = 10;
//        public int baseDefense = 2;
//        public int baseMaxHP = 100;
//        public float baseCritRate = 0.05f;
//        public float baseDashCooldown = 1f;

//        [Header("战斗属性")]
//        public int currentHP;
//        public int currentShield;
//        public float currentCritRate;

//        public System.Action OnDeath;

//        private void Awake()
//        {
//            ApplyMetaUpgrades();
//            ResetStats();
//        }
//        public void ApplyMetaUpgrades()
//        {
//            foreach (var upgrade in MetaManager.Instance.upgrades)
//            {
//                switch (upgrade.stat)
//                {
//                    case UpgradeData.StatType.Attack:
//                        baseAttack += upgrade.level;
//                        break;
//                    case UpgradeData.StatType.HP:
//                        baseMaxHP += upgrade.level * 10;
//                        break;
//                    case UpgradeData.StatType.Crit:
//                        baseCritRate += upgrade.level;
//                        break;
//                    case UpgradeData.StatType.Defense:
//                        baseDefense += upgrade.level;
//                        break;
//                    case UpgradeData.StatType.DashCooldown:
//                        baseDefense += upgrade.level;
//                        break;

//                }
//            }
//        }
//        public void ResetStats()
//        {
//            currentHP = baseMaxHP;
//            currentShield = 0;
//            currentCritRate = baseCritRate;
//        }

//        public void TakeDamage(int damage)
//        {
//            int finalDamage = Mathf.Max(0, damage - baseDefense);

//            if (currentShield > 0)
//            {
//                int shieldAbsorb = Mathf.Min(currentShield, finalDamage);
//                currentShield -= shieldAbsorb;
//                finalDamage -= shieldAbsorb;
//            }

//            currentHP -= finalDamage;
//            if (currentHP <= 0)
//            {
//                currentHP = 0;
//                OnDeath?.Invoke();
//            }
//        }

//        public void Heal(int amount)
//        {
//            currentHP = Mathf.Min(currentHP + amount, baseMaxHP);
//        }

//        public void AddShield(int amount)
//        {
//            currentShield += amount;
//        }

//        public void AddCritRate(float value)
//        {
//            currentCritRate += value;
//        }

//        public void IncreaseStat(string stat, float value)
//        {
//            switch (stat)
//            {
//                case "Attack": baseAttack += Mathf.RoundToInt(value); break;
//                case "Defense": baseDefense += Mathf.RoundToInt(value); break;
//                case "MaxHP": baseMaxHP += Mathf.RoundToInt(value); currentHP = baseMaxHP; break;
//                case "Crit": AddCritRate(value); break;
//            }
//        }

//        public int GetAttack()
//        {
//            return baseAttack;
//        }

//        public float GetCritRate()
//        {
//            return currentCritRate;
//        }
//    }
//}
