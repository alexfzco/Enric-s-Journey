using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 2.0f;

    [Header("Patrulla (opcional)")]
    public Transform[] patrolPoints;
    public float patrolPointReachedThreshold = 0.1f;
    public bool loopPatrol = true;

    private int patrolIndex = 0;
    private int patrolDir = 1;

    [Header("Detección")]
    public float detectionRange = 5f;
    public LayerMask playerLayer;

    [Header("Ataque")]
    public float attackRange = 1.2f;
    public int damage = 10;
    public float attackCooldown = 1.0f;
    public float attackPause = 0.25f;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float nextAttackAllowed = 0f;
    private float pauseUntil = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player != null && !player.gameObject.activeInHierarchy)
            player = null;

        if (player == null)
            DetectPlayer();
    }

    void FixedUpdate()
    {
        // Pausa tras atacar
        if (Time.time < pauseUntil)
        {
            StopMovement();
            return;
        }

        // Si no hay player -> patrulla
        if (player == null)
        {
            PatrolXOnly();
            return;
        }

        // Distancias SOLO EN X
        float distX = Mathf.Abs(rb.position.x - (float)player.position.x);

        // Si se aleja mucho en X -> olvidar y volver a patrulla
        // (no usamos Vector2.Distance para que la Y no fastidie)
        if (distX > detectionRange * 1.3f)
        {
            player = null;
            PatrolXOnly();
            return;
        }

        // En rango de ataque (solo X)
        if (distX <= attackRange)
        {
            StopMovement();
            Face(player.position.x);

            if (Time.time >= nextAttackAllowed)
                Attack();

            return;
        }

        // Persecución SOLO EN X (mantiene la Y del enemigo)
        Vector2 targetPos = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, chaseSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        Face(targetPos.x);
    }

    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit == null) return;

        // Intentamos agarrar el objeto "real" que recibe daño (suele estar en el root del player)
        var dmg = hit.GetComponentInParent<IDamageable>();
        if (dmg != null)
            player = ((MonoBehaviour)dmg).transform;   // el transform del componente que implementa IDamageable
        else
            player = hit.transform.root;               // fallback
    }

    void Attack()
    {
        nextAttackAllowed = Time.time + attackCooldown;
        pauseUntil = Time.time + attackPause;

        if (player == null) return;

        IDamageable dmg = player.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(damage);
    }

    void PatrolXOnly()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            StopMovement();
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        if (target == null)
        {
            StopMovement();
            return;
        }

        Vector2 targetPos = new Vector2(target.position.x, rb.position.y);
        float distX = Mathf.Abs(rb.position.x - targetPos.x);

        if (distX <= patrolPointReachedThreshold)
        {
            AdvancePatrolIndex();
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, patrolSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        Face(targetPos.x);
    }

    void AdvancePatrolIndex()
    {
        if (patrolPoints.Length <= 1) return;

        if (loopPatrol)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
        else
        {
            if (patrolIndex == patrolPoints.Length - 1) patrolDir = -1;
            else if (patrolIndex == 0) patrolDir = 1;

            patrolIndex += patrolDir;
            patrolIndex = Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1);
        }
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void Face(float targetX)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = targetX < transform.position.x;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}