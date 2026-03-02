using System;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolAttack2D : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform player;
    public MonoBehaviour enemyHealth; // Arrastra aquí tu EnemyHealth (opcional: lo intenta detectar)

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float arriveDistanceX = 0.15f;
    public float waitAtPointSeconds = 0.25f;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float detectRange = 6f;
    public float loseRange = 8.5f;

    [Header("Attack")]
    public float attackRange = 1.1f;
    public float attackCooldown = 1.2f;
    public int attackDamage = 1;

    [Header("Flip (anti-vibración)")]
    [Tooltip("Si la diferencia en X con el objetivo es menor que esto, NO cambia la orientación.")]
    public float flipDeadzoneX = 0.08f;

    [Header("Debug")]
    public bool debugLogs = false;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;

    private int patrolIndex = 0;
    private int lastLoggedPatrolIndex = -1;

    private float waitTimer = 0f;
    private float nextAttackTime = 0f;

    // Facing fijo para evitar flip spam
    private int facingSign = 1; // 1 derecha, -1 izquierda

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (enemyHealth == null)
            enemyHealth = GetComponent("EnemyHealth") as MonoBehaviour;

        // Inicializa facing según escala actual
        if (transform.localScale.x < 0f) facingSign = -1;
        else facingSign = 1;
    }

    void FixedUpdate()
    {
        if (IsDeadFromEnemyHealth())
        {
            StopHorizontal();
            return;
        }

        if (player == null)
        {
            DoPatrol();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Transiciones de estado con rangos claros
        switch (state)
        {
            case State.Patrol:
                if (distToPlayer <= detectRange) state = State.Chase;
                break;

            case State.Chase:
                if (distToPlayer > loseRange) state = State.Patrol;
                else if (distToPlayer <= attackRange) state = State.Attack;
                break;

            case State.Attack:
                if (distToPlayer > attackRange) state = State.Chase;
                break;
        }

        // Ejecutar estado
        if (state == State.Patrol) DoPatrol();
        else if (state == State.Chase) DoChase();
        else DoAttack();
    }

    // ----------------------------
    // PATROL
    // ----------------------------
    private void DoPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            StopHorizontal();
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            StopHorizontal();
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        if (target == null)
        {
            StopHorizontal();
            AdvancePatrolIndex();
            return;
        }

        // Debug solo cuando cambia el objetivo
        if (debugLogs && patrolIndex != lastLoggedPatrolIndex)
        {
            Debug.Log($"[{name}] Yendo a punto {patrolIndex}: {target.name} (pos {target.position})");
            lastLoggedPatrolIndex = patrolIndex;
        }

        float dx = target.position.x - transform.position.x;

        // Orientación (con deadzone anti-vibración)
        FaceToDx(dx);

        // Llegada por X
        if (Mathf.Abs(dx) <= arriveDistanceX)
        {
            StopHorizontal();
            AdvancePatrolIndex();
            waitTimer = waitAtPointSeconds;
            return;
        }

        // Movimiento estable: si dx es muy pequeño, mejor parar
        if (Mathf.Abs(dx) < flipDeadzoneX)
        {
            StopHorizontal();
            return;
        }

        float dir = dx > 0f ? 1f : -1f;
        MoveHorizontal(dir, patrolSpeed);
    }

    private void AdvancePatrolIndex()
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        lastLoggedPatrolIndex = -1; // fuerza log del siguiente
    }

    // ----------------------------
    // CHASE
    // ----------------------------
    private void DoChase()
    {
        float dx = player.position.x - transform.position.x;

        // Orientación estable
        FaceToDx(dx);

        // Evita micro oscilaciones cuando casi está alineado
        if (Mathf.Abs(dx) <= arriveDistanceX)
        {
            StopHorizontal();
            return;
        }

        // Si está dentro de la deadzone del flip, no metas empujoncitos
        if (Mathf.Abs(dx) < flipDeadzoneX)
        {
            StopHorizontal();
            return;
        }

        float dir = dx > 0f ? 1f : -1f;
        MoveHorizontal(dir, chaseSpeed);
    }

    // ----------------------------
    // ATTACK
    // ----------------------------
    private void DoAttack()
    {
        StopHorizontal();

        float dx = player.position.x - transform.position.x;
        FaceToDx(dx);

        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (debugLogs)
            Debug.Log($"[{name}] ATTACK (cooldown {attackCooldown}s) dmg={attackDamage}");

        // Intento: si el Player tiene TakeDamage(int), se llamará.
        // Si no existe, no pasa nada.
        player.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
    }

    // ----------------------------
    // MOVIMIENTO
    // ----------------------------
    private void MoveHorizontal(float dir, float speed)
    {
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    private void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // Flip anti-vibración
    private void FaceToDx(float dx)
    {
        // No cambies orientación si dx es muy pequeño
        if (Mathf.Abs(dx) < flipDeadzoneX) return;

        facingSign = dx > 0f ? 1 : -1;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facingSign;
        transform.localScale = s;
    }

    // ----------------------------
    // ENEMY HEALTH (sin depender de tu clase exacta)
    // ----------------------------
    private bool IsDeadFromEnemyHealth()
    {
        if (enemyHealth == null) return false;

        object comp = enemyHealth;
        Type t = comp.GetType();

        // bool IsDead / isDead / dead
        if (TryReadBool(t, comp, "IsDead", out bool b)) return b;
        if (TryReadBool(t, comp, "isDead", out b)) return b;
        if (TryReadBool(t, comp, "dead", out b)) return b;

        // números currentHealth/health/etc. <= 0
        if (TryReadNumber(t, comp, "currentHealth", out float hp)) return hp <= 0f;
        if (TryReadNumber(t, comp, "CurrentHealth", out hp)) return hp <= 0f;
        if (TryReadNumber(t, comp, "health", out hp)) return hp <= 0f;
        if (TryReadNumber(t, comp, "Health", out hp)) return hp <= 0f;

        // método IsDead()
        MethodInfo m = t.GetMethod("IsDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m != null && m.ReturnType == typeof(bool) && m.GetParameters().Length == 0)
        {
            try { return (bool)m.Invoke(comp, null); } catch { }
        }

        return false;
    }

    private bool TryReadBool(Type t, object comp, string name, out bool value)
    {
        value = false;

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(bool))
        {
            try { value = (bool)p.GetValue(comp); return true; } catch { return false; }
        }

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool))
        {
            try { value = (bool)f.GetValue(comp); return true; } catch { return false; }
        }

        return false;
    }

    private bool TryReadNumber(Type t, object comp, string name, out float value)
    {
        value = 0f;

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null)
        {
            try
            {
                object v = p.GetValue(comp);
                if (v == null) return false;
                value = Convert.ToSingle(v);
                return true;
            }
            catch { }
        }

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            try
            {
                object v = f.GetValue(comp);
                if (v == null) return false;
                value = Convert.ToSingle(v);
                return true;
            }
            catch { }
        }

        return false;
    }

    // ----------------------------
    // GIZMOS
    // ----------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.DrawWireSphere(transform.position, loseRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}