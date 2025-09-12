using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePickup : MonoBehaviour
{
    // —— 说话人/条目定义 ——
    public enum Speaker { None, Hero, Oldman }

    [System.Serializable]
    public class Entry
    {
        public Speaker speaker = Speaker.None;
        [TextArea] public string text;
        public Sprite overridePortrait;   // 可选：直接指定一个 Sprite 覆盖头像
        public bool waitForClick = true;
        public float preDelay = 0f;
        public float postDelay = 0f;
    }

    [Header("Dialogue")]
    public DialogueUI dialogueUI;              // 场景里的 DialogueUI
    public float fadeDuration = 0.2f;

    [Header("Portraits (Image，内部会取 .sprite)")]
    [SerializeField] private Image oldmanPortrait; // 老者头像（Image）
    [SerializeField] private Image heroPortrait;   // 主角头像（Image）

    [Header("Scripted Lines (推荐)")]
    public List<Entry> entries = new List<Entry>();

    [Header("Legacy Lines (兼容旧用法，无头像)")]
    [TextArea] public string[] lines;

    [Header("Pickup Settings")]
    public bool destroyAfter = true;
    public bool hideSpriteOnPickup = true;

    private bool triggered = false;
    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        if (hideSpriteOnPickup && sr) sr.enabled = false;
        if (col) col.enabled = false;

        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // 暂停游戏；DialogueUI 用 unscaled 时间，暂停下也能正常打字/点击
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;

        if (dialogueUI != null)
            yield return dialogueUI.FadeIn(fadeDuration);

        if (dialogueUI != null)
        {
            if (entries != null && entries.Count > 0)
            {
                // —— 新：逐句带头像 ——
                foreach (var e in entries)
                {
                    // 1) 选择头像
                    Sprite toUse = e.overridePortrait;
                    if (toUse == null)
                    {
                        if (e.speaker == Speaker.Hero) toUse = heroPortrait ? heroPortrait.sprite : null;
                        else if (e.speaker == Speaker.Oldman) toUse = oldmanPortrait ? oldmanPortrait.sprite : null;
                        else toUse = null; // None
                    }
                    dialogueUI.SetPortrait(toUse); // 让 DialogueUI 切头像（或清空）

                    // 2) 节奏控制
                    if (e.preDelay > 0f) yield return new WaitForSecondsRealtime(e.preDelay);

                    // 3) 打字 & 等点击
                    yield return dialogueUI.TypeLine(e.text);
                    if (e.waitForClick) yield return dialogueUI.WaitForClick();

                    if (e.postDelay > 0f) yield return new WaitForSecondsRealtime(e.postDelay);
                }
            }
            else if (lines != null && lines.Length > 0)
            {
                // —— 旧：只有文本，不切头像 ——
                dialogueUI.SetPortrait(null);
                foreach (var line in lines)
                {
                    yield return dialogueUI.TypeLine(line);
                    yield return dialogueUI.WaitForClick();
                }
            }
        }

        if (dialogueUI != null)
            yield return dialogueUI.FadeOut(fadeDuration);

        Time.timeScale = prevScale;

        if (destroyAfter) Destroy(gameObject);
    }
}
