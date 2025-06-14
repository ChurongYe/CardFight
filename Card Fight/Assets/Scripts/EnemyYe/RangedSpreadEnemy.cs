using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedSpreadEnemy : EnemyManager
{
    [Header("远程攻击设置")]
    public GameObject bulletPrefab;            // 子弹预制体
    public float fireInterval = 2f;            // 每次发射间隔
    public float bulletSpeed = 8f;             // 子弹速度
    public float spreadAngle = 15f;            // 左右子弹偏移角度
    public Transform firePoint;                // 子弹发射点

    private float fireTimer;

    protected override void Update()
    {
        base.Update();

        if (currentTarget == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireSpread();
            fireTimer = fireInterval;
        }

        FaceTarget(currentTarget);
    }

    void FireSpread()
    {
        if (!bulletPrefab || !firePoint) return;

        Vector2 dirToTarget = (currentTarget.position - firePoint.position).normalized;

        // 中间直线子弹
        FireBullet(dirToTarget);

        // 左右偏移子弹
        FireBullet(RotateVector(dirToTarget, spreadAngle));
        FireBullet(RotateVector(dirToTarget, -spreadAngle));
    }

    void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.transform.parent = transform;
        bullet.GetComponent<BulletEnemy>().SetDirection(direction);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * bulletSpeed;
        }

        bullet.transform.right = direction; // 设置子弹朝向
    }

    /// <summary>
    /// 旋转一个方向向量（2D）一定角度
    /// </summary>
    Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}