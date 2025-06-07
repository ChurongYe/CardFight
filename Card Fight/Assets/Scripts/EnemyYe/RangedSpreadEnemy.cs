using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedSpreadEnemy : EnemyManager
{
    [Header("Զ�̹�������")]
    public GameObject bulletPrefab;            // �ӵ�Ԥ����
    public float fireInterval = 2f;            // ÿ�η�����
    public float bulletSpeed = 8f;             // �ӵ��ٶ�
    public float spreadAngle = 15f;            // �����ӵ�ƫ�ƽǶ�
    public Transform firePoint;                // �ӵ������

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

        // �м�ֱ���ӵ�
        FireBullet(dirToTarget);

        // ����ƫ���ӵ�
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

        bullet.transform.right = direction; // �����ӵ�����
    }

    /// <summary>
    /// ��תһ������������2D��һ���Ƕ�
    /// </summary>
    Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}