using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // ===== Singleton =====
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化音乐源
        SetupMusicSource(musicA);
        SetupMusicSource(musicB);

        // 初始音量
        ApplyVolume(masterVolume, musicVolume, sfxVolume);
    }

    void SetupMusicSource(AudioSource src)
    {
        if (!src) return;
        src.playOnAwake = false;
        src.loop = true;
        src.volume = 0f;
        src.spatialBlend = 0f;
        if (musicGroup) src.outputAudioMixerGroup = musicGroup;
    }

    // ===== Mixer / Volume =====
    [Header("Audio Mixer")]
    [Tooltip("主Mixer")]
    public AudioMixer mixer;
    [Tooltip("Master组(可选)")] public AudioMixerGroup masterGroup;
    [Tooltip("Music组(用于BGM)")] public AudioMixerGroup musicGroup;
    [Tooltip("SFX组(用于音效)")] public AudioMixerGroup sfxGroup;

    [Header("Exposed Param Names (必须与Mixer一致)")]
    public string masterParam = "MasterVol";   // dB
    public string musicParam = "MusicVol";    // dB
    public string sfxParam = "SFXVol";      // dB

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); SetDb(masterParam, v); }
    public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); SetDb(musicParam, v); }
    public void SetSfxVolume(float v) { sfxVolume = Mathf.Clamp01(v); SetDb(sfxParam, v); }

    void ApplyVolume(float master, float music, float sfx)
    {
        SetDb(masterParam, master);
        SetDb(musicParam, music);
        SetDb(sfxParam, sfx);
    }

    void SetDb(string exposedParam, float linear01)
    {
        if (!mixer || string.IsNullOrEmpty(exposedParam)) return;
        // 线性到dB，0则给一个较小的静音值
        float dB = linear01 > 0.0001f ? Mathf.Log10(linear01) * 20f : -80f;
        mixer.SetFloat(exposedParam, dB);
    }

    // ===== Music (Cross Fade) =====
    [Header("Music Sources (两个交叉淡入淡出)")]
    public AudioSource musicA;
    public AudioSource musicB;
    [Tooltip("默认淡入淡出时长(秒)")]
    public float defaultFade = 2f;
    [Tooltip("交叉淡入淡出使用UnscaledTime")]
    public bool useUnscaledTime = true;

    bool _usingA = true;
    Coroutine _musicCo;

    AudioSource Active => _usingA ? musicA : musicB;
    AudioSource Idle => _usingA ? musicB : musicA;

    [Header("Music Clips")]
    public AudioClip menuClip;
    public AudioClip blackClip;
    public AudioClip gameplayClip;

    public void PlayMenuMusic(float fade = -1f) => PlayMusic(menuClip, fade);
    public void PlayBlackMusic(float fade = -1f) => PlayMusic(blackClip, fade);
    public void PlayGameplayMusic(float fade = -1f) => PlayMusic(gameplayClip, fade);

    public void PlayMusic(AudioClip clip, float fade = -1f)
    {
        if (!clip) return;
        if (fade < 0f) fade = defaultFade;
        if (_musicCo != null) StopCoroutine(_musicCo);
        _musicCo = StartCoroutine(CoCrossFade(clip, fade));
    }

    IEnumerator CoCrossFade(AudioClip nextClip, float duration)
    {
        var from = Active;
        var to = Idle;

        // 准备to
        to.clip = nextClip;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float k = duration > 0 ? t / duration : 1f;
            from.volume = Mathf.Lerp(1f, 0f, k);
            to.volume = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }

        from.volume = 0f; from.Stop();
        to.volume = 1f;
        _usingA = !_usingA;
        _musicCo = null;
    }

    // ===== SFX =====
    [Header("SFX (2D UI)")]
    public AudioSource uiSfxSource; // 2D 一次性SFX
    [Header("SFX 3D Prefab (带AudioSource，自动销毁)")]
    public AudioSource sfx3DPrefab;

    [Header("SFX Clips")]
    public AudioClip doorOpen;
    public AudioClip footstep;
    public AudioClip dash;
    public AudioClip attack;
    public AudioClip cardClick;
    public AudioClip cardPlay;
    public AudioClip getHit;
    public AudioClip castSkill;

    public enum Sfx
    {
        DoorOpen, Footstep, Dash, Attack, CardClick, CardPlay, GetHit, CastSkill
    }

    public void PlaySfx(Sfx sfx, float volume = 1f) =>
        PlayOneShot2D(GetSfxClip(sfx), volume);

    public void PlaySfxAt(Sfx sfx, Vector3 worldPos, float volume = 1f) =>
        PlayOneShot3D(GetSfxClip(sfx), worldPos, volume);

    AudioClip GetSfxClip(Sfx id)
    {
        switch (id)
        {
            case Sfx.DoorOpen: return doorOpen;
            case Sfx.Footstep: return footstep;
            case Sfx.Dash: return dash;
            case Sfx.Attack: return attack;
            case Sfx.CardClick: return cardClick;
            case Sfx.CardPlay: return cardPlay;
            case Sfx.GetHit: return getHit;
            case Sfx.CastSkill: return castSkill;
        }
        return null;
    }

    public void PlayOneShot2D(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
        if (!uiSfxSource)
        {
            // 兜底：创建一个
            uiSfxSource = gameObject.AddComponent<AudioSource>();
            uiSfxSource.playOnAwake = false;
            uiSfxSource.spatialBlend = 0f;
            if (sfxGroup) uiSfxSource.outputAudioMixerGroup = sfxGroup;
        }
        uiSfxSource.PlayOneShot(clip, volume);
    }

    public void PlayOneShot3D(AudioClip clip, Vector3 worldPos, float volume = 1f, float life = 3f)
    {
        if (!clip || !sfx3DPrefab) { PlayOneShot2D(clip, volume); return; }
        var inst = Instantiate(sfx3DPrefab, worldPos, Quaternion.identity);
        if (sfxGroup && inst.outputAudioMixerGroup != sfxGroup)
            inst.outputAudioMixerGroup = sfxGroup;
        inst.spatialBlend = 1f;
        inst.PlayOneShot(clip, volume);
        Destroy(inst.gameObject, life);
    }

    // ====== 旧接口兼容层（无需修改其它脚本） ======

    // 旧项目里存在的枚举名
    public enum MusicTrack { Menu, Black, Gameplay }

    // 旧写法：AudioManager.Instance.Play(AudioManager.MusicTrack.XXX);
    //public void Play(MusicTrack track) { Play(track, -1f); }

    // 旧写法：AudioManager.Instance.Play(AudioManager.MusicTrack.XXX, fade);
    public void Play(MusicTrack track, float fade)
    {
        switch (track)
        {
            case MusicTrack.Menu: PlayMenuMusic(fade); break;
            case MusicTrack.Black: PlayBlackMusic(fade); break;
            case MusicTrack.Gameplay: PlayGameplayMusic(fade); break;
        }
    }

    // 有些旧代码可能有第三个布尔参数（忽略即可）
    //public void Play(MusicTrack track, float fade, bool _ignored) => Play(track, fade);

    // 旧写法：AudioManager.Play(...)
    //public static void Play(MusicTrack track) { Instance?.Play(track); }
    //public static void Play(MusicTrack track, float fade) { Instance?.Play(track, fade); }
    //public static void Play(MusicTrack track, float fade, bool ignore) { Instance?.Play(track, fade, ignore); }
}
