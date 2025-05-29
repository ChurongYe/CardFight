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
        public void AddAttack()
        {
            playerValue.IncreaseStat("Attack", 5f, BuffType.Stage);
        }
    }
}
