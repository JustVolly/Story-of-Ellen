using UnityEngine;

/// <summary>A visible Level 3 hazard strip, independent of legacy TrapThorns.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class EllenCryptRuneHazard : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;

    private void Awake()
    {
        Collider2D zone = GetComponent<Collider2D>();
        if (zone != null) zone.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => Damage(other);
    private void OnTriggerStay2D(Collider2D other) => Damage(other);

    private void Damage(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null && health.isAlive)
            health.TakeDamage(damage);
    }
}
