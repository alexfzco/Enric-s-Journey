using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 50;
    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log($"{name} recibió {amount} de daño. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();

        Debug.Log($"{name} daño: {amount} | antes: {currentHealth} | después: {currentHealth - amount}");
    }

    void Die()
    {
        Destroy(gameObject);
    }
}