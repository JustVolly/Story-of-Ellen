using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelBeatTrigger : MonoBehaviour
{
    [SerializeField] private VerticalSliceDirector director;
    [SerializeField] private VerticalSliceDirector.Beat beat;
    [SerializeField] private bool triggerOnce = true;

    private bool triggered;

    private void Awake()
    {
        BoxCollider2D zone = GetComponent<BoxCollider2D>();
        zone.isTrigger = true;

        if (director == null) director = FindAnyObjectByType<VerticalSliceDirector>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((triggerOnce && triggered) || !other.CompareTag("Player")) return;

        triggered = true;
        director?.SetBeat(beat);
    }
}
