using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class RangedKnife : MonoBehaviour
{
    //    private float speedFactor = 1.0f;
    //    private float baseMaxSpeed = 30f;
    //    private float baseAcceleration = 80f;
    //    private float baseDeceleration = 80f;

    //    private float maxSpeed = 30f;
    //    private float acceleration = 80f;
    //    private float deceleration = 80f;
    //    private float maxDistance = 10f;
    //    private float returnSpeed = 60f;
    //    private float rotationSpeed = 720f; // 旋转速度
    //    private Vector3 startPosition;
    //    private Vector3 targetDirection;
    //    private float currentSpeed = 0f;
    //    private float traveledDistance = 0f;

    //    private bool isReturning = false;
    //    private Transform playerTransform;
    //    private bool launched = false;
    //    private bool isStopping = false;
    //    private CameraShake cameraShake;
    //    private PlayerController playerController;
    //    public float SpeedFactor
    //    {
    //        get { return speedFactor; }
    //        set { speedFactor = value;
    //            }
    //    }
    //    public float MaxDistance
    //    {
    //        get { return maxDistance; }
    //        set
    //        {
    //            maxDistance = value;
    //        }
    //    }
    //    void ApplySpeedFactor()
    //    {
    //        maxSpeed = baseMaxSpeed * speedFactor;
    //        acceleration = baseAcceleration * speedFactor;
    //        deceleration = baseDeceleration * speedFactor;
    //    }
    //    public void Launch(Vector2 direction, Transform player)
    //    {
    //        startPosition = transform.position;
    //        targetDirection = direction.normalized;
    //        playerTransform = player;
    //        launched = true;
    //        cameraShake = FindObjectOfType<CameraShake>();
    //        playerController = FindObjectOfType<PlayerController>();
    //    }

    //    void Update()
    //    {
    //        ApplySpeedFactor();
    //        if (!launched) return;

    //        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

    //        if (!isReturning)
    //        {
    //            float remainingDistance = maxDistance - traveledDistance;

    //            if (remainingDistance > 0f)
    //            {
    //                float brakingDistance = (currentSpeed * currentSpeed) / (2 * deceleration);

    //                if (brakingDistance >= remainingDistance)
    //                {
    //                    // 开始减速
    //                    currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
    //                }
    //                else
    //                {
    //                    // 继续加速
    //                    currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    //                }

    //                Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
    //                transform.position += movement;
    //                traveledDistance += movement.magnitude;
    //            }

    //            // 达到最大距离并停下
    //            if (remainingDistance <= 0f && !isStopping)
    //            {
    //                isStopping = true;
    //                currentSpeed = 0f;
    //                StartCoroutine(StopAndReturn()); // 停留再返回
    //            }
    //        }
    //        else if (playerTransform != null)
    //        {
    //            // 返回玩家
    //            Vector3 returnDir = (playerTransform.position - transform.position).normalized;
    //            transform.position += returnDir * returnSpeed * Time.deltaTime;

    //            if (Vector3.Distance(transform.position, playerTransform.position) < 0.1f)
    //            {
    //                gameObject.GetComponent<SpriteRenderer>().enabled = false;
    //                gameObject.GetComponent<Collider2D>().enabled = false;
    //                StartCoroutine(StopAndShack());
    //            }
    //        }
    //    }

    //    IEnumerator StopAndReturn()
    //    {
    //        // 停顿
    //        yield return new WaitForSeconds(0.2f);
    //        isReturning = true;
    //    }
    //    IEnumerator StopAndShack()
    //    {
    //        playerController.CantMove(0.2f);
    //        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    //        // 触发屏幕抖动效果
    //        if (cameraShake != null)
    //        {
    //            cameraShake.TriggerLightShake();
    //        }
    //        // 停顿
    //        yield return new WaitForSeconds(0.2f);
    //        playerController.CanAttack = true;
    //        Destroy(gameObject);
    //    }
    //}
    public float baseSpeed = 10f;
    public float baseAcceleration = 30f;
    public float speedFactor = 1f;

    private float speed;
    private float acceleration;
    private float maxDistance = 200f;
    private float lifeAfterMaxDistance = 0.2f; // 超出距离后存活时间

    private Vector3 startPosition;
    private Vector3 targetDirection;
    private float currentSpeed = 0f;
    private float traveledDistance = 0f;
    private bool launched = false;
    private bool isDead = false;

    [Header("Lighting")]
    public GameObject LightingPrefab;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void SpeedFactor()
    {
        speed = baseSpeed * speedFactor;
        acceleration = baseAcceleration * speedFactor;
    }
    public void Launch(Vector2 direction)
    {
        startPosition = transform.position;
        targetDirection = direction.normalized;
        launched = true;
    }

    void Update()
    {
        SpeedFactor();
        if (!launched || isDead) return;

        // 加速飞行
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, speed);
        Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
        transform.position += movement;
        traveledDistance += movement.magnitude;

        // 超出最大距离后销毁
        if (traveledDistance >= maxDistance)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }
    bool IsVisibleToCamera(Transform target)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(target.position);
        return viewportPoint.x >= 0.1f && viewportPoint.x <= 0.9f &&
               viewportPoint.y >= 0.1f && viewportPoint.y <= 0.9f &&
               viewportPoint.z > 0;
    }
    IEnumerator DestroyAfterDelay()
    {
        isDead = true;
        yield return new WaitForSeconds(lifeAfterMaxDistance);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Enemy") || other.CompareTag("Obstacle")) &&
         IsVisibleToCamera(other.transform))
        {
            launched = false; // 停止飞行
            isDead = true;
            GetComponent<Collider2D>().enabled = false;
            // 触发击中动画
            if (animator != null)
                animator.SetTrigger("Hit");

            if (CardValue.AddLighting && LightingPrefab != null)
            {
                GameObject lightZone = Instantiate(LightingPrefab, transform.position, Quaternion.identity);
            }

            StartCoroutine(StopAfterAnimation());
        }
    }
    IEnumerator StopAfterAnimation()
    {
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        Destroy(gameObject);
    }
}
