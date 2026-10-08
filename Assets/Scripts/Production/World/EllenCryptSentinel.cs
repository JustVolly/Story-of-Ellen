using UnityEngine;

/// <summary>Ranged crypt enemy; no prefab dependencies or hidden scene wiring.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class EllenCryptSentinel : MonoBehaviour
{
    [SerializeField, Min(2f)] private float sightRange = 16f;
    [SerializeField, Min(0.5f)] private float attackInterval = 2.5f;
    [SerializeField, Min(1f)] private float boltSpeed = 6f;
    [SerializeField, Min(0.5f)] private float boltLifetime = 3f;

    private PlayerHealth target;
    private SpriteRenderer appearance;
    private float nextAttack;

    private void Awake()
    {
        appearance = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Time.time < nextAttack) return;
        if (target == null)
            target = FindAnyObjectByType<PlayerHealth>();
        if (target == null || !target.isAlive) return;

        Vector2 delta = target.transform.position - transform.position;
        if (delta.sqrMagnitude > sightRange * sightRange) return;

        nextAttack = Time.time + attackInterval;
        Fire(delta.normalized);
    }

    private void Fire(Vector2 direction)
    {
        GameObject bolt = new GameObject("AstralCrypt_SpectralBolt");
        bolt.transform.position = transform.position + (Vector3)(direction * 1.1f);
        bolt.transform.localScale = Vector3.one * 0.42f;

        SpriteRenderer sprite = bolt.AddComponent<SpriteRenderer>();
        sprite.sprite = appearance.sprite;
        sprite.color = new Color(0.52f, 0.85f, 1f, 1f);
        sprite.sortingOrder = 45;

        Rigidbody2D body = bolt.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D hitbox = bolt.AddComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        hitbox.radius = 0.6f;

        EllenCryptSpectralBolt projectile =
            bolt.AddComponent<EllenCryptSpectralBolt>();
        projectile.Launch(direction, boltSpeed, boltLifetime);
    }
}
