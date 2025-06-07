using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 10f;
    public Vector2 targetPosition;
    public float hitThreshold = 0.1f; // 距离目标小于此值则认为已砸中

    void Update()
    {
        // 向目标位置移动
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // 判断是否到达目标点
        if (Vector2.Distance(transform.position, targetPosition) <= hitThreshold)
        {
            // TODO: 播放爆炸特效或伤害处理
            Destroy(gameObject,0.5f);
        }
    }
}