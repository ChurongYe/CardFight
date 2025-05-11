using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class RangedKnife : MonoBehaviour
{
    public float maxSpeed = 15f;
    public float acceleration = 60f;
    public float deceleration = 80f;
    public float maxDistance = 10f;
    public float returnSpeed = 20f;
    public float rotationSpeed = 720f; // 旋转速度
    private Vector3 startPosition;
    private Vector3 targetDirection;
    private float currentSpeed = 0f;
    private float traveledDistance = 0f;

    private bool isReturning = false;
    private Transform playerTransform;
    private bool launched = false;
    private bool isStopping = false;
    private CameraShake cameraShake;
    private PlayerController playerController;

    public void Launch(Vector2 direction, Transform player)
    {
        startPosition = transform.position;
        targetDirection = direction.normalized;
        playerTransform = player;
        launched = true;
        cameraShake = FindObjectOfType<CameraShake>();
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (!launched) return;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        if (!isReturning)
        {
            float distancePercent = traveledDistance / maxDistance;

            if (distancePercent < 0.8f)
            {
                currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            }
            else
            {
                currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0.2f);
            }

            Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
            transform.position += movement;
            traveledDistance += movement.magnitude;

            if (traveledDistance >= maxDistance && !isStopping)
            {
                isStopping = true;
                StartCoroutine(StopAndReturn());
            }
        }
        else if (playerTransform != null)
        {
            // 返回玩家
            Vector3 returnDir = (playerTransform.position - transform.position).normalized;
            transform.position += returnDir * returnSpeed * Time.deltaTime;
            
            if (Vector3.Distance(transform.position, playerTransform.position) < 0.1f)
            {
                StartCoroutine(StopAndShack());
            }
        }
    }

    IEnumerator StopAndReturn()
    {
        // 停顿
        yield return new WaitForSeconds(0.5f);
        isReturning = true;
    }
    IEnumerator StopAndShack()
    {
        playerController.CanMove = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        // 触发屏幕抖动效果
        if (cameraShake != null)
        {
            cameraShake.TriggerLightShake();
        }
        // 停顿
        yield return new WaitForSeconds(0.2f);
        playerController.CanMove = true;
        playerController.CanAttack = true;
        Destroy(gameObject);
    }
}
//public float speed = 10f;
//public float acceleration = 30f;
//public float maxDistance = 10f;
//public float lifeAfterMaxDistance = 0.2f; // 超出距离后存活时间

//private Vector3 startPosition;
//private Vector3 targetDirection;
//private float currentSpeed = 0f;
//private float traveledDistance = 0f;
//private bool launched = false;
//private bool isDead = false;

//public void Launch(Vector2 direction)
//{
//    startPosition = transform.position;
//    targetDirection = direction.normalized;
//    launched = true;
//}

//void Update()
//{
//    if (!launched || isDead) return;

//    // 加速飞行
//    currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, speed);
//    Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
//    transform.position += movement;
//    traveledDistance += movement.magnitude;

//    // 超出最大距离后销毁
//    if (traveledDistance >= maxDistance)
//    {
//        StartCoroutine(DestroyAfterDelay());
//    }
//}

//IEnumerator DestroyAfterDelay()
//{
//    isDead = true;
//    yield return new WaitForSeconds(lifeAfterMaxDistance);
//    Destroy(gameObject);
//}

//void OnTriggerEnter2D(Collider2D other)
//{
//    // 可以设置忽略玩家自身，检测敌人或地形
//    if (other.CompareTag("Enemy") || other.CompareTag("Obstacle"))
//    {
//        Destroy(gameObject);
//    }
//}
