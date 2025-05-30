using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Core
{
    public class CardValue : MonoBehaviour
    {
        private PlayerValue playerValue;
        void Start()
        {
            playerValue = FindObjectOfType<Core.PlayerValue>();
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

        //加点牌
        public void AttackFire()
        {
            //playerValue.IncreaseStat("None", 0, BuffType.Once, 2f, false, new BurnEffect());
        }
      
    }
}
