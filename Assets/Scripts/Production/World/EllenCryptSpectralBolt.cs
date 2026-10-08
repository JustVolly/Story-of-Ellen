using UnityEngine;

/// <summary>Simple, bounded projectile used by the Astral Crypt sentinels.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class EllenCryptSpectralBolt : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 velocity;
    private float expiryTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void Launch(Vector2 direction, float speed, float lifetime)
    {
        velocity = direction.normalized * speed;
        expiryTime = Time.time + Mathf.Max(0.5f, lifetime);
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        if (Time.time >= expiryTime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isAlive) return;
        health.TakeDamage(1);
        Destroy(gameObject);
    }
}
