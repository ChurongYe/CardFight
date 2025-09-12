using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup group;          // DialogueCanvas 的 CanvasGroup
    [SerializeField] private Image portraitImage;        // 头像 Image（可为空）
    [SerializeField] private TextMeshProUGUI textUI;     // 文本
    [SerializeField] private Button clickCatcher;        // 全屏 Button（点击继续）
    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 30f; // 打字速度
    [SerializeField] private bool useUnscaledTime = true;

    public bool IsTyping { get; private set; }

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        clickCatcher = GetComponentInChildren<Button>(true);
        textUI = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void SetPortrait(Sprite sprite)
    {
        if (portraitImage == null) return;
        portraitImage.enabled = (sprite != null);
        portraitImage.sprite = sprite;
    }

    public IEnumerator FadeIn(float dur, bool clearTextFirst = false, bool hidePortrait = false)
    {
        if (clearTextFirst)
        {
            if (textUI) textUI.text = "";
            if (hidePortrait) SetPortrait(null);
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (group) group.alpha = 0f; // 确保从 0 开始
        yield return FadeCanvas(group, 1f, dur, true, true, useUnscaledTime);
    }


    public IEnumerator FadeOut(float dur)
    {
        yield return FadeCanvas(group, 0f, dur, false, false, useUnscaledTime);
        // 可选：textUI.text = "";
    }


    public IEnumerator TypeLine(string line)
    {
        IsTyping = true;
        textUI.text = "";
        float t = 0f;
        while (t < line.Length)
        {
            int show = Mathf.FloorToInt(t);
            textUI.text = line.Substring(0, Mathf.Clamp(show, 0, line.Length));
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * charsPerSecond;
            yield return null;
        }
        textUI.text = line;
        IsTyping = false;
    }

    public IEnumerator WaitForClick()
    {
        bool clicked = false;
        clickCatcher.gameObject.SetActive(true);
        clickCatcher.onClick.RemoveAllListeners();
        clickCatcher.onClick.AddListener(() => {
            // 播放点击音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.CardClick);
            }
            clicked = true;
        });

        while (!clicked) yield return null;

        clickCatcher.onClick.RemoveAllListeners();
        clickCatcher.gameObject.SetActive(false);
    }

    // 供外部复用的 UI 淡入淡出
    public static IEnumerator FadeCanvas(CanvasGroup g, float to, float dur, bool interactable, bool blocksRaycast, bool unscaled = true)
    {
        if (g == null) yield break;
        float from = g.alpha;
        float t = 0f;
        while (t < dur)
        {
            t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, dur <= 0f ? 1f : t / dur);
            yield return null;
        }
        g.alpha = to;
        g.interactable = interactable;
        g.blocksRaycasts = blocksRaycast;
    }
}
