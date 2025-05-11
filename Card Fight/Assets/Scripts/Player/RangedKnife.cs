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
    public float rotationSpeed = 720f; // ��ת�ٶ�
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
            // �������
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
        // ͣ��
        yield return new WaitForSeconds(0.5f);
        isReturning = true;
    }
    IEnumerator StopAndShack()
    {
        playerController.CanMove = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        // ������Ļ����Ч��
        if (cameraShake != null)
        {
            cameraShake.TriggerLightShake();
        }
        // ͣ��
        yield return new WaitForSeconds(0.2f);
        playerController.CanMove = true;
        playerController.CanAttack = true;
        Destroy(gameObject);
    }
}
//public float speed = 10f;
//public float acceleration = 30f;
//public float maxDistance = 10f;
//public float lifeAfterMaxDistance = 0.2f; // �����������ʱ��

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

//    // ���ٷ���
//    currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, speed);
//    Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
//    transform.position += movement;
//    traveledDistance += movement.magnitude;

//    // ���������������
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
//    // �������ú���������������˻����
//    if (other.CompareTag("Enemy") || other.CompareTag("Obstacle"))
//    {
//        Destroy(gameObject);
//    }
//}
