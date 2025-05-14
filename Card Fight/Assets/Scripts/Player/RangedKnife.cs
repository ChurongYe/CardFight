using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class RangedKnife : MonoBehaviour
{
    private float speedFactor = 1.0f;
    private float baseMaxSpeed = 30f;
    private float baseAcceleration = 80f;
    private float baseDeceleration = 80f;

    private float maxSpeed = 30f;
    private float acceleration = 80f;
    private float deceleration = 80f;
    private float maxDistance = 10f;
    private float returnSpeed = 60f;
    private float rotationSpeed = 720f; // ��ת�ٶ�
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
    public float SpeedFactor
    {
        get { return speedFactor; }
        set { speedFactor = value;
            }
    }
    public float MaxDistance
    {
        get { return maxDistance; }
        set
        {
            maxDistance = value;
        }
    }
    void ApplySpeedFactor()
    {
        maxSpeed = baseMaxSpeed * speedFactor;
        acceleration = baseAcceleration * speedFactor;
        deceleration = baseDeceleration * speedFactor;
    }
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
        ApplySpeedFactor();
        if (!launched) return;

        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        if (!isReturning)
        {
            float remainingDistance = maxDistance - traveledDistance;

            if (remainingDistance > 0f)
            {
                float brakingDistance = (currentSpeed * currentSpeed) / (2 * deceleration);

                if (brakingDistance >= remainingDistance)
                {
                    // ��ʼ����
                    currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
                }
                else
                {
                    // ��������
                    currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
                }

                Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
                transform.position += movement;
                traveledDistance += movement.magnitude;
            }

            // �ﵽ�����벢ͣ��
            if (remainingDistance <= 0f && !isStopping)
            {
                isStopping = true;
                currentSpeed = 0f;
                StartCoroutine(StopAndReturn()); // ͣ���ٷ���
            }
        }
        else if (playerTransform != null)
        {
            // �������
            Vector3 returnDir = (playerTransform.position - transform.position).normalized;
            transform.position += returnDir * returnSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, playerTransform.position) < 0.1f)
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                gameObject.GetComponent<Collider2D>().enabled = false;
                StartCoroutine(StopAndShack());
            }
        }
    }

    IEnumerator StopAndReturn()
    {
        // ͣ��
        yield return new WaitForSeconds(0.2f);
        isReturning = true;
    }
    IEnumerator StopAndShack()
    {
        playerController.CantMove(0.2f);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        // ������Ļ����Ч��
        if (cameraShake != null)
        {
            cameraShake.TriggerLightShake();
        }
        // ͣ��
        yield return new WaitForSeconds(0.2f);
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
