using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorEnemy : EnemyManager 
{
    [Header("攻击设置")]
    public GameObject meteorPrefab;
    public float summonInterval = 4f;              // 冷却时间
    public float meteorInterval = 3f;            // 陨石之间的间隔
    public int meteorCount = 3;
    public float summonHeight = 10f;
    public float horizontalOffset = 2f;
    public GameObject warningCirclePrefab;
    public float warningDuration = 1.5f;           // 红圈存在时间

    private float attackTimer;

    protected override void Update()
    {
        base.Update();
        if (currentTarget == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            StartCoroutine(SummonMeteorsAndResetCooldown());
            attackTimer = float.MaxValue; // 暂时阻止重复触发，直到协程里重置
        }
    }

    IEnumerator SummonMeteorsAndResetCooldown()
    {
        for (int i = 0; i < meteorCount; i++)
        {
            SummonMeteor();
            yield return new WaitForSeconds(meteorInterval);
        }

        attackTimer = summonInterval; // 所有陨石召唤完之后才开始冷却
    }

    void SummonMeteor()
    {
        if (currentTarget == null || meteorPrefab == null) return;

        Vector2 targetPos = currentTarget.position;
        float randomXOffset = Random.Range(-horizontalOffset, horizontalOffset);
        Vector2 spawnPos = new Vector2(targetPos.x + randomXOffset, targetPos.y + summonHeight);
        Vector2 landingPos = new Vector2(spawnPos.x, targetPos.y); // 落点只取Y与玩家一致

        // 创建提示圈
        if (warningCirclePrefab != null)
        {
            GameObject warning = Instantiate(warningCirclePrefab, landingPos, Quaternion.identity);
            Destroy(warning, warningDuration);
        }

        StartCoroutine(SummonMeteorDelayed(spawnPos, landingPos, warningDuration));
    }

    IEnumerator SummonMeteorDelayed(Vector2 spawnPos, Vector2 landingPos, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
        meteor.transform.parent = transform;
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        if (meteorScript != null)
        {
            meteorScript.targetPosition = landingPos;
        }
    }
}