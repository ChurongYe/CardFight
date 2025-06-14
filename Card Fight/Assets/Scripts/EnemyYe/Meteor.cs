using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 10f;
    public Vector2 targetPosition;
    public float hitThreshold = 0.1f; // ����Ŀ��С�ڴ�ֵ����Ϊ������

    void Update()
    {
        // ��Ŀ��λ���ƶ�
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // �ж��Ƿ񵽴�Ŀ���
        if (Vector2.Distance(transform.position, targetPosition) <= hitThreshold)
        {
            // TODO: ���ű�ը��Ч���˺�����
            Destroy(gameObject,0.5f);
        }
    }
}