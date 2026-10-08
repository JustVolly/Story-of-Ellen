using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem deathEffect;

    public int CurrentHealth { get; private set; }
    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake() => CurrentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0 || amount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (hitEffect != null) hitEffect.Play();
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth == 0) Die();
    }

    private void Die()
    {
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        Died?.Invoke();
        Destroy(gameObject);
    }
}
