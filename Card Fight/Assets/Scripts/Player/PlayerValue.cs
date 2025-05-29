using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum BuffType
    {
        Once,
        Stage,      // 本关有效
        Session,    // 本局游戏有效，玩家死亡后清空
        Permanent   // 永久有效，不会自动清除
    }

    public class PlayerValue : MonoBehaviour
    {
        // 基础属性（基础值）
        [Header("基础属性")]
        public float baseMoveSpeed = 10f;
        [Range(0.1f, 1f)]
        public float baseDashCooldown = 1f;
        public int baseMaxHP = 100;
        public int baseDefense = 2;
        public int baseAttack = 1;
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
        public int currentDefense;
        public int currentAttack;
        public float currentCritRate;
        public float currentAttackSpeed;
        public event Action<float> OnAttackSpeedChanged;
        public float CurrentAttackSpeed
        {
            get => currentAttackSpeed;
            set
            {
                if (Mathf.Approximately(currentAttackSpeed, value)) return;
                currentAttackSpeed = value;
                OnAttackSpeedChanged?.Invoke(currentAttackSpeed);
            }
        }

        public int currentHP;
        public int currentShield;

        // 统一管理所有 Buff
        private List<Buff> Buffs = new List<Buff>();

        private void Awake()
        {
            ResetStats();
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
        public void IncreaseStat(string stat, float value, BuffType buffType = BuffType.Session, float duration = 0f)
        {
            Buff newBuff = new Buff(stat, value, buffType, duration);
            Buffs.Add(newBuff);
            ApplyBuffEffect(newBuff);

            if (buffType == BuffType.Once && duration > 0f)
            {
                StartCoroutine(RemoveOnceBuffAfterDuration(newBuff, duration));
            }
        }

        private void ApplyBuffEffect(Buff buff)
        {
            ModifyCurrentStat(buff.stat, buff.value);
        }

        private void RemoveBuffEffect(Buff buff)
        {
            ModifyCurrentStat(buff.stat, -buff.value);
        }

        private void ModifyCurrentStat(string stat, float value)
        {
            switch (stat)
            {
                case "Attack":
                    currentAttack += Mathf.RoundToInt(value);
                    break;
                case "Defense":
                    currentDefense += Mathf.RoundToInt(value);
                    break;
                case "MaxHP":
                    currentMaxHP += Mathf.RoundToInt(value);
                    currentHP = currentMaxHP;
                    break;
                case "Crit":
                    currentCritRate += value;
                    break;
                case "MoveSpeed":
                    currentMoveSpeed += value;
                    break;
                case "DashCooldown":
                    currentDashCooldown += value;
                    break;
                case "AttackSpeed":
                    currentAttackSpeed += value;
                    break;
                default:
                    Debug.LogWarning("Unknown stat: " + stat);
                    break;
            }
        }
        // 应用单个永久加点到基础属性
        public void ApplyPermanentUpgrade(UpgradeData upgrade)
        {
            float value = upgrade.GetCurrentValue();

            switch (upgrade.stat)
            {
                case UpgradeData.StatType.Attack:
                    baseAttack = Mathf.RoundToInt(value);
                    break;
                case UpgradeData.StatType.HP:
                    baseMaxHP = Mathf.RoundToInt(value);
                    break;
                case UpgradeData.StatType.Defense:
                    baseDefense = Mathf.RoundToInt(value);
                    break;
                case UpgradeData.StatType.Crit:
                    baseCritRate = value;
                    break;
                case UpgradeData.StatType.MoveSpeed:
                    baseMoveSpeed = value;
                    break;
                case UpgradeData.StatType.DashCooldown:
                    baseDashCooldown = value;
                    break;
            }
        }

        // 应用所有永久加点
        public void ApplyAllPermanentUpgrades(List<UpgradeData> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.level > 0)
                    ApplyPermanentUpgrade(upgrade);
            }

            // 重新计算当前状态
            ResetStats();
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
        }

        /// <summary>
        /// 玩家死亡时调用，清除 Session 类型 Buff
        /// </summary>
        public void ClearSessionBuffs()
        {
            RemoveBuffsByType(BuffType.Session);
        }

        // 获取当前攻击力示例
        public int GetAttack() => currentAttack;
        public float GetCritRate() => currentCritRate;
    }

    // Buff 数据类
    public class Buff
    {
        public string stat;
        public float value;
        public BuffType buffType;
        public float duration; // 仅 Once 类型有效

        public Buff(string stat, float value, BuffType buffType, float duration = 0f)
        {
            this.stat = stat;
            this.value = value;
            this.buffType = buffType;
            this.duration = duration;
        }
    }
}