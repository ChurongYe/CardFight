using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Shield : MonoBehaviour
{
    private Vector3 shieldForward;

    void Start()
    {
        shieldForward = transform.forward;  
    }

    // �жϹ����Ƿ����Զ��Ƶ�ǰ��
    public bool IsAttackFromFront(Vector3 attackDirection)
    {
        // ���㹥��������������淽��ĵ��
        float dotProduct = Vector3.Dot(attackDirection.normalized, shieldForward.normalized);

        // ����������0��˵���������Զ���ǰ��
        return dotProduct > 0;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ��ȡ�������򣨵��˵Ĺ����ĳ���������
        Vector3 attackDirection = other.transform.position - transform.position;

        // �жϹ����Ƿ����Զ��Ƶ�ǰ��
        if (IsAttackFromFront(attackDirection))
        {
            // ���ع���
            Debug.Log("���������Ƶ�ס");
            // ��������������ص���Ϊ��������ˡ�������
        }
        else
        {
            // �������б���
            Debug.Log("�����ɹ������б���");
            // �����˺������
        }
    }
}

public class EnemyAttack : MonoBehaviour
{
    public GameObject target; // ��һ�����Ŀ��

    void Attack()
    {
        Vector3 attackDirection = (target.transform.position - transform.position).normalized;

        // ���ö��Ƶ� IsAttackFromFront �������жϹ�������
        Shield shield = target.GetComponentInChildren<Shield>();  // �����������ҵ�������
        if (shield != null && shield.IsAttackFromFront(attackDirection))
        {
            // ����������Զ��Ƶ�ǰ�������Ա���ס
            Debug.Log("��������ס");
        }
        else
        {
            // ���򹥻����Դӱ�������
            Debug.Log("�����ɹ�");
        }
    }
}

