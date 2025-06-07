using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideBomberEnemy : EnemyManager 
{
    [Header("自爆敌人设置")]

    public float explodeDelay = 1f;                // 停止后多久自爆

    protected override void OnTryAttack()
    {
        ifattacking = true;
        // 停止后准备自爆
        StartCoroutine(ExplodeAfterDelay());
    }


    IEnumerator ExplodeAfterDelay()
    {

        yield return new WaitForSeconds(explodeDelay);

        // 播放爆炸特效
        if (attackArea)
        {
            attackArea.SetActive(true);
            Destroy(gameObject, 1f);
        }

    }

}