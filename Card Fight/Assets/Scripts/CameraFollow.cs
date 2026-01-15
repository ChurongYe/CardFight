using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target; // ½ÇÉ«
    public Vector3 offset = new Vector3(0, 5, -10); // Ïà¶ÔÆ«ÒÆ

    private void Start()
    {
        target = GameObject.FindWithTag("Player");
    }
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.transform .position + offset;
        }
    }
}
