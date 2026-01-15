using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushProfeb : MonoBehaviour
{
    public float speed = 5f;                 // 子弹飞行速度
    public float lifetime = 100f;              // 飞行最大时间，超过后销毁

    void Start()
    {
        Destroy(gameObject, lifetime); // 超时销毁
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger && (other.CompareTag("Player") || other.CompareTag("Obstacle")))
        {
            Destroy(gameObject, 0.3f);
        }
    }
}
