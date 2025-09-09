using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 对话框控制：逐字显示、等待点击、淡出隐藏。
/// 用法：
///   yield return dialogue.ShowAndWaitForClick("第一句台词", portrait:null);
///   yield return dialogue.HideWithFade(0.3f);
/// 或者：
///   yield return dialogue.Show("一句话", portrait); // 显示→等待一次点击→立即隐藏
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup panel;     // 指向 DialoguePanel 的 CanvasGroup
    [SerializeField] private TMP_Text text;         // 台词文本（TMP）
    [SerializeField] private Image portraitLeft;    // 左侧头像（可为空）
    [SerializeField] private Button clickCatcher;   // 覆盖整块的透明按钮（接收点击）

    [Header("Typewriter")]
    [Range(1, 120)]
    [SerializeField] private float charsPerSecond = 35f;     // 每秒字符数
    [SerializeField] private bool useUnscaledTime = true;    // 使用不受 Time.timeScale 影响的时间

    private bool _clicked;
    private bool _isTyping;

    private float DT => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    #region 生命周期
    private void Awake()
    {
        // 尝试自动补引用（避免忘记拖）
        if (!panel) panel = GetComponentInChildren<CanvasGroup>(true);
        if (!text) text = GetComponentInChildren<TMP_Text>(true);
        if (!clickCatcher)
        {
            // 优先找名为 ClickCatcher 的 Button；找不到就随便取一个 Button
            foreach (var btn in GetComponentsInChildren<Button>(true))
            {
                if (btn.name == "ClickCatcher") { clickCatcher = btn; break; }
                if (!clickCatcher) clickCatcher = btn;
            }
        }

        if (clickCatcher)
        {
            clickCatcher.onClick.AddListener(OnClick);

            // 确保有 Image 且能被射线命中
            var img = clickCatcher.GetComponent<Image>();
            if (!img) img = clickCatcher.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);  // 全透明
            img.raycastTarget = true;
        }

        HideImmediate(); // 初始隐藏
    }

    private void OnDestroy()
    {
        if (clickCatcher) clickCatcher.onClick.RemoveListener(OnClick);
    }
    #endregion

    #region 对外接口
    /// <summary>
    /// 显示并逐字播放，等待一次点击，但不自动隐藏（留给导演脚本决定何时淡出/隐藏）
    /// </summary>
    public IEnumerator ShowAndWaitForClick(string line, Sprite portrait = null, float? cpsOverride = null)
    {
        EnsureVisible(true);

        _clicked = false;
        _isTyping = true;

        if (portraitLeft)
        {
            if (portrait) { portraitLeft.enabled = true; portraitLeft.sprite = portrait; }
            else { portraitLeft.enabled = false; }
        }

        if (text) text.text = "";

        float cps = cpsOverride.HasValue ? cpsOverride.Value : charsPerSecond;
        cps = Mathf.Max(1f, cps);

        // 逐字机
        int shown = 0;
        while (shown < line.Length)
        {
            if (_clicked) // 点击跳至全文
            {
                if (text) text.text = line;
                _clicked = false;
                break;
            }

            shown++;
            if (text) text.text = line.Substring(0, shown);

            float wait = 1f / cps;
            float t = 0f;
            while (t < wait)
            {
                if (_clicked) { if (text) text.text = line; _clicked = false; t = wait; break; }
                t += DT;
                yield return null;
            }
        }

        _isTyping = false;

        // 等待一次点击（确认继续）
        while (!_clicked) yield return null;
        _clicked = false;
    }

    /// <summary>
    /// 便捷：显示→等待一次点击→立即隐藏
    /// </summary>
    public IEnumerator Show(string line, Sprite portrait = null)
    {
        yield return ShowAndWaitForClick(line, portrait);
        HideImmediate();
    }

    /// <summary>
    /// 对话面板淡出隐藏
    /// </summary>
    public IEnumerator HideWithFade(float duration)
    {
        if (!panel || !panel.gameObject.activeSelf) yield break;

        float start = panel.alpha;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += DT;
            panel.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }

        HideImmediate();
    }

    /// <summary>
    /// 立刻隐藏（无过渡）
    /// </summary>
    public void HideImmediate()
    {
        if (!panel) return;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);

        if (portraitLeft) portraitLeft.enabled = false;
        if (text) text.text = "";
    }
    #endregion

    #region 内部方法
    private void EnsureVisible(bool on)
    {
        if (!panel) return;

        panel.gameObject.SetActive(on);
        panel.alpha = on ? 1f : 0f;
        panel.interactable = on;
        panel.blocksRaycasts = on;

        if (clickCatcher)
        {
            clickCatcher.gameObject.SetActive(on);
            clickCatcher.enabled = on;

            var img = clickCatcher.GetComponent<Image>();
            if (img) img.raycastTarget = on;
        }
    }

    private void OnClick()
    {
        // 如果正在逐字，第一次点击会跳到全文；下一次点击才会继续
        _clicked = true;
    }
    #endregion
}
