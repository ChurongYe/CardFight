using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingZone : MonoBehaviour
{
    public float duration = 0.5f;

    void Start()
    {
        // 0.5 ����Զ�����
        Destroy(gameObject, duration);
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, 1.5f);
    //}
}