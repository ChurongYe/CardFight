//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyHurt : MonoBehaviour
//{
//    public float impactForce = 10f;
//    public int maxHP = 3;
//    public float hurtCooldown = 0.2f;
//    public SpriteRenderer spriteRenderer;
//    public Color flashColor = Color.white;
//    public float flashTime = 0.1f;

//    private int currentHP;
//    private bool canBeHurt = true;
//    private Rigidbody2D rb;
//    private Shibie shibie;
//   // private Animator animator;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        shibie = GetComponent<Shibie>();
//        //animator = GetComponent<Animator>();
//        currentHP = maxHP;
//    }

//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (!canBeHurt || shibie.IsAttacking) return;

//        if (collision.CompareTag("weaponSprite"))
//        {
//            Vector2 direction = (transform.position - collision.transform.position).normalized;
//            rb.AddForce(direction * impactForce, ForceMode2D.Impulse);

//            TakeDamage(1);
//        }
//    }

//    private void TakeDamage(int damage)
//    {
//        currentHP -= damage;
//        canBeHurt = false;

//        StartCoroutine(HurtFlash());
//        //animator.SetTrigger("Hurt");

//        if (currentHP <= 0)
//        {
//            StartCoroutine(Die());
//        }
//        else
//        {
//            StartCoroutine(ResetHurtCooldown());
//        }
//    }

//    private IEnumerator HurtFlash()
//    {
//        if (spriteRenderer != null)
//        {
//            Color original = spriteRenderer.color;
//            spriteRenderer.color = flashColor;
//            yield return new WaitForSeconds(flashTime);
//            spriteRenderer.color = original;
//        }
//    }

//    private IEnumerator ResetHurtCooldown()
//    {
//        yield return new WaitForSeconds(hurtCooldown);
//        canBeHurt = true;
//    }

//    private IEnumerator Die()
//    {
//        shibie.StopMoving();
//        //animator.SetTrigger("Die");

//        yield return new WaitForSeconds(0.5f);
//        Destroy(gameObject);
//    }
//}
