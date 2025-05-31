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
            playerValue.IncreaseStat("currentHP", 1f, BuffType.Stage, 0, false);
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
            playerValue.IncreaseStat("currentHP", critValue, BuffType.Once, 5f);
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
    }
}
