using UnityEngine;
using System.Collections;

public class MoveDownObject : MonoBehaviour
{
    public float speed = 2f;
    public bool useRigidbody = false;
    public bool useLocalSpace = false;

    public bool limitDistance = false;
    public float maxDistance = 5f;

    public bool moveOnlyOnce = true;

    public float delayBeforeMove = 0.0f;

    public Color gizmoLineColor = Color.yellow;
    public Color gizmoEndColor = Color.red;

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private float movedDistance = 0f;

    private bool isMoving = false;
    private bool hasActivated = false;
    private bool isWaitingDelay = false;

    public bool IsMoving => isMoving;
    public bool IsWaitingDelay => isWaitingDelay;

    void Start()
    {
        startPosition = transform.position;

        if (useRigidbody)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isMoving) return;
        if (useRigidbody) return;

        MoveDown();
    }

    void FixedUpdate()
    {
        if (!isMoving) return;
        if (!useRigidbody) return;

        MoveDown();
    }

    public bool Activate()
    {
        if (isMoving || isWaitingDelay) return false;
        if (moveOnlyOnce && hasActivated) return false;

        hasActivated = true;

        if (delayBeforeMove > 0f)
            StartCoroutine(DelayThenMove());
        else
            isMoving = true;

        return true;
    }

    private IEnumerator DelayThenMove()
    {
        isWaitingDelay = true;

        if (useRigidbody && rb != null)
            rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(delayBeforeMove);

        isWaitingDelay = false;
        isMoving = true;
    }

    void MoveDown()
    {
        if (limitDistance && movedDistance >= maxDistance)
        {
            if (useRigidbody && rb != null)
                rb.linearVelocity = Vector2.zero;

            isMoving = false;
            return;
        }

        Vector3 direction = useLocalSpace ? -transform.up : Vector3.down;

        if (useRigidbody && rb != null)
            rb.linearVelocity = (Vector2)(direction * speed);
        else
            transform.position += direction * (speed * Time.deltaTime);

        movedDistance = Vector3.Distance(startPosition, transform.position);
    }

    void OnDrawGizmosSelected()
    {
        if (!limitDistance) return;

        Vector3 start = transform.position;
        Vector3 direction = useLocalSpace ? -transform.up : Vector3.down;
        Vector3 end = start + direction * maxDistance;

        Gizmos.color = gizmoLineColor;
        Gizmos.DrawLine(start, end);

        Gizmos.color = gizmoEndColor;
        Gizmos.DrawCube(end, Vector3.one * 0.3f);
    }
}