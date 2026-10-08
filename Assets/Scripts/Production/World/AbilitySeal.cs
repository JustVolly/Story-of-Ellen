using UnityEngine;

/// <summary>
/// A single-action puzzle barrier. Touch its proximity sensor while dashing,
/// or while shifted into Spirit World. It unlocks permanently for this run,
/// leaving the underlying authored terrain untouched.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class AbilitySeal : MonoBehaviour
{
    public enum RequiredAction { SpiritWorld, Dash }

    [SerializeField] private RequiredAction requiredAction;
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Collider2D proximityTrigger;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioSource activationAudio;

    private bool unlocked;
    public bool IsUnlocked => unlocked;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindAnyObjectByType<SpiritWorldController>();
        if (blockingCollider == null)
        {
            foreach (Collider2D candidate in GetComponents<Collider2D>())
                if (!candidate.isTrigger) { blockingCollider = candidate; break; }
        }
        if (proximityTrigger == null)
        {
            foreach (Collider2D candidate in GetComponents<Collider2D>())
                if (candidate.isTrigger) { proximityTrigger = candidate; break; }
        }
        if (proximityTrigger != null) proximityTrigger.isTrigger = true;
        Refresh();
    }

    private void OnTriggerEnter2D(Collider2D other) => TryUnlock(other);
    private void OnTriggerStay2D(Collider2D other) => TryUnlock(other);

    private void TryUnlock(Collider2D other)
    {
        if (unlocked) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isAlive) return;

        bool actionSatisfied = requiredAction == RequiredAction.SpiritWorld
            ? spiritWorld != null && spiritWorld.IsSpiritWorld
            : IsPlayerDashing(other);
        if (!actionSatisfied) return;

        unlocked = true;
        Refresh();
        if (activationEffect != null) activationEffect.Play();
        if (activationAudio != null && activationAudio.clip != null) activationAudio.Play();
    }

    private static bool IsPlayerDashing(Collider2D other)
    {
        PlayerAdvancedMovement movement = other.GetComponentInParent<PlayerAdvancedMovement>();
        return movement != null && movement.IsDashing;
    }

    private void Refresh()
    {
        if (blockingCollider != null) blockingCollider.enabled = !unlocked;
        if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
        if (proximityTrigger != null) proximityTrigger.enabled = !unlocked;
    }
}
