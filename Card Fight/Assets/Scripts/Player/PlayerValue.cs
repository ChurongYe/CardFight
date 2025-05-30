using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    public class PlayerValue : MonoBehaviour
    {
        // �������ԣ�����ֵ��
        [Header("��������")]
        public float baseMoveSpeed = 10f;
        [Range(0.1f, 1f)]
        public float baseDashCooldown = 1f;
        public int baseMaxHP = 100;
        public int baseDefense = 2;
        public int baseAttack = 1;
        public float baseCritRate = 0.05f;
        public float baseAttackSpeed = 0.25f;

        // ��ǰ״̬���ԣ�����buff�ӳɣ�
        [Header("��ǰ״̬����")]
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

        // ͳһ�������� Buff
        private List<Buff> Buffs = new List<Buff>();

        private void Awake()
        {
            ResetStats();
        }
        public void ResetValue()
        {
            // ����������ҽű�PlayerValue���Start
            UpgradeManager.Instance.OnPlayerSpawned(this);
        }

        // ���õ�ǰ״̬Ϊ����ֵ��������Ӧ�����з�Once Buff
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

            // �������Once buff�����ú�Onceȫ��
            RemoveBuffsByType(BuffType.Once);

            // ����Ӧ�ó�Once�����Buff
            foreach (var buff in Buffs)
            {
                if (buff.buffType != BuffType.Once)
                {
                    ApplyBuffEffect(buff);
                }
            }
        }

        /// <summary>
        /// ��������buff
        /// </summary>
        /// <param name="stat">���������� "Attack"</param>
        /// <param name="value">����ֵ</param>
        /// <param name="buffType">buff����</param>
        /// <param name="duration">��Once��Ч������ʱ��</param>
        public void IncreaseStat(string stat, float value,BuffType buffType = BuffType.Session, float duration = 0f, bool isPercentage = true)
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
                    CurrentAttackSpeed += finalValue;
                    break;
                default:
                    Debug.LogWarning("Unknown stat: " + stat);
                    break;
            }
        }
        // Ӧ�õ������üӵ㵽��������
        public void ApplyPermanentUpgrade(UpgradeData upgrade)
        {
            // �����ǰ�ȼ�û�г����Ѽӵȼ��������ټ�
            if (upgrade.level <= upgrade.appliedLevel)
                return;

            // ����� appliedLevel+1 ����ǰ level ֮������еȼ��ӳ��ܺ�
            float bonus = 0f;
            for (int i = upgrade.appliedLevel + 1; i <= upgrade.level; i++)
            {
                bonus += upgrade.GetLevelValue(i);
            }

            // Ӧ�üӳ�
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

            // �����Ѽӵȼ�����ֹ�ظ��ӳ�
            upgrade.appliedLevel = upgrade.level;
        }

        // һ����Ӧ���������üӵ�
        public void ApplyAllPermanentUpgrades(List<UpgradeData> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.level > 0)
                    ApplyPermanentUpgrade(upgrade);
            }

            ResetStats(); // ������������
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
        /// ���ָ�����͵� Buff
        /// </summary>
        /// <param name="type">Buff ����</param>
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
        /// �ؿ�����ʱ���ã���� Stage ���� Buff
        /// </summary>
        public void ClearStageBuffs()
        {
            RemoveBuffsByType(BuffType.Stage);
        }

        /// <summary>
        /// �������ʱ���ã���� Session ���� Buff
        /// </summary>
        public void ClearSessionBuffs()
        {
            RemoveBuffsByType(BuffType.Session);
        }

        // ��ȡ��ǰ������ʾ��
        public int GetAttack() => currentAttack;
        public float GetCritRate() => currentCritRate;

    }

    // Buff ������
    public enum BuffType
    {
        Once,
        Stage,      // ������Ч
        Session,    // ������Ϸ��Ч��������������
        Permanent   // ������Ч�������Զ����
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