using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Core
{
    public class PlayerValue : MonoBehaviour
    {
        // 基础属性（基础值）
        [Header("基础属性")]
        public float baseMoveSpeed = 10f;
        [Range(0.1f, 1f)]
        public float baseDashCooldown = 1f;
        public int baseMaxHP = 100;
        public int baseDefense = 2;
        public int baseAttack = 10;
        public float baseCritRate = 0.05f;
        public float baseAttackSpeed = 0.25f;

        // 当前状态属性（包含buff加成）
        [Header("当前状态属性")]
        public float currentMoveSpeed;
        public event Action<float> OnMoveSpeedChanged;
        public float CurrentMoveSpeed
        {
            get => currentMoveSpeed;
            set
            {
                if (Mathf.Approximately(currentMoveSpeed, value)) return;
                currentMoveSpeed = value;
                OnMoveSpeedChanged?.Invoke(currentMoveSpeed);
            }
        }
        public float currentDashCooldown;
        public int currentMaxHP;
        public static int currentDefense;
        public int currentAttack;
        public float currentCritRate;
        public float currentAttackSpeed;
        //public event Action<float> OnAttackSpeedChanged;
        //public float CurrentAttackSpeed
        //{
        //    get => currentAttackSpeed;
        //    set
        //    {
        //        if (Mathf.Approximately(currentAttackSpeed, value)) return;
        //        currentAttackSpeed = value;
        //        OnAttackSpeedChanged?.Invoke(currentAttackSpeed);
        //    }
        //}

        public static int currentHP;
        public static int currentShield;
        private bool hasLifeStolenThisAttack = false;
        public static bool hasTriggeredLowHpShield = false;
        // 统一管理所有 Buff
        private List<Buff> Buffs = new List<Buff>();

        private void Awake()
        {
            ResetStats();
        }
        private void Update()
        {
            if (CardValue.bloodCritEnabled)
            {
                hpPercent = (float)currentHP / currentMaxHP;
                UpdateBloodCritBonus();
            }
        }
        public void ResetValue()
        {
            // 假设这是玩家脚本PlayerValue里的Start
            UpgradeManager.Instance.OnPlayerSpawned(this);
        }

        // 重置当前状态为基础值，并重新应用所有非Once Buff
        public void ResetStats()
        {
            currentMoveSpeed = baseMoveSpeed;
            currentDashCooldown = baseDashCooldown;
            currentMaxHP = baseMaxHP;
            currentDefense = baseDefense;
            currentAttack = baseAttack;
            currentCritRate = baseCritRate;
            currentAttackSpeed = baseAttackSpeed;

            currentHP = currentMaxHP;
            currentShield = 0;

            // 清除所有Once buff，重置后Once全清
            RemoveBuffsByType(BuffType.Once);

            // 重新应用除Once以外的Buff
            foreach (var buff in Buffs)
            {
                if (buff.buffType != BuffType.Once)
                {
                    ApplyBuffEffect(buff);
                }
            }
        }

        /// <summary>
        /// 增加属性buff
        /// </summary>
        /// <param name="stat">属性名，如 "Attack"</param>
        /// <param name="value">增益值</param>
        /// <param name="buffType">buff类型</param>
        /// <param name="duration">仅Once有效，持续时间</param>
        public void IncreaseStat(string stat, float value, BuffType buffType = BuffType.Session, float duration = 0f, bool isPercentage = true)
        {
            Buff newBuff = new Buff(stat, value, buffType, duration, isPercentage);
            Buffs.Add(newBuff);
            ApplyBuffEffect(newBuff);

            if (buffType == BuffType.Once && duration > 0f)
            {
                StartCoroutine(RemoveOnceBuffAfterDuration(newBuff, duration));
            }
        }
        private void ApplyBuffEffect(Buff buff)
        {
            ModifyCurrentStat(buff.stat, buff.value, buff.isPercentage);
        }
        private void RemoveBuffEffect(Buff buff)
        {
            ModifyCurrentStat(buff.stat, -buff.value, buff.isPercentage);
        }

        private void ModifyCurrentStat(string stat, float value, bool isPercentage = true)
        {
            float finalValue = value;

            switch (stat)
            {
                case "Attack":
                    finalValue = isPercentage ? baseAttack * value : value;
                    currentAttack += Mathf.RoundToInt(finalValue);
                    break;
                case "Defense":
                    finalValue = isPercentage ? baseDefense * value : value;
                    currentDefense += Mathf.RoundToInt(finalValue);
                    break;
                case "currentHP":
                    finalValue = isPercentage ? baseMaxHP * value : value;
                    if (currentHP < currentMaxHP)
                    {
                        int heal = Mathf.RoundToInt(finalValue);
                        currentHP = Mathf.Min(currentHP + heal, currentMaxHP);
                    }
                    break;
                case "MaxHP":
                    finalValue = isPercentage ? baseMaxHP * value : value;
                    currentMaxHP += Mathf.RoundToInt(finalValue);
                    break;
                case "Crit":
                    finalValue = isPercentage ? baseCritRate * value : value;
                    currentCritRate += finalValue;
                    break;
                case "MoveSpeed":
                    finalValue = isPercentage ? baseMoveSpeed * value : value;
                    CurrentMoveSpeed += finalValue;
                    break;
                case "DashCooldown":
                    finalValue = isPercentage ? baseDashCooldown * value : value;
                    currentDashCooldown += finalValue;
                    break;
                case "AttackSpeed":
                    finalValue = isPercentage ? baseAttackSpeed * value : value;
                    currentAttackSpeed += finalValue;
                    break;

                case "AttackFire":
                    // 留空
                    break;
                case "AttackPlayerFire":
                    // 留空
                    break;
                case "Fireballs":
                    // 留空
                    break;
                case "BloodCrit":
                    // 留空
                    break;
                case "AttackLighting":
                    // 留空
                    break;
                case "AddOneLight":
                    // 留空
                    break;
                case "LightingPlus":
                    // 留空
                    break;
                case "AttackLight":
                    finalValue = isPercentage ? baseAttack * value : value;
                    baseAttack += Mathf.RoundToInt(finalValue);
                    if (baseAttack > currentAttack)
                    {
                        currentAttack = baseAttack;
                    }
                    break;
                case "AddWallDefense":
                    finalValue = isPercentage ? baseDefense * value : value;
                    if (isPercentage && finalValue < 1f)
                    {
                        finalValue = 1f;
                    }

                    baseDefense += Mathf.RoundToInt(finalValue);

                    if (baseDefense > currentDefense)
                    {
                        currentDefense = baseDefense;
                    }
                    break;
                case "ThornsDamage":
                    // 留空
                    break;
                case "ThornsShield":
                    // 留空
                    break;
                case "AddTriggerLowHpShield":
                    // 留空
                    break;
                case "AddTreeBlood1":
                    finalValue = isPercentage ? baseMaxHP * value : value;
                    baseMaxHP += Mathf.RoundToInt(finalValue);
                    if (baseMaxHP > currentMaxHP)
                    {
                        currentMaxHP = baseMaxHP;
                    }
                    break;
                case "AddTreeBlood2":
                    finalValue = isPercentage ? currentMaxHP * value : value;

                    if (finalValue > 0)
                    {
                        float hpPercent = (float)currentHP / currentMaxHP; // 记录当前血量百分比

                        currentMaxHP += Mathf.RoundToInt(finalValue);       // 增加最大血量
                        currentHP = Mathf.RoundToInt(currentMaxHP * hpPercent); // 按比例更新当前血量
                    }
                    break;
                case "AddStealLevel":
                    // 留空
                    break;
                case "AddDashTree":
                    finalValue = isPercentage ? baseDashCooldown * value : value;

                    if (isPercentage && finalValue < 0.05f)
                    {
                        finalValue = 0.05f;
                    }

                    baseDashCooldown -= finalValue;

                    // 限制在 [0.1f, 1f] 范围内
                    baseDashCooldown = Mathf.Clamp(baseDashCooldown, 0.1f, 1f);

                    if (baseDashCooldown < currentDashCooldown)
                    {
                        currentDashCooldown = baseDashCooldown;
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown stat: " + stat);
                    break;
            }
        }
        // 应用单个永久加点到基础属性
        public void ApplyPermanentUpgrade(UpgradeData upgrade)
        {
            // 如果当前等级没有超过已加等级，无需再加
            if (upgrade.level <= upgrade.appliedLevel)
                return;

            // 计算从 appliedLevel+1 到当前 level 之间的所有等级加成总和
            float bonus = 0f;
            for (int i = upgrade.appliedLevel + 1; i <= upgrade.level; i++)
            {
                bonus += upgrade.GetLevelValue(i);
            }

            // 应用加成
            switch (upgrade.stat)
            {
                case UpgradeData.StatType.Attack:
                    baseAttack += Mathf.RoundToInt(bonus);
                    break;
                case UpgradeData.StatType.HP:
                    baseMaxHP += Mathf.RoundToInt(bonus);
                    break;
                case UpgradeData.StatType.Defense:
                    baseDefense += Mathf.RoundToInt(bonus);
                    break;
                case UpgradeData.StatType.Crit:
                    baseCritRate += bonus;
                    break;
                case UpgradeData.StatType.MoveSpeed:
                    baseMoveSpeed += bonus;
                    break;
                case UpgradeData.StatType.DashCooldown:
                    baseDashCooldown *= (1 - bonus);
                    break;
            }

            // 更新已加等级，防止重复加成
            upgrade.appliedLevel = upgrade.level;
        }

        // 一次性应用所有永久加点
        public void ApplyAllPermanentUpgrades(List<UpgradeData> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.level > 0)
                    ApplyPermanentUpgrade(upgrade);
            }

            ResetStats(); // 更新最终属性
        }
        /// <summary>
        /// //////////血量越低，暴击率越高
        /// </summary>
        private float lastBloodCritBonus = 0f;
        public float hpPercent;

        public void UpdateBloodCritBonus()
        {
            if (!CardValue.bloodCritEnabled || CardValue.bloodCritLevel <= 0) return;

            int lostPercentage = Mathf.FloorToInt((1f - hpPercent) * 10); // 每损失10%血量

            // 每级暴击加成逐渐增加：如1级+3%，2级+4%，3级+5%...
            float perSegmentBonus = 0.02f + 0.01f * CardValue.bloodCritLevel;
            float newBonus = lostPercentage * perSegmentBonus;

            float delta = newBonus - lastBloodCritBonus;
            if (!Mathf.Approximately(delta, 0f))
            {
                IncreaseStat("Crit", delta, BuffType.Session , 0f, false);
                lastBloodCritBonus = newBonus;
            }
        }
        /// <summary>
        /// ////////////吸血
        /// </summary>
        /// <param name="damageDealt"></param>

        public void ResetLifeStealFlag()
        {
            hasLifeStolenThisAttack = false;
        }
        public void TryLifeSteal(int damageDealt)
        {
            if (hasLifeStolenThisAttack) return;
            float stealPercent = 0f;

            switch (CardValue.LifeStealLevel)
            {
                case 1: stealPercent = 0.1f; break;
                case 2: stealPercent = 0.15f; break;
                case 3: stealPercent = 0.25f; break;
                default: return; // 未开启吸血
            }

            int healAmount = Mathf.RoundToInt(damageDealt * stealPercent);
            if (currentHP < currentMaxHP)
            {
                currentHP = Mathf.Min(currentHP + healAmount, currentMaxHP);
                hasLifeStolenThisAttack = true;

                Debug.Log($"加血: {healAmount}");
                // 可选：回血特效、飘字提示
                // ShowHealEffect(healAmount);
            }
        }
        private IEnumerator RemoveOnceBuffAfterDuration(Buff buff, float duration)
        {
            yield return new WaitForSeconds(duration);

            if (Buffs.Contains(buff))
            {
                RemoveBuffEffect(buff);
                Buffs.Remove(buff);
            }
        }

        /// <summary>
        /// 清除指定类型的 Buff
        /// </summary>
        /// <param name="type">Buff 类型</param>
        public void RemoveBuffsByType(BuffType type)
        {
            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                if (Buffs[i].buffType == type)
                {
                    RemoveBuffEffect(Buffs[i]);
                    Buffs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 关卡结束时调用，清除 Stage 类型 Buff
        /// </summary>
        public void ClearStageBuffs()
        {
            RemoveBuffsByType(BuffType.Stage);
            // 清除所有Once buff，重置后Once全清
            RemoveBuffsByType(BuffType.Once);
            currentMoveSpeed = baseMoveSpeed;
            currentDashCooldown = baseDashCooldown;
            currentMaxHP = baseMaxHP;
            currentDefense = baseDefense;
            currentAttack = baseAttack;
            //currentCritRate = baseCritRate;
            currentAttackSpeed = baseAttackSpeed;
            currentShield = 0;
            if (currentHP > currentMaxHP)
            { currentHP = currentMaxHP; }
            hasTriggeredLowHpShield = false;

        }

        /// <summary>
        /// 玩家死亡时调用，清除 Session 类型 Buff
        /// </summary>
        public void ClearSessionBuffs()
        {
            RemoveBuffsByType(BuffType.Session);
        }

        // 获取当前攻击力示例
        public int GetcurrntAttack() => currentAttack;
        public int GetBaseAttack() => baseAttack;
        public float GetCritRate() => currentCritRate;

    }

    // Buff 数据类
    public enum BuffType
    {
        Once,
        Stage,      // 本关有效
        Session,    // 本局游戏有效，玩家死亡后清空
        Permanent   // 永久有效，不会自动清除
    }
    public class Buff
    {
        public string stat;
        public float value;
        public BuffType buffType;
        public float duration;
        public bool isPercentage;

        public Buff(string stat, float value, BuffType buffType, float duration = 0f, bool isPercentage = true)
        {
            this.stat = stat;
            this.value = value;
            this.buffType = buffType;
            this.duration = duration;
            this.isPercentage = isPercentage;
        }
    }
}