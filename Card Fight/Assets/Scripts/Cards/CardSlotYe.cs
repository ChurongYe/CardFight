using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotYe : MonoBehaviour
{
    public CardDisplayYe currentCard;
    public bool isCoolingDown = false;
    [Header("Slot State")]
    public bool isLocked = false; // �������Ƿ�����
    public Image lockOverlay; // UIͼ�㣺������ʾ��������ͼ��͸����
    public float cooldownTime = 2f;
    private float cooldownTimer = 5f;
    public Image cooldownOverlay;


    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(locked);
    }

    public void StartCooldown(System.Action onCooldownEnd)
    {
        StartCoroutine(CooldownCoroutine(onCooldownEnd));
    }

    private IEnumerator CooldownCoroutine(System.Action onCooldownEnd)
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownTimer); // ��ȴʱ��
        isCoolingDown = false;
        onCooldownEnd?.Invoke();
    }
}
