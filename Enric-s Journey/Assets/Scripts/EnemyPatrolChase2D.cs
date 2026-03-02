using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolChasePlatformer2D : MonoBehaviour
{
    [Header("Referencias")]
    public Transform[] patrolPoints;
    public Transform player; // si vacío: busca por tag "Player"

    [Header("Movimiento")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float arriveDistanceX = 0.15f;
    public bool flipByScale = true;

    [Header("Ground Check (para no hacer cosas raras en el aire)")]
    public Transform groundCheck;              // empty a los pies
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Visión")]
    public float viewDistance = 6f;
    public float viewRadius = 1.0f;
    public LayerMask obstacleMask;             // paredes que bloquean visión
    public LayerMask playerMask;               // layer del player

    [Header("Persecución")]
    public float loseSightSeconds = 2f;

    private Rigidbody2D rb;
    private int patrolIndex = 0;

    private enum State { Patrol, Chase }
    private State state = State.Patrol;

    private float lastTimeSawPlayer = -999f;
    private int facing = 1;

    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // IMPORTANTE: no forzamos gravityScale aquí.
        // Déjalo configurado en el Inspector para que caiga.

        rb.freezeRotation = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = true; // si no hay groundCheck, asumimos true
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            PatrolMove();
            return;
        }

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            lastTimeSawPlayer = Time.time;
            state = State.Chase;
        }
        else
        {
            if (state == State.Chase && Time.time > lastTimeSawPlayer + loseSightSeconds)
            {
                state = State.Patrol;
                patrolIndex = GetNearestPatrolIndex();
            }
        }

        if (state == State.Chase) ChaseMove();
        else PatrolMove();
    }

    // ------------------ MOVIMIENTO ------------------

    private void PatrolMove()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetVelX(0f);
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        float dx = target.position.x - transform.position.x;

        // Llegada por X
        if (Mathf.Abs(dx) <= arriveDistanceX)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            SetVelX(0f);
            return;
        }

        float dirX = Mathf.Sign(dx);
        MoveX(dirX, patrolSpeed);
    }

    private void ChaseMove()
    {
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= arriveDistanceX)
        {
            SetVelX(0f);
            return;
        }

        float dirX = Mathf.Sign(dx);
        MoveX(dirX, chaseSpeed);
    }

    private void MoveX(float dirX, float speed)
    {
        // En plataformas, dejamos que la Y la controle la gravedad:
        // solo tocamos la X.
        SetVelX(dirX * speed);

        if (Mathf.Abs(dirX) > 0.01f)
        {
            facing = dirX > 0 ? 1 : -1;
            if (flipByScale)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * facing;
                transform.localScale = s;
            }
        }
    }

    private void SetVelX(float vx)
    {
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }
    private bool CanSeePlayer()
    {
        Vector2 origin = rb.position;

        Vector2 toPlayer = (Vector2)player.position - origin;
        float dist = toPlayer.magnitude;

        if (dist > viewDistance) return false;

        // Si está cerca, radio
        bool inRadius = Physics2D.OverlapCircle(origin, viewRadius, playerMask) != null;

        // Línea de visión
        Vector2 dir = toPlayer.normalized;

        // Si hay obstáculo antes, no lo ve
        RaycastHit2D hitObstacle = Physics2D.Raycast(origin, dir, dist, obstacleMask);
        if (hitObstacle.collider != null) return false;

        RaycastHit2D hitPlayer = Physics2D.Raycast(origin, dir, dist, playerMask);
        if (hitPlayer.collider != null) return true;

        return inRadius;
    }

    private int GetNearestPatrolIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return 0;

        int best = 0;
        float bestD = float.PositiveInfinity;
        Vector2 pos = transform.position;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            float d = Mathf.Abs(pos.x - patrolPoints[i].position.x); // en plataformas, por X va bien
            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }
        return best;
    }

    // ------------------ GIZMOS ------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.12f);
            }
        }
    }
}