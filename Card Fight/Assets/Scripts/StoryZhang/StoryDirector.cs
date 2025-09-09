using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoryDirector : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup blackFader;         // BlackImage 的 CanvasGroup（黑幕）
    public DialogueUI dialogue;            // 上面的 DialogueUI
    public Button doorButton;              // 门把手按钮
    public CanvasGroup doorCanvasGroup;    // 门的 CanvasGroup（可不填，脚本会自动添加）
    public Animator sequenceAnimator;      // 序列帧 Animator
    public Canvas sequenceCanvas;          // 序列帧所在 Canvas（建议起初 Inactive）
    public string sequenceStateName = "IntroSeq";

    [Header("Portraits")]
    public Sprite oldmanPortrait;          // 第三段：老人头像

    [Header("Texts")]
    [TextArea(2, 4)] public string firstLine = "Son... you must come back safely.\nWhy... why must you do this to us...?";
    [TextArea(2, 4)] public string secondLine = "Let me go— let me go——";
    [TextArea(2, 4)] public string thirdLine = "......Am I... alive?";

    [Header("Timings")]
    public float delayBeforeFirst = 5f;         // 黑屏后等 5 秒出现第一段
    public float dialogFade = 0.35f;            // 对话面板淡出时间
    public float doorFade = 0.35f;            // 门淡出时间
    public float fadeBlackAfterSecond = 0.6f;   // 第二段对白关闭时黑幕淡出时间
    public bool useUnscaledTime = true;

    [Header("Audio (optional)")]
    public AudioClip doorClickSFX;              // 点门时播放
    public float sfxVolume = 1f;

    float DT => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    public IEnumerator PlayOpeningSequence()
    {
        // 初始化：黑着，黑幕不拦截点击；门与序列帧关闭
        if (blackFader) { blackFader.alpha = 1f; blackFader.blocksRaycasts = false; }
        if (doorButton) doorButton.gameObject.SetActive(false);
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(false);

        if (!dialogue.gameObject.activeSelf) dialogue.gameObject.SetActive(true);

        // ========= 第一段对白（无头像） =========
        yield return WaitX(delayBeforeFirst);
        yield return dialogue.ShowAndWaitForClick(firstLine, null);
        // 第一段点击后，对话淡出
        yield return dialogue.HideWithFade(dialogFade);

        // ========= 门出现，等待点击 =========
        doorButton.gameObject.SetActive(true);
        if (!doorCanvasGroup && doorButton) doorCanvasGroup = doorButton.GetComponent<CanvasGroup>();
        if (!doorCanvasGroup && doorButton) doorCanvasGroup = doorButton.gameObject.AddComponent<CanvasGroup>();
        if (doorCanvasGroup) doorCanvasGroup.alpha = 1f;

        bool doorClicked = false;
        doorButton.onClick.RemoveAllListeners();
        doorButton.onClick.AddListener(() => doorClicked = true);

        yield return new WaitUntil(() => doorClicked);

        // 播放门 SFX
        if (doorClickSFX)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySFX(doorClickSFX, sfxVolume);
            else AudioSource.PlayClipAtPoint(doorClickSFX, Vector3.zero, sfxVolume);
        }

        // 门淡出后隐藏
        if (doorCanvasGroup) yield return FadeCanvasGroup(doorCanvasGroup, 0f, doorFade);
        doorButton.gameObject.SetActive(false);

        // ========= 第二段对白（无头像） =========
        yield return dialogue.ShowAndWaitForClick(secondLine, null);

        // 玩家点击关闭时，让“对白面板”和“黑幕”**同时淡出**
        IEnumerator d1 = dialogue.HideWithFade(dialogFade);
        IEnumerator d2 = FadeBlack(0f, fadeBlackAfterSecond);
        yield return StartTwo(d1, d2); // 同时等待两者完成

        // ========= 播放序列帧 =========
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(true);
        if (sequenceAnimator)
        {
            sequenceAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            sequenceAnimator.Play(sequenceStateName, 0, 0f);
            yield return new WaitUntil(() =>
            {
                var st = sequenceAnimator.GetCurrentAnimatorStateInfo(0);
                return st.normalizedTime >= 1f && !sequenceAnimator.IsInTransition(0);
            });
        }
        if (sequenceCanvas) sequenceCanvas.gameObject.SetActive(false);

        // ========= 第三段对白（带 Oldman 头像） =========
        yield return dialogue.Show(thirdLine, oldmanPortrait); // 这次可以直接用 Show：点击后立即隐藏

        // 演出结束，StartMenuController 会恢复 Time.timeScale、切正式 BGM、开局
    }

    // —— helpers ——
    private IEnumerator FadeBlack(float target, float duration)
    {
        if (!blackFader) yield break;
        float start = blackFader.alpha, t = 0f;
        while (t < duration)
        {
            t += DT;
            blackFader.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        blackFader.alpha = target;
    }
    private IEnumerator FadeCanvasGroup(CanvasGroup g, float target, float duration)
    {
        float start = g.alpha, t = 0f;
        while (t < duration)
        {
            t += DT;
            g.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        g.alpha = target;
    }
    private IEnumerator StartTwo(IEnumerator a, IEnumerator b)
    {
        bool A = false, B = false;
        Coroutine ca = StartCoroutine(Wrap(a, () => A = true));
        Coroutine cb = StartCoroutine(Wrap(b, () => B = true));
        while (!A || !B) yield return null;
    }
    private IEnumerator Wrap(IEnumerator r, System.Action done) { yield return r; done?.Invoke(); }
    private IEnumerator WaitX(float s)
    {
        if (s <= 0f) yield break;
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(s);
        else yield return new WaitForSeconds(s);
    }
}
