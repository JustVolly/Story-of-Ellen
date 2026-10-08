using UnityEngine;

/// <summary>
/// Reusable energy recovery point. Staying in the zone does not spam rewards:
/// cooldown begins only after energy was actually restored.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class SpiritWell : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField, Min(0.1f)] private float energyGranted = 2.5f;
    [SerializeField, Min(0f)] private float cooldown = 8f;
    [SerializeField] private ParticleSystem rechargeEffect;
    [SerializeField] private AudioSource rechargeAudio;
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private Color readyColor = new Color(0.42f, 1f, 0.85f, 1f);
    [SerializeField] private Color cooldownColor = new Color(0.35f, 0.45f, 0.5f, 0.7f);

    private float nextReadyTime;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindAnyObjectByType<SpiritWorldController>();
        Collider2D zone = GetComponent<Collider2D>();
        zone.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryRecharge(other);
    private void OnTriggerStay2D(Collider2D other) => TryRecharge(other);

    private void Update()
    {
        if (indicator != null)
            indicator.color = Time.time >= nextReadyTime ? readyColor : cooldownColor;
    }

    private void TryRecharge(Collider2D other)
    {
        if (spiritWorld == null || Time.time < nextReadyTime) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || !player.isAlive) return;

        // A fully charged player does not waste a well activation.
        if (spiritWorld.RestoreEnergy(energyGranted) <= 0.0001f) return;

        nextReadyTime = Time.time + cooldown;
        if (rechargeEffect != null) rechargeEffect.Play();
        if (rechargeAudio != null && rechargeAudio.clip != null) rechargeAudio.Play();
    }
}
