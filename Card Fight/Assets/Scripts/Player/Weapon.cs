using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform weaponPivot;
    public GameObject weaponSprite;
    public GameObject Player;
    private GameObject Face;
    public float swingAngle = 180f;
    public float swingDuration = 0.25f;
    public float comboInterval = 0.4f;

    private bool isSwinging = false;
    private float lastClickTime = -1f;
    private bool isLeftToRight = true;

    //public bool IsSwinging => isSwinging;
    private void Start()
    {
        Face = GameObject.FindWithTag("Face");
        Player = GameObject.FindWithTag("Player");
        weaponPivot.transform.parent = Player.transform;
    }

    public void TrySwing(float chargePercent)
    {
        float range = Mathf.Lerp(1f, 3f, chargePercent);   // 1 ~ 3 米攻击范围
        float force = Mathf.Lerp(10f, 30f, chargePercent); // 10 ~ 30 冲击力度
        if (!isSwinging)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= comboInterval)
                isLeftToRight = !isLeftToRight;
            else
                isLeftToRight = true;

            lastClickTime = Time.time;
            StartCoroutine(SwingWeapon(range, force));
        }
    }

    IEnumerator SwingWeapon(float range, float force)
    {
        weaponSprite.SetActive(true);
        isSwinging = true;

        float timer = 0f;

        Vector3 facingDir = Face.transform.right;
        float baseAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg - 90f;

        float startZ = baseAngle + (isLeftToRight ? -swingAngle / 2 : swingAngle / 2);
        float endZ = baseAngle + (isLeftToRight ? swingAngle / 2 : -swingAngle / 2);

        while (timer < swingDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / swingDuration);
            float angle = Mathf.Lerp(startZ, endZ, t);
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // 执行冲击逻辑
        ApplyImpact(range, force);

        isSwinging = false;
        weaponSprite.SetActive(false);
    }
    void ApplyImpact(float range, float force)
    {
        // 假设有敌人在范围内
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(weaponPivot.position, range);

        foreach (var enemy in enemiesInRange)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // 施加冲击力（比如击退）
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 impactDirection = (enemy.transform.position - weaponPivot.position).normalized;
                    rb.AddForce(impactDirection * force, ForceMode2D.Impulse);
                }

                // 可以在这里添加其他效果，比如伤害等
                // enemy.GetComponent<EnemyHealth>().TakeDamage(damage);
            }
        }
    }
}