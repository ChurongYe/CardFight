using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; 
    private CinemachineBasicMultiChannelPerlin perlin; 

    public float defaultShakeMagnitude = 0.2f; // 抖动幅度
    public float defaultShakeFrequency = 2f; // 抖动频率
    private float shakeTime = 0f; // 抖动持续时间

    void Start()
    {
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); 
        perlin.m_AmplitudeGain = 0f;
        perlin.m_FrequencyGain = 0f;
    }

    void Update()
    {
        // 检查是否还在抖动
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                StopShake();
            }
        }
    }

    // 触发默认抖动
    public void TriggerShake(float magnitude = 0f, float frequency = 0f, float duration = 0f)
    {
        if (magnitude == 0f) magnitude = defaultShakeMagnitude;  // 如果未传入抖动幅度，使用默认值
        if (frequency == 0f) frequency = defaultShakeFrequency; // 如果未传入频率，使用默认值

        perlin.m_AmplitudeGain = magnitude; // 设置抖动幅度
        perlin.m_FrequencyGain = frequency; // 设置抖动频率
        shakeTime = duration; // 设置持续时间
    }

    // 停止抖动
    private void StopShake()
    {
        perlin.m_AmplitudeGain = 0f;
        perlin.m_FrequencyGain = 0f;
    }

    public void TriggerStrongShake()
    {
        TriggerShake(0.5f, 3f, 0.5f); // 强烈抖动
    }

    public void TriggerLightShake()
    {
        TriggerShake(1f, 0.05f, 0.1f); // 轻微抖动
    }

    public void TriggerExplosionShake()
    {
        TriggerShake(1.0f, 5f, 0.2f); // 爆炸震动
    }
}