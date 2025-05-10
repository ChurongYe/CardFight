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
    // 冲击力的最小和最大值
    public float minImpactForce = 10f;
    public float maxImpactForce = 30f;

    private bool isSwinging = false;
    private float lastClickTime = -1f;
    private bool isLeftToRight = true;
    //public bool IsSwinging => isSwinging;
    private void Start()
    {
        weaponSprite = GameObject.FindWithTag("weaponSprite");
        weaponSprite.SetActive(false );
        Face = GameObject.FindWithTag("Face");
        Player = GameObject.FindWithTag("Player");
        weaponPivot.transform.parent = Player.transform;
    }

    public void TrySwing(float chargePercent)
    {
        float force = Mathf.Lerp(minImpactForce, maxImpactForce, chargePercent);
        float scale = Mathf.Lerp(1f, 1.5f, (force - minImpactForce) / (maxImpactForce - minImpactForce));
        weaponSprite.transform.localScale = new Vector3(scale, scale, 1f);

        weaponSprite. GetComponent<WeaponImpact>().SetImpactForce(force);

        if (!isSwinging)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= comboInterval)
                isLeftToRight = !isLeftToRight;
            else
                isLeftToRight = true;

            lastClickTime = Time.time;
            StartCoroutine(SwingWeapon());
        }
    }

    IEnumerator SwingWeapon()
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

        isSwinging = false;
        weaponSprite.SetActive(false);
    }
}