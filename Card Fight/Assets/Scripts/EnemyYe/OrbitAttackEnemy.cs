using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitAttackEnemy : EnemyManager
{
    [Header("��������")]
    public float orbitStartDistance = 3f;   // ������ת�ľ�����ֵ
    public float orbitRadius = 4f;
    public float orbitSpeed = 90f;          // ÿ��Ƕ�
    public float attackInterval = 2f;
    public float attackRange = 1.5f;
    public GameObject attackPivot;
    public float randomRadiusOffset = 0.3f;
    public float randomAngleJitter = 15f;

    private float currentAngle;
    private float attackTimer;
    private bool ifagent = false;
    protected override void Start()
    {
        base.Start();
        currentAngle = Random.Range(0f, 360f);
    }

    protected override void Update()
    {
        base.Update();

        if (currentTarget == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (distanceToTarget < orbitStartDistance)
        {
            ifagent = true;
            ifattacking = true;

            // �����ٶȺ�·����������
            agent.velocity = Vector3.zero;
            agent.enabled = false;

            OrbitAroundPlayer();

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackInterval;
            }
        }
        else
        {
            ResetAgent();
            agent.enabled = true;
            ifattacking = false;
        }
        attackPivot.transform.localScale = -transform.localScale;
    }
    void ResetAgent()
    {
        if(ifagent)
        {
            agent.Warp(transform .position );
            ifagent = false;
        }
    }
    void OrbitAroundPlayer()
    {
        float radiusOffset = Random.Range(-randomRadiusOffset, randomRadiusOffset);
        float jitter = Random.Range(-randomAngleJitter, randomAngleJitter) * Time.deltaTime;

        currentAngle += (orbitSpeed + jitter) * Time.deltaTime;
        float angleRad = currentAngle * Mathf.Deg2Rad;
        float effectiveRadius = orbitRadius + radiusOffset;

        // ����Ŀ��λ�ã�ȫ���� Vector2��
        Vector2 targetCenter = currentTarget.position;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * effectiveRadius;
        Vector2 targetPos = targetCenter + offset;

        // ���·���Ƿ�ǽ���ڵ�
        RaycastHit2D hit = Physics2D.Linecast(transform.position, targetPos, LayerMask.GetMask("Wall"));
        if (hit.collider == null)
        {
            // ���ϰ��������ƶ�
            Vector2 newPos = Vector2.Lerp((Vector2)transform.position, targetPos, Time.deltaTime * 5f);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            // ��ǽ�壬�򷴷�����΢ƫ��
            Vector2 safeDir = ((Vector2)transform.position - hit.point).normalized;
            Vector2 newPos = (Vector2)transform.position + safeDir * Time.deltaTime;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }

        FaceTarget(currentTarget);
    }

    void Attack()
    {
        Debug.Log("���˹�����ң�");
        if (attackArea && currentTarget)
        {
            // �ù����ᣨattackPivot������Ŀ��
            Vector2 direction = currentTarget.position - attackPivot.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            attackPivot.transform.rotation = Quaternion.Euler(0, 0, angle);
            StartCoroutine(ActivateAttackArea());
        }
    }
    IEnumerator ActivateAttackArea()
    {
        attackArea.SetActive(true);
        yield return new WaitForSeconds(0.2f); // ������Чʱ�䣬0.2 ��ɵ���
        attackArea.SetActive(false);
    }
}