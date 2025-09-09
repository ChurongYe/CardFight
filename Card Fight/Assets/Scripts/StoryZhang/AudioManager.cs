using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum MusicTrack { Menu, Black, Gameplay }

    [Header("Music Clips (exactly three)")]
    public AudioClip menuClip;
    public AudioClip blackClip;
    public AudioClip gameplayClip;

    [Header("Cross-fade")]
    [Range(0.01f, 8f)] public float defaultFade = 0.8f;
    public bool useUnscaledTime = true;

    private AudioSource _a, _b;     // BGM 交叉淡入
    private AudioSource _current, _next;

    // ✅ 新增：SFX 专用通道
    private AudioSource _sfx;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // 如果以后会切场景，保留；只用单场景也不影响
        // DontDestroyOnLoad(gameObject);

        _a = CreateSource("MusicA");
        _b = CreateSource("MusicB");
        _sfx = CreateSource("SFX");         // ✅
        _sfx.loop = false;                   // ✅

        _current = _a; _next = _b;
    }

    private AudioSource CreateSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
        return src;
    }

    public void Play(MusicTrack track, float fade = -1f, bool? loopOverride = null)
    {
        var clip = GetClip(track);
        if (clip == null) return;
        if (fade < 0f) fade = defaultFade;

        if (_current.clip == clip && _current.isPlaying) return;

        _next.clip = clip;
        _next.loop = loopOverride ?? true;
        _next.volume = 0f;
        _next.Play();

        StopAllCoroutines();
        StartCoroutine(CrossFade(_current, _next, fade));

        var t = _current; _current = _next; _next = t;
    }

    public void Stop(float fade = -1f)
    {
        if (fade < 0f) fade = defaultFade;
        StopAllCoroutines();
        StartCoroutine(FadeOut(_current, fade));
    }

    private AudioClip GetClip(MusicTrack t)
    {
        switch (t)
        {
            case MusicTrack.Menu: return menuClip;
            case MusicTrack.Black: return blackClip;
            case MusicTrack.Gameplay: return gameplayClip;
        }
        return null;
    }

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float duration)
    {
        if (duration <= 0.01f)
        {
            if (from.isPlaying) from.Stop();
            to.volume = 1f;
            yield break;
        }

        float t = 0f;
        float fromStart = from.volume;
        to.volume = 0f;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            to.volume = k;
            from.volume = Mathf.Lerp(fromStart, 0f, k);
            yield return null;
        }
        to.volume = 1f;
        if (from.isPlaying) from.Stop();
        from.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource src, float duration)
    {
        if (!src.isPlaying) yield break;
        float t = 0f, start = src.volume;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }
        src.Stop();
        src.volume = 1f;
    }

    // ✅ 新增：SFX 播放接口（StoryDirector 会调用它）
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfx == null) return;
        _sfx.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
