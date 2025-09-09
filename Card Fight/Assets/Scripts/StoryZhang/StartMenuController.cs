using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas startCanvas;
    [SerializeField] private Canvas optionsCanvas;
    [SerializeField] private CanvasGroup blackFader;

    [Header("Buttons")]
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button closeOptionsBtn;

    [Header("Fade Config")]
    [SerializeField, Range(0.05f, 8f)] private float fadeToBlack = 1.2f;
    [SerializeField, Range(0.05f, 8f)] private float fadeBackToGame = 1.0f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blockClicksWhileFading = true;

    [Header("Freeze Gameplay until Start")]
    [SerializeField] private bool freezeGameplayOnMenu = true;
    [SerializeField] private MonoBehaviour[] disableOnMenu;

    [Header("Story")]
    [SerializeField] private StoryDirector story;   // ← 拖上面的 StoryDirector

    private bool isStarting = false;

    private void Awake()
    {
        if (freezeGameplayOnMenu) SetGameplayActive(false);

        if (blackFader != null)
        {
            blackFader.alpha = 0f;     // 初始不遮
            blackFader.interactable = false;
            blackFader.blocksRaycasts = false;
        }
        if (optionsCanvas != null) optionsCanvas.gameObject.SetActive(false);

        if (newGameBtn) newGameBtn.onClick.AddListener(OnClickNewGame);
        if (optionsBtn) optionsBtn.onClick.AddListener(() => optionsCanvas?.gameObject.SetActive(true));
        if (exitBtn) exitBtn.onClick.AddListener(OnClickExit);
        if (closeOptionsBtn) closeOptionsBtn.onClick.AddListener(() => optionsCanvas?.gameObject.SetActive(false));
    }

    private void Start()
    {
        // 菜单音乐
        if (AudioManager.Instance) AudioManager.Instance.Play(AudioManager.MusicTrack.Menu, 0.4f);
    }

    private void OnClickNewGame()
    {
        if (isStarting) return;
        isStarting = true;
        StartCoroutine(StartFlow());
    }

    private IEnumerator StartFlow()
    {
        // 禁用菜单交互并保持可见
        SetCanvasInteractable(startCanvas, false);

        // 先淡到黑（覆盖）
        if (blackFader != null)
        {
            blackFader.blocksRaycasts = blockClicksWhileFading;
            yield return FadeTo(1f, fadeToBlack);
        }

        // 切黑屏音乐
        if (AudioManager.Instance) AudioManager.Instance.Play(AudioManager.MusicTrack.Black, 0.5f, loopOverride: false);

        // 隐藏开始菜单
        HideStartCanvasSafe();

        // 交给 StoryDirector 跑整段演出（期间保持 timeScale=0）
        if (story) yield return story.PlayOpeningSequence();

        // 演出结束 → 正式进入游戏
        SetGameplayActive(true);
        if (AudioManager.Instance) AudioManager.Instance.Play(AudioManager.MusicTrack.Gameplay, 1.0f);

        // 从黑幕回到可见（若演出已把黑幕设为 0，这里会很快）
        if (blackFader != null && blackFader.alpha > 0f)
            yield return FadeTo(0f, fadeBackToGame);

        // 黑幕不再拦截
        if (blackFader != null) blackFader.blocksRaycasts = false;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = blackFader.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            blackFader.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        blackFader.alpha = target;
    }

    private void HideStartCanvasSafe()
    {
        if (!startCanvas) return;
        if (startCanvas.gameObject == this.gameObject) startCanvas.enabled = false;
        else startCanvas.gameObject.SetActive(false);
    }

    private void SetCanvasInteractable(Canvas canvas, bool interactable)
    {
        if (!canvas) return;
        if (!canvas.TryGetComponent<CanvasGroup>(out var group))
            group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    private void SetGameplayActive(bool active)
    {
        Time.timeScale = active ? 1f : 0f;
        if (disableOnMenu != null)
            foreach (var m in disableOnMenu) if (m) m.enabled = active;
    }

    private void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
