using UnityEngine;

public class MoveDownObject : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2f;                 // Velocidad de bajada
    public bool useRigidbody = false;        // Si usa Rigidbody2D o Transform
    public bool useLocalSpace = false;       // Baja en espacio local o global

    [Header("Limitaci�n")]
    public bool limitDistance = false;       // �Tiene l�mite de bajada?
    public float maxDistance = 5f;           // Distancia m�xima que baja

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private float movedDistance = 0f;

    void Start()
    {
        startPosition = transform.position;

        if (useRigidbody)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (useRigidbody) return;

        MoveDown();
    }

    void FixedUpdate()
    {
        if (!useRigidbody) return;

        MoveDown();
    }

    void MoveDown()
    {
        if (limitDistance && movedDistance >= maxDistance)
        {
            if (useRigidbody && rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 direction = useLocalSpace ? -transform.up : Vector3.down;
        float movement = speed * Time.deltaTime;

        if (useRigidbody && rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.position += direction * movement;
        }

        movedDistance = Vector3.Distance(startPosition, transform.position);
    }
}