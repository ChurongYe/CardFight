using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI; // 若未使用NavMeshAgent，请保留，也不会报错
using System.Collections;
using UnityEngine.UI;

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

    // ===== Mid Cutscene 2→3 =====
    [Header("Mid Cutscene 2→3")]
    public int cutsceneMapIndex = 1;                 // Map2（0-based）
    public DialogueUI dialogueUI;                    // 对话UI
    public DialoguePickup.Entry[] cutscene;          // 文本（沿用 DialoguePickup 的 Entry）

    [Header("Portraits")]
    public UnityEngine.UI.Image oldmanPortrait;
    public UnityEngine.UI.Image heroPortrait;
    public UnityEngine.UI.Image generalPortrait;   // 新增：将军头像


    [Header("Cutscene Visual")]
    public CanvasGroup cutsceneGroup;                // 若过场是 UI 画面（CanvasGroup 淡入淡出）
    public SpriteRenderer[] cutsceneSprites;         // 若过场是 2D 序列帧（所有参与淡入的 SR）
    public Animator cutsceneAnimator;                // 可选：控制循环
    public string cutsceneTrigger = "Play";          // 可选触发参数

    [Header("Cutscene Timing")]
    public float animFadeIn = 0.4f;                  // 动画淡入
    public float preDialogueDelay = 0.6f;            // 动画出现→稍等→出对话
    public float postDialogueDelay = 0.4f;           // 对话结束→稍等→淡出
    public float animFadeOut = 0.4f;                 // 动画淡出

    [Header("Flow After Cutscene")]
    public bool skipRewardAndGoNext = true;          // 播完直接进 Map3
    bool midCutscenePlayed = false;                  // 防重复


    // ===== After Cutscene: Black + Oldman Dialogue =====
    [Header("After Cutscene (Black + Oldman Dialogue)")]
    public CanvasGroup screenFade;                    // 全屏黑幕（CanvasGroup，Image=黑色，alpha初始0）
    public DialoguePickup.Entry[] oldmanLines;        // 动画淡出后的“老人对白”
    public float toBlackDur = 0.25f;                  // 淡到全黑
    public float gapBeforeOldman = 0.4f;              // 黑屏后稍等再出对话
    public float fromBlackDur = 0.5f;                 // 对话结束后从黑淡入




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
            // Map2：先播中插剧情
            if (!midCutscenePlayed && currentIndex == cutsceneMapIndex && HasCutscene())
            {
                PausePlayer(true);
                StartCoroutine(PlayMidCutsceneThenNextStep(map));
                return; // 先不弹奖励
            }

            // 其它情况：直接弹奖励
            PausePlayer(true);
            map.ShowRewardUI(uiAudioSource);
        }
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


    // ===== 协程：动画淡入→稍等→对白→稍等→动画淡出→进 Map3 =====
    IEnumerator PlayMidCutsceneThenNextStep(MapZone map)
    {
        midCutscenePlayed = true;

        // (A) 过场画面淡入 + 循环播放
        if (cutsceneGroup)
        {
            cutsceneGroup.gameObject.SetActive(true);
            cutsceneGroup.alpha = 0f;
            yield return DialogueUI.FadeCanvas(cutsceneGroup, 1f, animFadeIn, false, false, true);
        }
        else if (cutsceneSprites != null && cutsceneSprites.Length > 0)
        {
            SetSpritesAlpha(cutsceneSprites, 0f);
            SetSpritesActive(cutsceneSprites, true);
            yield return FadeSprites(cutsceneSprites, 1f, animFadeIn);
        }
        if (cutsceneAnimator)
        {
            cutsceneAnimator.ResetTrigger(cutsceneTrigger);
            if (!string.IsNullOrEmpty(cutsceneTrigger)) cutsceneAnimator.SetTrigger(cutsceneTrigger);
            // 动画片段请在 Animator/Clip 勾选 Loop
        }

        // (B) 稍等一下再出对话（不受 Time.timeScale 影响）
        if (preDialogueDelay > 0f) yield return new WaitForSecondsRealtime(preDialogueDelay);

        // (C) 对话：将军头像（按 Entry 的 overridePortrait 优先）
        if (dialogueUI && cutscene != null && cutscene.Length > 0)
        {
            yield return dialogueUI.FadeIn(0.2f);

            foreach (var e in cutscene)
            {
                // 头像选择（参考 Pickup 的优先级：override > 预设）
                Sprite toUse = e.overridePortrait != null
                    ? e.overridePortrait
                    : (generalPortrait ? generalPortrait.sprite : null);
                dialogueUI.SetPortrait(toUse);                        // 切头像
                if (e.preDelay > 0f) yield return new WaitForSecondsRealtime(e.preDelay);

                yield return dialogueUI.TypeLine(e.text);             // 打字
                if (e.waitForClick) yield return dialogueUI.WaitForClick(); // 等点击（全屏 Button）
                if (e.postDelay > 0f) yield return new WaitForSecondsRealtime(e.postDelay);
            }

            yield return dialogueUI.FadeOut(0.2f);
        }

        // (D) 对话后稍等
        if (postDialogueDelay > 0f) yield return new WaitForSecondsRealtime(postDialogueDelay);

        /* —— 先把“黑幕”作为底层铺上来（稍晚于动画出现，但早于动画淡出） —— */
        if (screenFade)
        {
            screenFade.gameObject.SetActive(true);
            screenFade.alpha = 0f;

            // 可微调这个“底层黑幕出现”延迟，让它比动画出现稍晚一点
            // 例如 0.10f；如不需要额外延时设为 0 即可
            float blackUnderlayDelay = 0.10f;
            if (blackUnderlayDelay > 0f) yield return new WaitForSecondsRealtime(blackUnderlayDelay);

            // 让黑幕先淡到 1（但因为在下层，所以此时还被动画盖住）
            yield return DialogueUI.FadeCanvas(screenFade, 1f, toBlackDur, false, false, true);
        }

        /* —— 再把 2-3 动画本体淡出，此时“露出来”的就是黑幕 —— */
        if (cutsceneGroup)
        {
            yield return DialogueUI.FadeCanvas(cutsceneGroup, 0f, animFadeOut, false, false, true);
            cutsceneGroup.gameObject.SetActive(false);
        }
        else if (cutsceneSprites != null && cutsceneSprites.Length > 0)
        {
            yield return FadeSprites(cutsceneSprites, 0f, animFadeOut);
            SetSpritesActive(cutsceneSprites, false);
        }

        

        // 黑屏后稍等一下再出“老人对白”
        if (gapBeforeOldman > 0f) yield return new WaitForSecondsRealtime(gapBeforeOldman);


        // (E) 老人对白（在黑幕之上）
        if (dialogueUI && oldmanLines != null && oldmanLines.Length > 0)
        {
            yield return dialogueUI.FadeIn(0.2f);

            foreach (var e in oldmanLines)
            {
                // 头像优先级：单句 override > 老人头像
                Sprite toUse = e.overridePortrait != null
                    ? e.overridePortrait
                    : (oldmanPortrait ? oldmanPortrait.sprite : null);
                dialogueUI.SetPortrait(toUse);

                if (e.preDelay > 0f) yield return new WaitForSecondsRealtime(e.preDelay);
                yield return dialogueUI.TypeLine(e.text);
                if (e.waitForClick) yield return dialogueUI.WaitForClick();
                if (e.postDelay > 0f) yield return new WaitForSecondsRealtime(e.postDelay);
            }
            yield return dialogueUI.FadeOut(0.2f);
        }

        // (F) 仍在黑屏时切到下一关 → 再从黑淡入显示 Map3
        if (skipRewardAndGoNext)
        {
            // 此时屏幕是全黑，看不到传送与刷新
            ConfirmAndGoNext();                // 这里会 TeleportToEntry(map3) 并取消玩家暂停
            if (screenFade)
                yield return DialogueUI.FadeCanvas(screenFade, 0f, fromBlackDur, false, false, true);
            else
                PausePlayer(false);            // 没黑幕时至少恢复玩家
        }
        else
        {
            // 若仍然要弹奖励UI，可在黑屏后改成：先淡回可视，再 map.ShowRewardUI(...)
            if (screenFade)
                yield return DialogueUI.FadeCanvas(screenFade, 0f, fromBlackDur, false, false, true);
            map.ShowRewardUI(uiAudioSource);
        }
    }

    // ===== 工具函数 =====
    bool HasCutscene()
    {
        return (cutsceneAnimator != null)
            || (cutsceneGroup != null)
            || (cutsceneSprites != null && cutsceneSprites.Length > 0)
            || (dialogueUI != null && cutscene != null && cutscene.Length > 0);
    }

    void SetSpritesActive(SpriteRenderer[] rs, bool active)
    {
        foreach (var r in rs) if (r) r.gameObject.SetActive(active);
    }
    void SetSpritesAlpha(SpriteRenderer[] rs, float a)
    {
        foreach (var r in rs) if (r) { var c = r.color; c.a = a; r.color = c; }
    }
    IEnumerator FadeSprites(SpriteRenderer[] rs, float to, float dur)
    {
        float from = (rs != null && rs.Length > 0) ? rs[0].color.a : 0f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = (dur <= 0f) ? to : Mathf.Lerp(from, to, t / dur);
            SetSpritesAlpha(rs, a);
            yield return null;
        }
        SetSpritesAlpha(rs, to);
    }

}
