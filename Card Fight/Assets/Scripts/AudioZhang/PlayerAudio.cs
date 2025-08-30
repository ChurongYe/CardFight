using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip footstepLoop;   // 跑步循环
    public AudioClip dashClip;       // 冲刺一次

    [Header("Volumes")]
    [Range(0, 1)] public float footstepVol = 0.6f;
    [Range(0, 1)] public float dashVol = 1.0f;

    [Header("Detect Settings")]
    public float moveThreshold = 0.02f;    // 速度阈值：判定“在移动”
    public float dashMuteDuration = 0.15f; // 冲刺期间暂时静音脚步

    AudioSource footSrc;   // 脚步专用（循环）
    AudioSource sfxSrc;    // 冲刺专用（一次性）
    Rigidbody2D rb;
    float dashMuteTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 用身上的 AudioSource 做脚步
        footSrc = GetComponent<AudioSource>();
        footSrc.playOnAwake = false;
        footSrc.loop = true;
        footSrc.spatialBlend = 0f;

        // 另建一个子物体做冲刺SFX（完全独立，不受脚步影响）
        var go = new GameObject("SFX_Source");
        go.transform.SetParent(transform, false);
        sfxSrc = go.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        sfxSrc.loop = false;
        sfxSrc.spatialBlend = 0f;
        sfxSrc.volume = 1f;          // OneShot 的基底音量
    }

    void Update()
    {
        // 左右Shift都有效
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && dashClip)
        {
            PlayDashSFX();
        }

        // 冲刺静音计时
        if (dashMuteTimer > 0f) dashMuteTimer -= Time.deltaTime;

        // 是否在移动（优先刚体速度；没有刚体就用输入轴兜底）
        bool isMoving = rb ? rb.velocity.sqrMagnitude > moveThreshold * moveThreshold
                           : (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                              Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f);

        HandleFootstep(isMoving && dashMuteTimer <= 0f);
    }

    void HandleFootstep(bool moving)
    {
        if (moving)
        {
            if (footstepLoop && (!footSrc.isPlaying || footSrc.clip != footstepLoop))
            {
                footSrc.clip = footstepLoop;
                footSrc.volume = footstepVol;
                footSrc.Play();
            }
        }
        else
        {
            if (footSrc.isPlaying) footSrc.Stop();
        }
    }

    void PlayDashSFX()
    {
        // 冲刺瞬间：停脚步，开启静音窗口 + 播一次性冲刺音
        if (footSrc.isPlaying) footSrc.Stop();
        dashMuteTimer = dashMuteDuration;

        sfxSrc.PlayOneShot(dashClip, dashVol);
        Debug.Log("[PlayerAudio] Dash SFX played");   // 看到这条日志说明代码已触发
    }
}
