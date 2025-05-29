using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        private PlayerValue playerValue;
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            StartCoroutine(WaitForPlayerAndApplyUpgrades());
        }
        public void OnPlayerDeath()
        {
            Debug.Log("玩家死亡，游戏失败");
            // UI、重开等逻辑
        }
        private IEnumerator WaitForPlayerAndApplyUpgrades()
        {
            // 等待直到场景里有玩家的 PlayerValue
            while (playerValue == null)
            {
                playerValue = FindObjectOfType<PlayerValue>();
                yield return null;
            }

            // 找到玩家后调用，给玩家应用永久升级
            OnPlayerSpawned(playerValue);
        }

        private void OnPlayerSpawned(PlayerValue playerValue)
        {
            UpgradeManager.Instance.ApplyAllUpgradesToPlayer(playerValue);
        }
    }
}
