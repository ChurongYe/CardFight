using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && !player.IsInvincible())
            {
                player.TakeDamage(damage);
            }
        }
        else if (collision.CompareTag("Summon"))
        {
            Summon summon = collision.GetComponent<Summon >();
            if (summon != null && !summon.IsInvincible())
            {
                summon.TakeDamage(damage);
            }
        }
    }
}
