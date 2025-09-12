using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    [Header("出牌特效 (Prefab)")]
    public GameObject playEffectPrefab; // 在 Inspector 里拖一个粒子/特效 Prefab
    public Transform effectSpawnPoint;  // 出牌位置 (可以是 DropZone 自己)
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;

        if (droppedObj != null && droppedObj.TryGetComponent<Card>(out var droppedCard))
        {
            var holder = FindObjectOfType<HorizontalCardHolder>();
            var selectedCards = holder.GetSelectedCards();

            // 组合是否合法
            if (holder.ValidateCombination(selectedCards) > 0)
            {
                // 合法 → 直接调用 TryPlaySelectedCards（包含：加成、出牌、清空、补位）
                holder.TryPlaySelectedCards();
                // 播放出牌特效
                if (playEffectPrefab != null)
                {
                    var spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
                    var fx = Instantiate(playEffectPrefab, spawnPos, Quaternion.identity);
                    Destroy(fx, 2f); // 2 秒后销毁（可根据特效时长调整）
                }
            }
            else
            {
                // 不合法 → 取消选中 + 回到原位
                foreach (var card in selectedCards)
                {
                    card.Deselect();
                    card.ReturnToOriginalPosition();
                }
            }
        }
    }
}
