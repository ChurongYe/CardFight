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
            Debug.Log("�����������Ϸʧ��");
            // UI���ؿ����߼�
        }
        private IEnumerator WaitForPlayerAndApplyUpgrades()
        {
            // �ȴ�ֱ������������ҵ� PlayerValue
            while (playerValue == null)
            {
                playerValue = FindObjectOfType<PlayerValue>();
                yield return null;
            }

            // �ҵ���Һ���ã������Ӧ����������
            OnPlayerSpawned(playerValue);
        }

        private void OnPlayerSpawned(PlayerValue playerValue)
        {
            UpgradeManager.Instance.ApplyAllUpgradesToPlayer(playerValue);
        }
    }
}
