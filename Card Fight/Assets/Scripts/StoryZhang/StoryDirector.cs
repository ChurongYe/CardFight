using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StoryDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup blackFader;          // 黑幕（BlackImage）
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

    public UnityEvent OnStoryFinished;                         // 故事流程结束回调（在此回到正式游戏）

    private Coroutine _running;

    #region Public API
    public void PlayOpeningSequence()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoPlayOpeningSequence());
    }
    #endregion

    #region Sequence
    private IEnumerator CoPlayOpeningSequence()
    {
        // —— 安全初始化：先把不该显示的面板关掉/设为透明 ——
        SafeSetActive(sequenceCanvas, false);
        SafeSetActive(cardCanvas, false);
        SetCanvasState(doorCanvasGroup, 0f, false, false);

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
        if (oldmanLines != null && oldmanLines.Length > 0)
        {
            dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);
            yield return dialogue.FadeIn(dialogFade);
            yield return PlayLines(oldmanLines);
            yield return dialogue.FadeOut(dialogFade);
        }

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
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(true);
        if (sequenceAnimator)
        {
            sequenceAnimator.Play(sequenceStateName, 0, 0f);
            yield return new WaitUntil(() =>
            {
                var s = sequenceAnimator.GetCurrentAnimatorStateInfo(0);
                return s.IsName(sequenceStateName) && s.normalizedTime >= 1f;
            });
        }

        // 动画结束 → 黑幕淡出 → 关闭序列帧画布
        yield return FadeCanvasGroup(blackFader, 0f, 0.35f, false, false);
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(false);

        // 4) 主角对白
        if (heroLines != null && heroLines.Length > 0)
        {
            dialogue.SetPortrait(heroPortrait ? heroPortrait.sprite : null);
            yield return dialogue.FadeIn(dialogFade);
            yield return PlayLines(heroLines);
            yield return dialogue.FadeOut(dialogFade);
        }

        // 5) 老者最后一段
        if (oldmanFinalLines != null && oldmanFinalLines.Length > 0)
        {
            dialogue.SetPortrait(oldmanPortrait ? oldmanPortrait.sprite : null);
            yield return dialogue.FadeIn(dialogFade);
            yield return PlayLines(oldmanFinalLines);
            yield return dialogue.FadeOut(dialogFade);
        }

        // 6) 卡牌教学：显示 → 等确认 → 隐藏
        if (cardCanvas)
        {
            yield return FadeCanvasGroup(cardCanvas, 1f, cardFadeIn, true, true);
            yield return WaitForButton(cardConfirmButton);
            yield return FadeCanvasGroup(cardCanvas, 0f, cardFadeOut, false, false);
            cardCanvas.gameObject.SetActive(false);
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
    public IEnumerator PlayOpeningSequenceAndWait()
    {
        // 直接把核心协程跑一遍并把控制权交还给调用方
        yield return CoPlayOpeningSequence();
    }
    #endregion
}
