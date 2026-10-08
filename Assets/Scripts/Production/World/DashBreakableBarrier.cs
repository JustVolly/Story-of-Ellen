using UnityEngine;

public class DashBreakableBarrier : MonoBehaviour
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private ParticleSystem breakEffect;
    [SerializeField] private AudioClip breakSound;

    private bool broken;

    private void Awake()
    {
        if (blockingCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D item in colliders)
                if (item != null && !item.isTrigger) { blockingCollider = item; break; }
        }

        if (triggerCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D item in colliders)
                if (item != null && item.isTrigger) { triggerCollider = item; break; }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => TryBreak(other);

    private void OnTriggerStay2D(Collider2D other) => TryBreak(other);

    private void TryBreak(Collider2D other)
    {
        if (broken || !other.CompareTag("Player")) return;

        PlayerAdvancedMovement movement = other.GetComponent<PlayerAdvancedMovement>();
        if (movement == null || !movement.IsDashing) return;

        Break();
    }

    private void Break()
    {
        broken = true;

        if (blockingCollider != null) blockingCollider.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;
        if (visualRoot != null) visualRoot.SetActive(false);
        if (breakEffect != null) breakEffect.Play();
        AudioManager.Instance?.PlaySfx(breakSound);
    }
}
