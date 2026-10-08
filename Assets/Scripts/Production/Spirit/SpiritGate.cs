using UnityEngine;

public class SpiritGate : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private bool passableInSpiritWorld = true;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindObjectOfType<SpiritWorldController>();
        if (blockingCollider == null) blockingCollider = GetComponent<Collider2D>();
        if (visual == null) visual = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (spiritWorld == null) return;
        spiritWorld.WorldChanged += Apply;
        Apply(spiritWorld.IsSpiritWorld);
    }

    private void OnDisable()
    {
        if (spiritWorld != null) spiritWorld.WorldChanged -= Apply;
    }

    private void Apply(bool spirit)
    {
        bool passable = passableInSpiritWorld ? spirit : !spirit;
        if (blockingCollider != null) blockingCollider.enabled = !passable;
        if (visual != null)
        {
            Color color = visual.color;
            color.a = passable ? 0.3f : 1f;
            visual.color = color;
        }
    }
}
