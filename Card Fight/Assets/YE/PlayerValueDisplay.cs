using TMPro;
using UnityEngine;

namespace Core
{
    public class PlayerValueDisplay : MonoBehaviour
    {
        public PlayerValue player; // 拖入场景中的PlayerValue
        public TMP_Text statsText; // 拖入 Text 组件

        private void Update()
        {
            if (player == null || statsText == null) return;

            statsText.text =
                $"<b>BASE:</b>\n" +
                $"Move Speed: {player.baseMoveSpeed}\n" +
                $"Dash CD: {player.baseDashCooldown:F2}s\n" +
                $"Max HP: {player.baseMaxHP}\n" +
                $"Defense: {player.baseDefense}\n" +
                $"Attack: {player.baseAttack}\n" +
                $"Crit Rate: {player.baseCritRate:P1}\n" +
                $"Attack Speed: {player.baseAttackSpeed:F2}s\n\n" +
                $"<b>NOW:</b>\n" +
                $"Move Speed: {player.currentMoveSpeed}\n" +
                $"Dash CD: {player.currentDashCooldown:F2}s\n" +
                $"Max HP: {PlayerValue.currentMaxHP}\n" +
                $"HP: {PlayerValue.currentHP}/{PlayerValue.currentMaxHP}\n" +
                $"Shield: {player.currentShield}\n" +
                $"Defense: {player.currentDefense}\n" +
                $"Attack: {player.currentAttack}\n" +
                $"Crit Rate: {player.currentCritRate:P1}\n" +
                $"Attack Speed: {player.currentAttackSpeed:F2}s";
        }
    }
}
