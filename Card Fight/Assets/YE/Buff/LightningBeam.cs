using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBeam : MonoBehaviour
{
    public float duration =1f;

    private void Start()
    {
        // �ӳ�����
        Destroy(gameObject, duration);
    }
}
