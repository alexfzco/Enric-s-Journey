using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MovingPlatform2D : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private float speed;
    private bool isWaiting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
    }

    void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            enabled = false;
            return;
        }

        transform.position = startPoint.position;
        targetPosition = endPoint.position;
        CalculateSpeed();
    }

    void FixedUpdate()
    {
        if (isWaiting) return;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPosition) < 0.02f)
        {
            StartCoroutine(WaitAndSwitchTarget());
        }
    }

    private void CalculateSpeed()
    {
        float distance = Vector2.Distance(startPoint.position, endPoint.position);
        speed = distance / Mathf.Max(0.001f, duration);
    }

    private IEnumerator WaitAndSwitchTarget()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(waitTime);

        targetPosition = (targetPosition == (Vector2)endPoint.position) ? startPoint.position : endPoint.position;

        isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(null);
        }
    }
}