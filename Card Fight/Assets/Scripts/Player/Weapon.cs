using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private Core.PlayerValue playerValue;
    public Transform weaponPivot;
    public GameObject weaponSprite;
    private GameObject Player;
    private GameObject Face;
    private float swingAngle = 180f;
    private float swingDuration = 0.2f;
    private float comboInterval = 2f;

    private bool isSwinging = false;
    private float lastClickTime = -1f;
    private bool isLeftToRight = true;

    private void Start()
    {
        playerValue = FindObjectOfType<Core.PlayerValue>();
        playerValue.OnAttackSpeedChanged += speed => swingDuration = speed;
        weaponSprite = GameObject.FindWithTag("weaponSprite");
        weaponSprite.SetActive(false );
        Face = GameObject.FindWithTag("Face");
        Player = GameObject.FindWithTag("Player");
        weaponPivot.transform.parent = Player.transform;
    }

    public void TrySwing()
    {
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
        float baseAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg - 75f;

        float startZ = baseAngle + (isLeftToRight ? -swingAngle / 2 : swingAngle / 2);
        float endZ = baseAngle + (isLeftToRight ? swingAngle / 2 : -swingAngle / 2);

        while (timer < swingDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / swingDuration);

            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f); 
            float angle = Mathf.Lerp(startZ, endZ, easedT);

            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // 挥完后保持一段时间
        yield return new WaitForSeconds(0.1f); // 停顿时间可调

        isSwinging = false;
        weaponSprite.SetActive(false);
    }
}