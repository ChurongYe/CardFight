using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitPortal : MonoBehaviour
{
    [Tooltip("进入这个触发器的对象需要带 Player 标签")]
    public string playerTag = "Player";

    private MapZone owner;
    private LevelFlowController flow;

    /// <summary>由 MapZone/LevelFlowController 在运行时注入</summary>
    public void Init(MapZone map, LevelFlowController controller)
    {
        owner = map;
        flow = controller;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // 作为传送 / 出口触发器
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!owner || !flow) return;
        if (!other.CompareTag(playerTag)) return;

        // 只有清场后才会弹出结算 UI
        flow.TryOpenClearUI(owner);
    }
}
