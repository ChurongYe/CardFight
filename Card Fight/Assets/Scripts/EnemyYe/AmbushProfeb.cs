using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushProfeb : MonoBehaviour
{
    public float speed = 5f;                 // �ӵ������ٶ�
    public float lifetime = 100f;              // �������ʱ�䣬����������

    void Start()
    {
        Destroy(gameObject, lifetime); // ��ʱ����
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger && (other.CompareTag("Player") || other.CompareTag("Obstacle")))
        {
            Destroy(gameObject, 0.3f);
        }
    }
}
