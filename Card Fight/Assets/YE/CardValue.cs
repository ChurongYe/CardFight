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
            FireLevel = 0; // 默认为0级，最大3级
            PlayerFireLevel = 0;
            fireballLevel = 0;
            bloodCritLevel = 0; // 血量越低暴击越高技能等级，最大 3 级
            bloodCritEnabled = false;
            PlayerFire = false;
            fireball = false;
        }
        //基础牌
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
        //技能牌
        public void Summon()
        {
            //具体逻辑
        }
        //加点牌
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
        public static int FireLevel = 0; // 默认为0级，最大3级
        public static int PlayerFireLevel = 0; // 默认为0级，最大3级
        public void AttackFire()
        {
            if (FireLevel == 0)
            {
                attackType |= AttackType.Fire; // 添加 Fire 类型
                attackType &= ~AttackType.NormalMelee;
                playerValue.IncreaseStat("AttackFire", 0, BuffType.Session);
                //具体逻辑
            }
            if (FireLevel < 3)
                FireLevel++;
        }//火1
        public static bool PlayerFire = false ;
        public void AttackPlayerFire()
        {
            if (PlayerFireLevel == 0)
            {
                PlayerFire = true;
                attackType |= AttackType.PlayerFire; // 添加 PlayerFire 类型
                playerValue.IncreaseStat("AttackPlayerFire", 0, BuffType.Session);
                //具体逻辑
            }
            if (PlayerFireLevel < 3)
                PlayerFireLevel++;
        }//火2
        public static int fireballLevel = 0; // 默认为0，最大3级
        public static bool fireball = false ;
        public void Fireballs()//火3
        {
            if (fireballLevel == 0)
            {
                fireball = true;
                playerValue.IncreaseStat("Fireballs", 0, BuffType.Session);
                //具体逻辑
            }

            if (fireballLevel < 3)
                fireballLevel++;
        }
        public static int bloodCritLevel = 0; // 血量越低暴击越高技能等级，最大 3 级
        public static bool bloodCritEnabled = false;

        public void ActivateBloodCrit()//火4
        {
            if (bloodCritLevel == 0)
            {
                bloodCritEnabled = true;
                playerValue.IncreaseStat("BloodCrit", 0, BuffType.Session);
                // 你可以根据需要添加动画/提示等
            }

            if (bloodCritLevel < 3)
                bloodCritLevel++;
        }
    }
}
