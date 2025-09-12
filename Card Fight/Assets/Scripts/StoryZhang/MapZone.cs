using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MapZone : MonoBehaviour
{
    [Header("标识(可选)")]
    public string mapName = "Map";

    [Header("位点")]
    public Transform entryPoint;         // 下一关传入的位置（Map1 可留空）
    public ExitPortal exitPortal;        // 传出触发器（最后一关留空）

    [Header("敌人（清场判定）")]
    [Tooltip("把本关所有敌人的GameObject拖进来即可；死亡被销毁或SetActive(false)都可以")]
    public GameObject[] enemies;

    [Header("结算UI")]
    public GameObject rewardCanvas;      // 弹出的Canvas（含“新卡牌展示”等）
    public Button confirmButton;         // Canvas里的确认按钮（例如“UNDERSTAND”）
    public AudioClip rewardSfx;          // 结算音效（可选）

    [Header("事件(可选)")]
    public UnityEvent OnCleared;         // 第一次清场时触发
    public UnityEvent OnShown;           // 结算UI显示时
    public UnityEvent OnHidden;          // 结算UI隐藏时

    bool clearedOnce = false;

    private LevelFlowController controllerRef;   // 记录关卡总控

    void NotifyControllerCleared()
    {
        if (controllerRef) controllerRef.TryOpenClearUI(this); // 通知总控“我清场了”
    }

    public void Setup(LevelFlowController controller)
    {
        controllerRef = controller;

        if (exitPortal) exitPortal.Init(this, controller);
        if (rewardCanvas) rewardCanvas.SetActive(false);

        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(controller.ConfirmAndGoNext);
        }

        // 第一次清场时，自动回调总控
        OnCleared.RemoveListener(NotifyControllerCleared);
        OnCleared.AddListener(NotifyControllerCleared);
    }

    void Update()
    {
        // 只在还没清场过的阶段做检查；第一次全空时会触发 OnCleared
        if (!clearedOnce) IsCleared();
    }


    public bool IsCleared()
    {
        // 只要数组里还有“活着”的对象(不为null并且activeInHierarchy)就未清场
        for (int i = 0; i < enemies.Length; i++)
        {
            var go = enemies[i];
            if (go && go.activeInHierarchy) return false;
        }
        if (!clearedOnce)
        {
            clearedOnce = true;
            OnCleared?.Invoke();
        }
        return true;
    }

    public void ShowRewardUI(AudioSource uiAudio)
    {
        if (rewardCanvas) rewardCanvas.SetActive(true);
        if (rewardSfx && uiAudio) uiAudio.PlayOneShot(rewardSfx);
        OnShown?.Invoke();
    }

    public void HideRewardUI()
    {
        if (rewardCanvas) rewardCanvas.SetActive(false);
        OnHidden?.Invoke();
    }
}
