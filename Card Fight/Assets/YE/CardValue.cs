using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static Core.CardValue;
using static UnityEngine.Rendering.DebugUI;

namespace Core
{
    public class CardValue : MonoBehaviour
    {
        private PlayerValue playerValue;
        void Start()
        {
            playerValue = FindObjectOfType<Core.PlayerValue>();
            attackType = AttackType.NormalMelee | AttackType.NormalRanged;
            FireLevel = 0; // Ĭ��Ϊ0�������3��
            PlayerFireLevel = 0;
            fireballLevel = 0;
            bloodCritLevel = 0; // Ѫ��Խ�ͱ���Խ�߼��ܵȼ������ 3 ��
            bloodCritEnabled = false;
            AttackLighting = 0;
            AddLighting = false;
            AttackLight = 0;
            LightingPlus = 0;
            WallDefense = 0;
            ThornsLevel = 0;
            ThornsShieldLevel = 0;
            TriggerLowHpShield = 0;
            TreeBlood1 = 0;
            TreeBlood2 = 0;
            LifeStealLevel = 0;
            DashTree = 0;
            PlayerFire = false;
            fireball = false;
        }
        //������
        public void AddAttack1()
        {
            playerValue.IncreaseStat("Attack", 1f, BuffType.Stage, 0, false);
        }
        public void AddDefense1()
        {
            playerValue.IncreaseStat("Defense", 1f, BuffType.Stage,0,false);
        }
        public void AddcurrentHP1()
        {
            playerValue.IncreaseStat("currentHP", 1f, BuffType.Session , 0, false);
        }
        public void AddCrit123()
        {
            float critValue = UnityEngine.Random.Range(1f, 2f);
            playerValue.IncreaseStat("Crit", critValue, BuffType.Once,5f);
        }
        public void AddDefense123()
        {
            float critValue = UnityEngine.Random.Range(1f, 2f);
            playerValue.IncreaseStat("Defense", critValue, BuffType.Once, 5f);
        }
        public void AddcurrentHP123()
        {
            float critValue = UnityEngine.Random.Range(1f, 2f);
            playerValue.IncreaseStat("currentHP", critValue, BuffType.Session , 5f);
        }
        //������
        public void Summon()
        {
            //�����߼�
        }
        //�ӵ���
        [System.Flags]
        public enum AttackType
        {
            None = 0,
            NormalMelee = 1 << 0,
            NormalRanged = 1 << 1,
            Fire = 1 << 2,
            PlayerFire = 1 << 3,
            Lightning = 1 << 4
        }

        public static AttackType attackType = AttackType.NormalMelee | AttackType.NormalRanged;
        public static int FireLevel = 0; // Ĭ��Ϊ0�������3��
        public static int PlayerFireLevel = 0; // Ĭ��Ϊ0�������3��
        public void AttackFire()
        {
            if (FireLevel == 0)
            {
                attackType |= AttackType.Fire; // ��� Fire ����
                attackType &= ~AttackType.NormalMelee;
                playerValue.IncreaseStat("AttackFire", 0, BuffType.Session);
                //�����߼�
            }
            if (FireLevel < 3)
                FireLevel++;
        }//��1
        public static bool PlayerFire = false ;
        public void AttackPlayerFire()
        {
            if (FireLevel == 0) return;
            if (PlayerFireLevel == 0)
            {
                PlayerFire = true;
                attackType |= AttackType.PlayerFire; // ��� PlayerFire ����
                playerValue.IncreaseStat("AttackPlayerFire", 0, BuffType.Session);
                //�����߼�
            }
            if (PlayerFireLevel < 3)
                PlayerFireLevel++;
        }//��2
        public static int fireballLevel = 0; // Ĭ��Ϊ0�����3��
        public static bool fireball = false ;
        public void Fireballs()//��3
        {
            if (fireballLevel == 0)
            {
                fireball = true;
                playerValue.IncreaseStat("Fireballs", 0, BuffType.Session);
                //�����߼�
            }

            if (fireballLevel < 3)
                fireballLevel++;
        }
        public static int bloodCritLevel = 0; // Ѫ��Խ�ͱ���Խ�߼��ܵȼ������ 3 ��
        public static bool bloodCritEnabled = false;

        public void ActivateBloodCrit()//��4
        {
            if (bloodCritLevel == 0)
            {
                bloodCritEnabled = true;
                playerValue.IncreaseStat("BloodCrit", 0, BuffType.Session);
                // ����Ը�����Ҫ��Ӷ���/��ʾ��
            }

            if (bloodCritLevel < 3)
                bloodCritLevel++;
        }
        public static int AttackLighting = 0;
        public static bool AddLighting = false;
        public void Lighting()//��1
        {
            if (AttackLighting == 0)
            {
                AddLighting = true;
                playerValue.IncreaseStat("AttackLighting", 0, BuffType.Session);
                // ����Ը�����Ҫ��Ӷ���/��ʾ��
            }

            if (AttackLighting < 3)
                AttackLighting++;
        }

        public static int AttackLight = 0;
        public void AddAttackLight() //��3
        {
            if (AttackLight < 3)
            {
                AttackLight++;
                playerValue.IncreaseStat("AttackLight", 0.3f, BuffType.Session, 0, true);
            }
        }
        public static int LightingPlus = 0;
        public void AddLightingPlus() //��4
        {
            if (AttackLighting == 0) return;
            if (LightingPlus == 0)
            {
                playerValue.IncreaseStat("LightingPlus", 0, BuffType.Session);
                // ����Ը�����Ҫ��Ӷ���/��ʾ��
            }
            if (LightingPlus < 3)
                LightingPlus++;
        }
        public static int WallDefense = 0;
        public void AddWallDefense() //ǽ1
        {
            if (WallDefense < 3)
            {
                WallDefense++;

                float percent = 0f;
                switch (WallDefense)
                {
                    case 1: percent = 0.1f; break;
                    case 2: percent = 0.2f; break;
                    case 3: percent = 0.3f; break;
                }

                playerValue.IncreaseStat("AddWallDefense", percent, BuffType.Session, 0, true);
            }
        }

        public static int ThornsLevel = 0; //ǽ2
        public void AddThornsDamage()
        {
            if (ThornsLevel == 0)
            {
                playerValue.IncreaseStat("ThornsDamage", 0, BuffType.Session);
            }
            if (ThornsLevel < 3)
                ThornsLevel++;
        }
        public static int ThornsShieldLevel = 0; //ǽ3
        public void ThornsShield()
        {
            if (ThornsShieldLevel == 0)
            {
                playerValue.IncreaseStat("ThornsShield", 0, BuffType.Session);
            }
            if (ThornsShieldLevel < 3)
                ThornsShieldLevel++;
        }
        public static int TriggerLowHpShield = 0; //ǽ3
        public void AddTriggerLowHpShield()
        {
            if (TriggerLowHpShield == 0)
            {
                playerValue.IncreaseStat("AddTriggerLowHpShield", 0, BuffType.Session);
            }
            if (TriggerLowHpShield < 3)
                TriggerLowHpShield++;
        }
        public static int TreeBlood1 = 0;
        public void AddTreeBlood1() //��1
        {
            if (TreeBlood1 < 3)
            {
                TreeBlood1++;
                playerValue.IncreaseStat("AddTreeBlood1", 0.3f, BuffType.Session, 0, true);
            }
        }
        public static int TreeBlood2 = 0;
        public void AddTreeBlood2() //��2
        {
            if (TreeBlood2 < 3)
            {
                TreeBlood2++;
                playerValue.IncreaseStat("AddTreeBlood2", 0.3f, BuffType.Session, 0, true);
            }
        }
        public static int LifeStealLevel = 0;
        public void AddStealLevel() //��3
        {
            if (LifeStealLevel < 3)
            {
                LifeStealLevel++;
                playerValue.IncreaseStat("AddStealLevel", 0.3f, BuffType.Session, 0, true);
            }
        }
        public static int DashTree = 0;
        public void AddDashTree() //��4
        {
            if (DashTree < 3)
            {
                DashTree++;
                playerValue.IncreaseStat("AddDashTree", 0.3f, BuffType.Session, 0, true);
            }
        }
    }
}
