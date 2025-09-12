using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI; // 若未使用NavMeshAgent，请保留，也不会报错

public class LevelFlowController : MonoBehaviour
{
    [Header("玩家/控制")]
    public Transform player;                 // 玩家Transform（或带 Player 标签自动找）
    [Tooltip("可选：禁用该脚本来冻结玩家操作（UI弹出时）")]
    public MonoBehaviour playerController;   // 你的移动/操作脚本（可留空）

    [Header("NavMesh(可选)")]
    [Tooltip("如玩家带有 NavMeshAgent，则用 Warp 传送更稳")]
    public NavMeshAgent navAgent;            // 可留空

    [Header("关卡顺序")]
    public MapZone[] maps;
    public int startIndex = 0;

    [Header("音频")]
    public AudioSource uiAudioSource;        // 用于播放结算音效的AudioSource

    [Header("完成时事件(可选)")]
    public UnityEvent OnAllFinished;         // 全部关卡结束后的事件

    int currentIndex = -1;
    MapZone Current => (currentIndex >= 0 && currentIndex < maps.Length) ? maps[currentIndex] : null;

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!navAgent && player) navAgent = player.GetComponent<NavMeshAgent>();

        // 初始化每个Map
        foreach (var m in maps)
            if (m) m.Setup(this);
    }

    void Start()
    {
        currentIndex = Mathf.Clamp(startIndex, 0, maps.Length - 1);
        // 传送到当前关入口（Map1可不设）
        TeleportToEntry(Current);
    }

    void TeleportToEntry(MapZone map)
    {
        if (!map || !player || !map.entryPoint) return;

        Vector3 pos = map.entryPoint.position;
        if (navAgent) navAgent.Warp(pos);
        else player.position = pos;
    }

    public void TryOpenClearUI(MapZone map)
    {
        if (map != Current) return;

        if (map.IsCleared())
        {
            PausePlayer(true);
            map.ShowRewardUI(uiAudioSource);
        }
        // 未清场则什么都不做（你也可以在这里提示“先清理敌人！”）
    }

    public void ConfirmAndGoNext()
    {
        if (Current) Current.HideRewardUI();

        // 最后一关就走收尾
        if (currentIndex >= maps.Length - 1)
        {
            PausePlayer(false);
            OnAllFinished?.Invoke();
            return;
        }

        // 进入下一关
        currentIndex++;
        TeleportToEntry(Current);
        PausePlayer(false);
    }

    void PausePlayer(bool pause)
    {
        if (playerController)
            playerController.enabled = !pause;
        // 如需完全暂停时间线，改用：Time.timeScale = pause ? 0f : 1f;
    }
}
