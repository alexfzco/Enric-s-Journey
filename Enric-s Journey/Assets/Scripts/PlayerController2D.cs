using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float invulnerabilitySeconds = 0.25f;

    public float moveSpeed = 7f;
    public float crouchSpeedMultiplier = 0.5f;

    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    public int maxJumps = 2;

    public KeyCode standKey = KeyCode.S;
    public Collider2D standingCollider;
    public Collider2D crouchCollider;
    public Collider2D headCheckCollider;
    public LayerMask ceilingLayer;
    public float ceilingCheckPadding = 0.02f;

    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashSpeed = 18f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.35f;

    public KeyCode tpKey = KeyCode.E;
    public GameObject tpMarkerPrefab;
    public Vector3 markerOffset = Vector3.zero;

    public KeyCode lightAttackKey = KeyCode.J;
    public KeyCode heavyAttackKey = KeyCode.K;

    public LayerMask enemyMask;
    public int lightDamage = 25;
    public int heavyDamage = 50;
    public float lightAttackCooldown = 0.35f;
    public float heavyAttackCooldown = 1.0f;

    public float lightHitRadius = 0.9f;
    public float heavyAttackRadius = 1.3f;

    public Transform sword;
    public bool hideSwordWhenIdle = true;

    public float lightSwordDistance = 0.7f;
    public float lightSwordOutTime = 0.06f;
    public float lightSwordBackTime = 0.08f;

    public float heavySwordDistance = 1.2f;
    public float heavySwordSweepTime = 0.14f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private int jumpsLeft;

    private bool isCrouching = true;

    private bool isDashing;
    private float lastDashTime = -999f;

    private float nextLightTime = 0f;
    private float nextHeavyTime = 0f;

    private float invulnUntil = 0f;

    private int facing = 1;

    private bool hasSavedPosition = false;
    private Vector3 savedPosition;
    private GameObject spawnedMarker;

    private bool isAttacking = false;
    private Vector3 swordRestLocalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0) currentHealth = maxHealth;

        jumpsLeft = maxJumps;

        ForceStartCrouched();

        if (sword != null)
        {
            swordRestLocalPos = sword.localPosition;
            if (hideSwordWhenIdle) sword.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            jumpsLeft = maxJumps;

        if (!isDashing && Mathf.Abs(moveInput) > 0.01f)
            facing = moveInput > 0 ? 1 : -1;

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
        if (!isDashing)
        {
            float speed = moveSpeed * (isCrouching ? crouchSpeedMultiplier : 1f);
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (Time.time < invulnUntil) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player recibió " + amount + " de daño. Vida: " + currentHealth + "/" + maxHealth);

        invulnUntil = Time.time + Mathf.Max(0f, invulnerabilitySeconds);

        if (currentHealth <= 0)
            gameObject.SetActive(false);
    }

    private void TryJump()
    {
        if (jumpsLeft <= 0) return;

        jumpsLeft--;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

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

    private void ForceStartCrouched()
    {
        isCrouching = true;

        if (standingCollider != null) standingCollider.enabled = false;
        if (crouchCollider != null) crouchCollider.enabled = true;
    }

    private void UpdateReverseCrouch()
    {
        bool standHeld = Input.GetKey(standKey);

        if (standHeld)
        {
            if (isCrouching && !IsCeilingBlocked())
                SetCrouch(false);
        }
        else
        {
            if (!isCrouching)
                SetCrouch(true);
        }
    }

    private void SetCrouch(bool crouch)
    {
        isCrouching = crouch;

        if (standingCollider != null) standingCollider.enabled = !crouch;
        if (crouchCollider != null) crouchCollider.enabled = crouch;
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
            }
            return;
        }

        transform.position = savedPosition;

        if (spawnedMarker != null)
            Destroy(spawnedMarker);

        spawnedMarker = null;
        hasSavedPosition = false;
    }

    private void TryLightAttack()
    {
        if (Time.time < nextLightTime) return;
        if (isAttacking) return;

        nextLightTime = Time.time + lightAttackCooldown;

        Debug.Log("ATAQUE LIGERO");
        StartCoroutine(LightAttackRoutine());
    }

    private void TryHeavyAttack()
    {
        if (Time.time < nextHeavyTime) return;
        if (isAttacking) return;

        nextHeavyTime = Time.time + heavyAttackCooldown;

        Debug.Log("ATAQUE PESADO (AOE)");
        StartCoroutine(HeavyAttackRoutine());
    }

    private IEnumerator LightAttackRoutine()
    {
        isAttacking = true;

        if (sword != null)
        {
            if (hideSwordWhenIdle) sword.gameObject.SetActive(true);

            Vector3 start = swordRestLocalPos;
            Vector3 target = swordRestLocalPos + new Vector3(facing * lightSwordDistance, 0f, 0f);

            float t = 0f;
            float outDur = Mathf.Max(0.001f, lightSwordOutTime);
            while (t < 1f)
            {
                t += Time.deltaTime / outDur;
                sword.localPosition = Vector3.Lerp(start, target, t);
                yield return null;
            }

            DoRadialDamage((Vector2)transform.position, lightHitRadius, lightDamage);

            t = 0f;
            float backDur = Mathf.Max(0.001f, lightSwordBackTime);
            while (t < 1f)
            {
                t += Time.deltaTime / backDur;
                sword.localPosition = Vector3.Lerp(target, start, t);
                yield return null;
            }

            sword.localPosition = start;
            if (hideSwordWhenIdle) sword.gameObject.SetActive(false);
        }
        else
        {
            DoRadialDamage((Vector2)transform.position, lightHitRadius, lightDamage);
            yield return null;
        }

        isAttacking = false;
    }

    private IEnumerator HeavyAttackRoutine()
    {
        isAttacking = true;

        if (sword != null)
        {
            if (hideSwordWhenIdle) sword.gameObject.SetActive(true);

            Vector3 start = swordRestLocalPos;
            Vector3 from = swordRestLocalPos + new Vector3(-facing * heavySwordDistance, 0f, 0f);
            Vector3 to = swordRestLocalPos + new Vector3(facing * heavySwordDistance, 0f, 0f);

            sword.localPosition = from;

            float t = 0f;
            float dur = Mathf.Max(0.001f, heavySwordSweepTime);
            bool damageDone = false;

            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                sword.localPosition = Vector3.Lerp(from, to, t);

                if (!damageDone && t >= 0.5f)
                {
                    damageDone = true;
                    DoRadialDamage((Vector2)transform.position, heavyAttackRadius, heavyDamage);
                }

                yield return null;
            }

            sword.localPosition = start;
            if (hideSwordWhenIdle) sword.gameObject.SetActive(false);
        }
        else
        {
            DoRadialDamage((Vector2)transform.position, heavyAttackRadius, heavyDamage);
            yield return null;
        }

        isAttacking = false;
    }

    private void DoRadialDamage(Vector2 center, float radius, int damage)
    {
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

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (headCheckCollider != null)
        {
            Bounds b = headCheckCollider.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        Gizmos.DrawWireSphere(transform.position, lightHitRadius);
        Gizmos.DrawWireSphere(transform.position, heavyAttackRadius);
    }
}