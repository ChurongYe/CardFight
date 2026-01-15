using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public Vector2 targetPoint;
    public float initialSpeed = 5f;
    public float acceleration = 20f;
    public float maxSpeed = 30f;

    private Vector2 direction;
    private float currentSpeed;
    private bool isMoving = false;
    private Collider2D col;

    public void Init(Vector2 target)
    {
        targetPoint = target;
        direction = (targetPoint - (Vector2)transform.position).normalized;

        currentSpeed = initialSpeed;
        isMoving = true;

        col = GetComponent<Collider2D>();
        if (col) col.enabled = false; // 初始禁用碰撞体
    }

    void Update()
    {
        if (!isMoving) return;

        // 模拟加速下落
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            isMoving = false;
            if (col) col.enabled = true;
            Destroy(gameObject, 0.5f); // 到达后 0.5 秒消失
        }
    }
}