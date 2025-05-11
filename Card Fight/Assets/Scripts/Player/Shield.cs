using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Shield : MonoBehaviour
{
    private GameObject Face;

    private void Start()
    {
        Face = GameObject.FindWithTag("Face");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector2 attackDirection = other.transform.position - transform.position;

            Vector2 faceDirection = Face.transform.right;

            if (IsAttackBlocked(attackDirection, faceDirection))
            {
                Debug.Log("攻击被盾牌挡住！");
            }
            else
            {
                Debug.Log("攻击命中背部！");
            }
        }
    }

    // 判断攻击是否被盾牌挡住
    private bool IsAttackBlocked(Vector2 attackDirection, Vector2 faceDirection)
    {
        float angle = Vector2.Angle(attackDirection, faceDirection);

        float tolerance = 35f;
        return Mathf.Abs(angle ) <= tolerance;
    }
}

