using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideBomberEnemy : EnemyManager 
{
    [Header("�Ա���������")]

    public float explodeDelay = 1f;                // ֹͣ�����Ա�

    protected override void OnTryAttack()
    {
        ifattacking = true;
        // ֹͣ��׼���Ա�
        StartCoroutine(ExplodeAfterDelay());
    }


    IEnumerator ExplodeAfterDelay()
    {

        yield return new WaitForSeconds(explodeDelay);

        // ���ű�ը��Ч
        if (attackArea)
        {
            attackArea.SetActive(true);
            Destroy(gameObject, 1f);
        }

    }

}