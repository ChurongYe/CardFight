using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponImpact : MonoBehaviour
{
    private GameObject Player;
    private float impactForce = 0f;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");
        impactForce = 10f;
    }

    public void SetImpactForce(float force)
    {
        impactForce = force;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Rigidbody2D rb = collision.attachedRigidbody;
            if (rb != null)
            {
                Vector2 direction = (collision.transform.position - Player.transform .position).normalized;
                Vector2 impactDirection = direction; 

                rb.AddForce(impactDirection * impactForce, ForceMode2D.Impulse);
            }
        }
    }
}
