using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardYe : MonoBehaviour
{
    // -------------------- ��ս������ʽ --------------------
    public enum MeleeType
    {
        Normal,
        FlameSlash,
        Whirlwind
    }
    // -------------------- Զ�̹�����ʽ --------------------
    public enum RangedType
    {
        Normal,
        Arrow,
        Boomerang,
    }
    // -------------------- �ٻ���--------------------
    public GameObject[] Summon;
    private List<ISummonUnit> activeSummons = new List<ISummonUnit>();

    // -------------------- ������ʽ�л� --------------------
    public MeleeType currentMelee = MeleeType.Normal;
    public RangedType currentRanged = RangedType.Normal;
    private enum AttackSlot { Melee, Ranged }
    private AttackSlot currentSlot = AttackSlot.Melee;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            currentSlot = currentSlot == AttackSlot.Melee ? AttackSlot.Ranged : AttackSlot.Melee;
            Debug.Log("�л���������λ��" + currentSlot);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseAttack();
        }
    }
    public void UseAttack()
    {
        if (currentSlot == AttackSlot.Melee)
            PerformMeleeAttack(currentMelee);
        else
            PerformRangedAttack(currentRanged);
    }
    // ���ɿ��Ƶ���
    public void SetMelee(MeleeType newType) => currentMelee = newType;
    public void SetRanged(RangedType newType) => currentRanged = newType;

    // -------------------- ������Ϊ --------------------
    #region ��ս
    void PerformMeleeAttack(MeleeType type)
    {
        switch (type)
        {
            case MeleeType.Normal:
                Debug.Log("��ͨ��ս����");
                break;
            case MeleeType.FlameSlash:
                Debug.Log("����ն��");
                break;
            case MeleeType.Whirlwind:
                Debug.Log("����ն");
                break;
        }
    }
    #endregion
    #region Զ��
    void PerformRangedAttack(RangedType type)
    {
        switch (type)
        {
            case RangedType.Normal:
                Debug.Log("��ͨԶ�̹���");
                break;
            case RangedType.Arrow:
                Debug.Log("���");
                break;
            case RangedType.Boomerang:
                Debug.Log("Ͷ��������");
                break;
        }
    }
    #endregion
    #region �ٻ�
    // ���÷�ʽ������ȷ���ٻ������� -> �ϵ�����������ͷ�λ�� -> ����
    public void SummonAttack(int summonType, Vector3 dropPosition)
    {
        StartCoroutine(SummonAttackCoroutine(summonType, dropPosition));
    }

    private IEnumerator SummonAttackCoroutine(int summonType, Vector3 dropPosition)
    {
        if (summonType < 0 || summonType >= Summon.Length)
        {
            Debug.LogWarning("�ٻ���������: " + summonType);
            yield break;
        }

        yield return new WaitForSeconds(1f); // �ȴ�����

        GameObject summon = Instantiate(Summon[summonType], dropPosition, Quaternion.identity);

        ISummonUnit unit = summon.GetComponent<ISummonUnit>();
        if (unit != null)
        {
            unit.SetManager(this);    
            activeSummons.Add(unit);  
        }
        else
        {
            Debug.LogWarning("�ٻ���û��ʵ�� ISummonUnit �ӿڣ�");
        }
    }

    public void RemoveSummon(ISummonUnit unit)
    {
        if (activeSummons.Contains(unit))
        {
            activeSummons.Remove(unit);
        }
    }
    public float buffMultiplier;
    public void MultiplyAllSummons(float multiplier)
    {
        foreach (var unit in activeSummons)
        {
            unit.MultiplyStats(buffMultiplier);
        }
    }
    #endregion
}
