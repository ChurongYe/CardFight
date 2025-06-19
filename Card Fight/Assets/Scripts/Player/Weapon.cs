using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject Up;
    [SerializeField] private GameObject Down;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;
    private Collider2D hitboxUp;
    private Collider2D hitboxDown;
    private Collider2D hitboxLeft;
    private Collider2D hitboxRight;

    private void Start()
    {
        hitboxUp = Up.GetComponent<Collider2D>();
        hitboxDown = Down.GetComponent<Collider2D>();
        hitboxLeft = Left.GetComponent<Collider2D>();
        hitboxRight = Right.GetComponent<Collider2D>();
        StartCoroutine(DisableAllHitboxes());
    }
    // 动画事件：关闭全部
    IEnumerator DisableAllHitboxes()
    {
        yield return null;
        hitboxUp.enabled = false;
        hitboxDown.enabled = false;
        hitboxLeft.enabled = false;
        hitboxRight.enabled = false;
    }

    // 动画事件：开启当前方向碰撞体
    public void EnableDirectionHitbox(int direction)
    {

        DisableAllHitboxesImmediate();

        switch ((AttackDirection)direction)
        {
            case AttackDirection.Up:
                hitboxUp.enabled = true;
                Debug.Log("启用 hitboxUp，状态：" + hitboxUp.enabled);
                break;

            case AttackDirection.Down:
                hitboxDown.enabled = true;
                Debug.Log("启用 hitboxDown，状态：" + hitboxDown.enabled);
                break;

            case AttackDirection.Left:
                hitboxLeft.enabled = true;
                Debug.Log("启用 hitboxLeft，状态：" + hitboxLeft.enabled);
                break;

            case AttackDirection.Right:
                hitboxRight.enabled = true;
                Debug.Log("启用 hitboxRight，状态：" + hitboxRight.enabled);
                break;

            default:
                Debug.LogWarning("未知方向：" + direction);
                break;
        }
    }
    public void DisableAllHitboxesImmediate()
    {
        hitboxUp.enabled = false;
        hitboxDown.enabled = false;
        hitboxLeft.enabled = false;
        hitboxRight.enabled = false;
    }


}