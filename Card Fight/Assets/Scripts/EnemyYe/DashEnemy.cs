using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Core.CardValue;

public class DashEnemy : EnemyManager 
{
    [Header("��̹���")]
    public float dashSpeed = 10f;
    public float dashDistance = 5f;
    public float attackCooldown = 1f;

    private bool isAttacking = false;

    public LayerMask obstacleMask; // ������ֹ��ǽ

    protected override void OnTryAttack()
    {
        ifattacking = true;
        if (!isAttacking && currentTarget != null)
        {
            agent.enabled = false;
            StartCoroutine(DashAndAttackRoutine());
        }
    }

    IEnumerator DashAndAttackRoutine()
    {
        isAttacking = true;

        // ����Ŀ��
        FaceTarget(currentTarget);
        Vector2 dir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;

        // ͣ�� 0.5 �벥��ǰҡ����
        yield return new WaitForSeconds(0.5f);

        float overshootDistance = Random.Range(0f, 3f); // ������һ��ľ���
        Vector2 desiredEndPos = (Vector2)transform.position + dir * (dashDistance + overshootDistance);
        // ���Ŀ����⣨��ֹ��ǽ��
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, (dashDistance + overshootDistance), obstacleMask);
        Vector2 targetPos = hit.collider != null ? hit.point : desiredEndPos;

        float dashTime = 0f;
        Vector2 startPos = transform.position;;

        while (dashTime < dashDistance / dashSpeed)
        {
            dashTime += Time.deltaTime;
            float t = dashTime / (dashDistance / dashSpeed);
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            if (attackArea != null) attackArea.SetActive(true);
            // �������Ŀ��ǽ�ھ��루������
            if (Vector2.Distance(transform.position, targetPos) < 0.05f) break;

            yield return null;
        }
        if (attackArea != null) attackArea.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        agent.Warp(transform.position);// ǿ��ͬ�� agent ��λ��
        agent.enabled = true;
        ifattacking = false;
    }
}