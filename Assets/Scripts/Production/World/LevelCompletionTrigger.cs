using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelCompletionTrigger : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;

    private void Awake()
    {
        BoxCollider2D zone = GetComponent<BoxCollider2D>();
        zone.isTrigger = true;
        if (flow == null) flow = FindAnyObjectByType<LevelFlowController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        flow?.TryComplete();
    }
}
