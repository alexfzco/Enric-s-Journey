using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float invulnerabilitySeconds = 0.25f;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public int maxJumps = 2;

    [Header("Teleport")]
    public KeyCode tpKey = KeyCode.E;
    public GameObject tpMarkerPrefab;
    public Vector3 markerOffset = Vector3.zero;

    [Header("Combat Inputs & Cooldowns")]
    public KeyCode lightAttackKey = KeyCode.J;
    public KeyCode heavyAttackKey = KeyCode.K;
    public LayerMask enemyMask;
    public int lightDamage = 25;
    public int heavyDamage = 50;
    public float lightAttackCooldown = 0.35f;
    public float heavyAttackCooldown = 1.0f;

    [Header("Combat Hitboxes")]
    public float lightHitRadius = 0.9f;
    public float heavyAttackRadius = 1.3f;

    [Header("Sword Visuals")]
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

        if (Mathf.Abs(moveInput) > 0.01f)
            facing = moveInput > 0 ? 1 : -1;

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetKeyDown(tpKey))
            HandleTeleportToggle();

        if (Input.GetKeyDown(lightAttackKey))
            TryLightAttack();

        if (Input.GetKeyDown(heavyAttackKey))
            TryHeavyAttack();
    }

    void FixedUpdate()
    {
        // Movimiento horizontal simple sin multiplicadores de agachado ni bloqueos de dash
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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

        Gizmos.DrawWireSphere(transform.position, lightHitRadius);
        Gizmos.DrawWireSphere(transform.position, heavyAttackRadius);
    }
}