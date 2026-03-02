using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 7f;
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Salto")]
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Doble salto")]
    public int maxJumps = 2;
    private int jumpsLeft;

    [Header("Agacharse / Levantarse (al revés)")]
    [Tooltip("Mantén esta tecla para LEVANTARTE. Si la sueltas, vuelves a AGACHARTE.")]
    public KeyCode standKey = KeyCode.S;

    [Tooltip("Collider de pie (EN EL MISMO PLAYER).")]
    public Collider2D standingCollider;

    [Tooltip("Collider agachado (EN EL MISMO PLAYER).")]
    public Collider2D crouchCollider;

    [Tooltip("Collider en un empty hijo (HeadCheck) con IsTrigger, para detectar techo.")]
    public Collider2D headCheckCollider;

    [Tooltip("Capas que cuentan como techo (Ground/Obstacle).")]
    public LayerMask ceilingLayer;

    [Tooltip("Reduce un poco el tamaño del check para evitar falsos positivos.")]
    public float ceilingCheckPadding = 0.02f;

    public bool debugCrouch = false;

    // True = agachado (estado por defecto)
    private bool isCrouching = true;

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashSpeed = 18f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.35f;

    [Header("TP con marcador")]
    public KeyCode tpKey = KeyCode.E;
    public GameObject tpMarkerPrefab;
    public Vector3 markerOffset = Vector3.zero;

    [Header("Ataque")]
    public KeyCode lightAttackKey = KeyCode.J;
    public KeyCode heavyAttackKey = KeyCode.K;

    public LayerMask enemyMask;
    public float lightAttackRadius = 0.8f;
    public float heavyAttackRadius = 1.3f;

    public float lightAttackCooldown = 0.35f;
    public float heavyAttackCooldown = 1.0f;

    public int lightDamage = 25;
    public int heavyDamage = 50;

    private float nextLightTime = 0f;
    private float nextHeavyTime = 0f;

    private Rigidbody2D rb;

    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private float lastDashTime = -999f;

    private int facing = 1;

    // TP
    private bool hasSavedPosition = false;
    private Vector3 savedPosition;
    private GameObject spawnedMarker;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        jumpsLeft = maxJumps;

        // Arranca AGACHADO por defecto
        ForceStartCrouched();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            jumpsLeft = maxJumps;

        if (!isDashing && moveInput != 0)
            facing = moveInput > 0 ? 1 : -1;

        // Agacharse / levantarse al revés
        UpdateReverseCrouch();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetKeyDown(dashKey))
            TryDash();

        if (Input.GetKeyDown(tpKey))
            HandleTeleportToggle();

        if (Input.GetKeyDown(lightAttackKey))
            TryLightAttack();

        if (Input.GetKeyDown(heavyAttackKey))
            TryHeavyAttack();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Si está agachado, va más lento
        float speed = moveSpeed * (isCrouching ? crouchSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    // ---------------- SALTO ----------------
    private void TryJump()
    {
        if (jumpsLeft <= 0) return;

        jumpsLeft--;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ---------------- DASH ----------------
    private void TryDash()
    {
        if (Time.time < lastDashTime + dashCooldown) return;
        if (isDashing) return;

        lastDashTime = Time.time;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    // ---------------- CROUCH REVERSE ----------------
    private void ForceStartCrouched()
    {
        isCrouching = true;

        if (standingCollider != null) standingCollider.enabled = false;
        if (crouchCollider != null) crouchCollider.enabled = true;

        if (debugCrouch)
            Debug.Log("Player: INICIO AGACHADO");
    }

    private void UpdateReverseCrouch()
    {
        bool standHeld = Input.GetKey(standKey);

        if (standHeld)
        {
            // QUIERE levantarse
            if (isCrouching)
            {
                if (!IsCeilingBlocked())
                    SetCrouch(false); // pasar a de pie
                else if (debugCrouch)
                    Debug.Log("No puedo levantarme: hay techo encima.");
            }
        }
        else
        {
            // Si suelta S, vuelve a agacharse
            if (!isCrouching)
                SetCrouch(true);
        }
    }

    private void SetCrouch(bool crouch)
    {
        isCrouching = crouch;

        if (standingCollider != null) standingCollider.enabled = !crouch;
        if (crouchCollider != null) crouchCollider.enabled = crouch;

        if (debugCrouch)
            Debug.Log(crouch ? "Player: AGACHADO" : "Player: DE PIE");
    }

    private bool IsCeilingBlocked()
    {
        if (headCheckCollider == null) return false;
        if (ceilingLayer.value == 0) return false;

        bool prev = headCheckCollider.enabled;
        headCheckCollider.enabled = false;

        Bounds b = headCheckCollider.bounds;

        Vector2 size = new Vector2(
            Mathf.Max(0.01f, b.size.x - ceilingCheckPadding),
            Mathf.Max(0.01f, b.size.y - ceilingCheckPadding)
        );

        bool blocked = Physics2D.OverlapBox(b.center, size, 0f, ceilingLayer) != null;

        headCheckCollider.enabled = prev;
        return blocked;
    }

    // ---------------- TP ----------------
    private void HandleTeleportToggle()
    {
        if (!hasSavedPosition)
        {
            savedPosition = transform.position;
            hasSavedPosition = true;

            if (tpMarkerPrefab != null)
            {
                if (spawnedMarker != null) Destroy(spawnedMarker);
                spawnedMarker = Instantiate(tpMarkerPrefab, savedPosition + markerOffset, Quaternion.identity);
                spawnedMarker.name = "TP_Marker";
            }
            return;
        }

        transform.position = savedPosition;

        if (spawnedMarker != null)
            Destroy(spawnedMarker);

        spawnedMarker = null;
        hasSavedPosition = false;
    }

    // ---------------- ATAQUES ----------------
    private void TryLightAttack()
    {
        if (Time.time < nextLightTime) return;

        nextLightTime = Time.time + lightAttackCooldown;

        Debug.Log("Player: ATAQUE LIGERO");
        DoRadialDamage(lightAttackRadius, lightDamage);
    }

    private void TryHeavyAttack()
    {
        if (Time.time < nextHeavyTime) return;

        nextHeavyTime = Time.time + heavyAttackCooldown;

        Debug.Log("Player: ATAQUE PESADO");
        DoRadialDamage(heavyAttackRadius, heavyDamage);
    }

    private void DoRadialDamage(float radius, int damage)
    {
        Vector2 center = transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyMask);
        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable dmg =
                hits[i].GetComponent<IDamageable>() ??
                hits[i].GetComponentInParent<IDamageable>();

            if (dmg != null)
                dmg.TakeDamage(damage);
        }
    }

    // ---------------- GIZMOS ----------------
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (headCheckCollider != null)
        {
            Bounds b = headCheckCollider.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        Gizmos.DrawWireSphere(transform.position, lightAttackRadius);
        Gizmos.DrawWireSphere(transform.position, heavyAttackRadius);
    }
}