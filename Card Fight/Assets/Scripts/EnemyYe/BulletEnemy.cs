using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;

    public LayerMask destroyOnHit; // 设置为 Player 和 Wall

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        // 设置子弹旋转角度朝向飞行方向
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Start()
    {
        // 自动销毁（飞行 5 秒后还没碰撞）
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否命中“玩家”或“墙”
        if (((1 << other.gameObject.layer) & destroyOnHit) != 0)
        {
            // TODO：可以在这里添加命中逻辑，比如扣血等
            Destroy(gameObject);
        }
    }
}
