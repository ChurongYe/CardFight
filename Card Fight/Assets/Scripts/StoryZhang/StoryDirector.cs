using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class StoryDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup blackFader;           // 黑幕（BlackImage）
    [SerializeField] private DialogueUI dialogue;              // 对话 UI（已有 FadeIn/Out、TypeLine、WaitForClick、SetPortrait）
    [SerializeField] private Button doorButton;                // 门按钮
    [SerializeField] private CanvasGroup doorCanvasGroup;      // 门所在 CanvasGroup
    [SerializeField] private Animator sequenceAnimator;        // 序列帧 Animator（controller 已挂好）
    [SerializeField] private CanvasGroup sequenceCanvas;       // 承载序列帧的 CanvasGroup
    [SerializeField] private string sequenceStateName = "IntroSeq";

    [Header("Portraits (Image，内部会取 .sprite)")]
    [SerializeField] private Image oldmanPortrait;             // 老者头像（Image）
    [SerializeField] private Image heroPortrait;               // 主角头像（Image）


    [Header("Texts")]
    [TextArea] public string[] preBlackLines;                  // 黑屏阶段对白（无头像）
    [TextArea] public string[] oldmanLines;                    // 老者第一段
    [TextArea] public string[] heroLines;                      // 主角对白
    [TextArea] public string[] oldmanFinalLines;               // 老者最后一段
    #region Scripted Sequence Dialogue (NEW)
    public enum Speaker { Hero, Oldman }

    [System.Serializable]
    public class DialogueEntry
    {
        public Speaker speaker;
        [TextArea] public string text;

        public bool waitForClick = true;   // 这句是否需要点击才能继续
        public float preDelay = 0f;        // 说这句前的停顿
        public float postDelay = 0f;       // 说完后的停顿

        // 勾上：当这句结束（若需要点击则等玩家点完）→ 恢复序列帧继续播放
        public bool resumeSequenceHere = false;
    }

    [Header("Sequence Dialogue (free scripted)")]
    public List<DialogueEntry> sequenceDialogue = new List<DialogueEntry>();
    #endregion

    [Header("Card Tutorial")]
    [SerializeField] private CanvasGroup cardCanvas;           // 卡牌教学面板（整张卡牌+说明）
    [SerializeField] private Button cardConfirmButton;         // “我知道了” 按钮
    [SerializeField] private float cardFadeIn = 0.30f;
    [SerializeField] private float cardFadeOut = 0.25f;

    [Header("Timings")]
    [SerializeField] private float delayBeforeFirst = 1f;      // 开始前微等待
    [SerializeField] private float dialogFade = 0.35f;         // 对话框淡入淡出
    [SerializeField] private float doorFade = 0.35f;           // 门出现/隐藏淡入淡出
    [SerializeField] private float fadeBlackAfterSecond = 0.6f;// 点击门后黑入时长
    [SerializeField] private bool useUnscaledTime = true;      // 是否用不随时间缩放的时间

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;          // 可用 AudioManager 上的 Source
    [SerializeField] private AudioClip doorClickSFX;           // 点击门的 SFX

    [Header("Sequence Control")]
    [SerializeField] private float sequenceSpeed = 1f;     // 序列帧正常播放速度
    [SerializeField] private int oldmanResumeAtLine = 9;   // 老者对白到第几句时触发序列帧继续（0基）
    [SerializeField] private float firstFrameLeadIn = 0.25f; // 首帧静止后，给对白一个小“起步”停顿

    [Header("Sequence Tail")]
    [SerializeField] private int tailFreezeFrames = 2;   // 末尾停留的帧数（默认2帧）
    [SerializeField] private float tailFadeOut = 0.35f;  // 末尾淡出到黑的时长
    [SerializeField] private float delayBeforeCard = 0.5f; // 淡出完成后，出卡牌教学前的等待秒数

    [SerializeField] private Transform heroRoot; // 主角的父物体（包含所有 SpriteRenderer）
    [SerializeField] private float heroRevealDelay = 0.2f;
    [SerializeField] private float heroRevealFade = 0.6f;

    // Enemy reveal（和主角同样逻辑）
    [SerializeField] private Transform enemyRoot;      // 敌人的父物体（包含所有 SpriteRenderer）
    [SerializeField] private float enemyRevealDelay = 0.2f;
    [SerializeField] private float enemyRevealFade = 0.6f;


    public UnityEvent OnStoryFinished;                         // 故事流程结束回调（在此回到正式游戏）

    private Coroutine _running;
    private bool isRunning => _running != null;

    #region Public API
    public void PlayOpeningSequence()
    {
        if (isRunning) return; // 已在播放，忽略
        _running = StartCoroutine(CoPlayOpeningSequence());
    }

    // 统一复用上面的入口，避免起两份协程
    public IEnumerator PlayOpeningSequenceAndWait()
    {
        PlayOpeningSequence();
        while (isRunning) yield return null;
    }


    #endregion

    private void SetHeroVisibleImmediate(bool visible)
    {
        if (!heroRoot) return;
        var renderers = heroRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            var c = r.color;
            c.a = visible ? 1f : 0f;
            r.color = c;
            r.enabled = visible; // 也可以只改 alpha 不关 renderer
        }
        // 如需禁用碰撞/交互：
        var colls = heroRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var c2d in colls) c2d.enabled = visible;
    }

    private IEnumerator FadeHeroIn(float dur)
    {
        if (!heroRoot) yield break;
        var rs = heroRoot.GetComponentsInChildren<SpriteRenderer>(true);
        // 先确保都可见（enabled=true），从 alpha 0 开始
        foreach (var r in rs)
        {
            r.enabled = true;
            var c = r.color; c.a = 0f; r.color = c;
        }
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // 跟你对话演出一致用 unscaled
            float a = Mathf.Clamp01(t / Mathf.Max(0.0001f, dur));
            foreach (var r in rs)
            {
                var c = r.color; c.a = a; r.color = c;
            }
            yield return null;
        }

        var colls = heroRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var c2d in colls) c2d.enabled = true;
    }

    private void SetEnemyVisibleImmediate(bool visible)
    {
        if (!enemyRoot) return;
        var renderers = enemyRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            var c = r.color; c.a = visible ? 1f : 0f; r.color = c;
            r.enabled = visible;
        }
        var colls = enemyRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var c2d in colls) c2d.enabled = visible;
    }

    private IEnumerator FadeEnemyIn(float dur)
    {
        if (!enemyRoot) yield break;
        var rs = enemyRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in rs)
        {
            r.enabled = true;
            var c = r.color; c.a = 0f; r.color = c;
        }
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // 和对话演出一致用 unscaled
            float a = Mathf.Clamp01(t / Mathf.Max(0.0001f, dur));
            foreach (var r in rs)
            {
                var c = r.color; c.a = a; r.color = c;
            }
            yield return null;
        }

        // 在 FadeEnemyIn 结尾加上
        var colls = enemyRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var c2d in colls) c2d.enabled = true;
    }


    #region Sequence

    private IEnumerator CoPlayOpeningSequence()
    {
        // —— 安全初始化：先把不该显示的面板关掉/设为透明 ——
        SafeSetActive(sequenceCanvas, false);
        SafeSetActive(cardCanvas, false);
        SetCanvasState(doorCanvasGroup, 0f, false, false);

        SetHeroVisibleImmediate(false);
        SetEnemyVisibleImmediate(false);




        if (delayBeforeFirst > 0f) yield return Wait(delayBeforeFirst);

        // 0) 黑屏对白（不显示头像）
        if (preBlackLines != null && preBlackLines.Length > 0)
        {
            dialogue.SetPortrait(null);
            yield return dialogue.FadeIn(dialogFade);
            yield return PlayLines(preBlackLines);
            yield return dialogue.FadeOut(dialogFade);
        }

        // 1) 老者第一段
        //if (oldmanLines != null && oldmanLines.Length > 0)
        //{
            //dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);
            //yield return dialogue.FadeIn(dialogFade);
            //yield return PlayLines(oldmanLines);
            //yield return dialogue.FadeOut(dialogFade);
        //}

        // 2) 显示门，等待玩家点击 → 播放 SFX → 门淡出 + 黑入
        if (doorCanvasGroup)
        {
            yield return FadeCanvasGroup(doorCanvasGroup, 1f, doorFade, true, true);
            yield return WaitForButton(doorButton);

            if (audioSource && doorClickSFX) audioSource.PlayOneShot(doorClickSFX);

            // 门先收拢消失
            yield return FadeCanvasGroup(doorCanvasGroup, 0f, 0.2f, false, false);
            // 黑幕渐入
            yield return FadeCanvasGroup(blackFader, 1f, fadeBlackAfterSecond, true, false);
        }

        // 3) 播放序列帧动画（在黑幕里头）
        //if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(true);
        //if (sequenceAnimator)
        //{
        //sequenceAnimator.Play(sequenceStateName, 0, 0f);



        //yield return new WaitUntil(() =>
        //{
        //var s = sequenceAnimator.GetCurrentAnimatorStateInfo(0);
        //return s.IsName(sequenceStateName) && s.normalizedTime >= 1f;
        //});
        //}

        // 2) 播放序列帧（先停在第一帧），并与“可编排对白队列”并行
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(true);
        if (sequenceAnimator)
        {
            // 播放到 0 并冻结在第一帧
            sequenceAnimator.speed = 0f;
            sequenceAnimator.Play(sequenceStateName, 0, 0f);

            // （可选）把黑幕拉开，露出画面；若希望对白在黑幕上方自行处理层级
            yield return FadeCanvasGroup(blackFader, 0f, 0.3f, false, false);

            if (firstFrameLeadIn > 0f) yield return Wait(firstFrameLeadIn);

            // —— 并行对白阶段 ——（优先使用“可编排对白队列”）
            bool usedScripted = sequenceDialogue != null && sequenceDialogue.Count > 0;
            if (usedScripted)
            {
                yield return dialogue.FadeIn(dialogFade);

                // 在对白条目中，遇到勾选 resumeSequenceHere 的那条 → 恢复序列帧速度
                yield return PlayScriptedSequence(sequenceDialogue, () =>
                {
                    if (sequenceAnimator)
                        sequenceAnimator.speed = Mathf.Max(0.01f, sequenceSpeed);
                });

                yield return dialogue.FadeOut(dialogFade);
            }
            else
            {
                // 兼容老逻辑（如果你没填 Scripted 队列，可以在这里播放“老者/主角”的旧段落）
                if (oldmanLines != null && oldmanLines.Length > 0)
                {
                    dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);
                    yield return dialogue.FadeIn(dialogFade);
                    yield return PlayLines(oldmanLines);
                    yield return dialogue.FadeOut(dialogFade);
                }

                // 若没有 Scripted 队列，直接恢复播放
                sequenceAnimator.speed = Mathf.Max(0.01f, sequenceSpeed);
            }

            
                float threshold = 0.98f;   // 兜底
                float holdSeconds = 0f;

                var clip = GetStateClip(sequenceAnimator, sequenceStateName);
                if (clip != null && clip.frameRate > 0f && clip.length > 0f)
                {
                    holdSeconds = Mathf.Max(0f, tailFreezeFrames / clip.frameRate);
                    float frac = holdSeconds / clip.length;
                    threshold = Mathf.Clamp01(1f - frac);
                }

                // 到达“倒数 N 帧”的时刻
                yield return WaitForNormalizedTime(sequenceAnimator, sequenceStateName, threshold);

                // 暂停在末尾
                sequenceAnimator.speed = 0f;
                if (holdSeconds > 0f) yield return Wait(holdSeconds);

                // 淡出到黑
                yield return FadeCanvasGroup(blackFader, 1f, tailFadeOut, true, false);

                // 关掉序列帧画布
                if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(false);

                // 黑屏下稍等，再出卡牌教学
                if (delayBeforeCard > 0f) yield return Wait(delayBeforeCard);
                 yield return FadeCanvasGroup(blackFader, 0f, 0.25f, false, false);


        }


        // 动画结束 → 黑幕淡出 → 关闭序列帧画布
        //yield return FadeCanvasGroup(blackFader, 0f, 0.35f, false, false);
        //if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(false);

        // 4) 主角对白
        //if (heroLines != null && heroLines.Length > 0)
        //{
        //dialogue.SetPortrait(heroPortrait ? heroPortrait.sprite : null);
        //yield return dialogue.FadeIn(dialogFade);
        //yield return PlayLines(heroLines);
        //yield return dialogue.FadeOut(dialogFade);
        //}

        // 5) 老者最后一段
        //if (oldmanFinalLines != null && oldmanFinalLines.Length > 0)
        //{
        //dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);
        //yield return dialogue.FadeIn(dialogFade);
        //yield return PlayLines(oldmanFinalLines);
        //yield return dialogue.FadeOut(dialogFade);
        //}

        // 6) 卡牌教学：显示 → 等确认 → 隐藏
        if (cardCanvas)
        {
            yield return FadeCanvasGroup(cardCanvas, 1f, cardFadeIn, true, true);
            yield return WaitForButton(cardConfirmButton);
            yield return FadeCanvasGroup(cardCanvas, 0f, cardFadeOut, false, false);
            cardCanvas.gameObject.SetActive(false);

            if (heroRevealDelay > 0f || enemyRevealDelay > 0f)
                yield return Wait(Mathf.Max(heroRevealDelay, enemyRevealDelay));

            // 并行淡入
            var heroCo = StartCoroutine(FadeHeroIn(heroRevealFade));
            var enemyCo = StartCoroutine(FadeEnemyIn(enemyRevealFade));

            // 等待二者中较长的那一个
            yield return Wait(Mathf.Max(heroRevealFade, enemyRevealFade));
        }

        OnStoryFinished?.Invoke();   // 通知开始正式游戏（在 Inspector 挂你的回调）
        _running = null;
    }
    #endregion

    #region Helpers
    private IEnumerator PlayLines(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            yield return dialogue.TypeLine(lines[i]);  // 逐字出现
            yield return dialogue.WaitForClick();      // 等点继续
        }
    }

    // 找到某个状态对应的 AnimationClip（用于拿到 frameRate 和 length）
    private AnimationClip GetStateClip(Animator animator, string stateName)
    {
        if (!animator || animator.runtimeAnimatorController == null) return null;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == stateName)
                return clip;
        }
        return null;
    }

    // 等到动画播放到某个 normalizedTime（0-1）
    private IEnumerator WaitForNormalizedTime(Animator animator, string stateName, float target)
    {
        if (!animator) yield break;
        target = Mathf.Clamp01(target);
        while (true)
        {
            var s = animator.GetCurrentAnimatorStateInfo(0);
            if (s.IsName(stateName) && s.normalizedTime >= target) break;
            yield return null;
        }
    }

    private IEnumerator PlayScriptedSequence(List<DialogueEntry> entries, System.Action onResume)
    {
        if (entries == null || entries.Count == 0) yield break;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];

            // 切头像
            if (e.speaker == Speaker.Hero)
                dialogue.SetPortrait(heroPortrait ? heroPortrait.sprite : null);
            else
                dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);

            if (e.preDelay > 0f) yield return Wait(e.preDelay);

            // 打字
            yield return dialogue.TypeLine(e.text);

            // 是否要点击
            if (e.waitForClick)
                yield return dialogue.WaitForClick();

            if (e.postDelay > 0f) yield return Wait(e.postDelay);

            // “这句结束后”触发（若需要点击，则在点击完成后再触发，更自然）
            if (e.resumeSequenceHere)
                onResume?.Invoke();
        }
    }


    private void SafeSetActive(CanvasGroup g, bool on)
    {
        if (g) g.gameObject.SetActive(on);
    }

    private void SetCanvasState(CanvasGroup g, float alpha, bool interactable, bool blocksRaycasts)
    {
        if (!g) return;
        g.alpha = alpha;
        g.interactable = interactable;
        g.blocksRaycasts = blocksRaycasts;
        g.gameObject.SetActive(interactable || blocksRaycasts || alpha > 0f);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup g, float target, float duration, bool interactable, bool blocksRaycasts)
    {
        if (!g) yield break;

        g.gameObject.SetActive(true);
        float start = g.alpha;
        float t = 0f;

        if (duration <= 0f)
        {
            g.alpha = target;
        }
        else
        {
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                g.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            g.alpha = target;
        }

        g.interactable = interactable;
        g.blocksRaycasts = blocksRaycasts;

        if (Mathf.Approximately(target, 0f) && !interactable && !blocksRaycasts)
            g.gameObject.SetActive(false);
    }

    private IEnumerator WaitForButton(Button btn)
    {
        if (!btn) yield break;

        bool clicked = false;
        void OnClick() { clicked = true; }

        btn.onClick.AddListener(OnClick);
        while (!clicked) yield return null;
        btn.onClick.RemoveListener(OnClick);
    }

    private WaitForSecondsRealtime Wait(float seconds) =>
        new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
    // StoryDirector.cs 里新增 ↓↓↓
    private IEnumerator PlayOpeningSequenceAndWait_Legacy()
    {
        yield return CoPlayOpeningSequence();
    }
    #endregion



}
