using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelCompletionTrigger : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;

    private void Awake()
    {
        BoxCollider2D zone = GetComponent<BoxCollider2D>();
        zone.isTrigger = true;
        if (flow == null) flow = FindObjectOfType<LevelFlowController>();
    }

    private void OnTriggerEnter2D(Collider2D other) => TryFinish(other);
    private void OnTriggerStay2D(Collider2D other) => TryFinish(other);

    private void TryFinish(Collider2D other)
    {
        if (flow == null || flow.Completed) return;
        // Player character rigs can place a collider on an untagged child.
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isAlive) return;
        // Required objectives may become satisfied while Ellen is already
        // inside the exit sensor; no need to leave and re-enter the trigger.
        flow.TryComplete();
    }
}
