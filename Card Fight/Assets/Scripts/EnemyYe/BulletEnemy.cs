using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;

    public LayerMask destroyOnHit; // ����Ϊ Player �� Wall

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        // �����ӵ���ת�Ƕȳ�����з���
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Start()
    {
        // �Զ����٣����� 5 ���û��ײ��
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �ж��Ƿ����С���ҡ���ǽ��
        if (((1 << other.gameObject.layer) & destroyOnHit) != 0)
        {
            // TODO��������������������߼��������Ѫ��
            Destroy(gameObject);
        }
    }
}
