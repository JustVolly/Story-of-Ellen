using System.Collections;
using UnityEngine;

/// <summary>
/// Spirit barriers must never rematerialize inside Ellen when energy runs out.
/// The visual may return immediately, but collision is restored only once the
/// player has cleared the barrier's original shape.
/// </summary>
public class SpiritGate : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private bool passableInSpiritWorld = true;

    private Coroutine pendingSolidify;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindAnyObjectByType<SpiritWorldController>();
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
        CancelPendingSolidification();
    }

    private void Apply(bool spirit)
    {
        bool passable = passableInSpiritWorld ? spirit : !spirit;
        CancelPendingSolidification();

        if (blockingCollider != null)
        {
            if (passable)
                blockingCollider.enabled = false;
            else if (PlayerInsideOriginalBounds())
            {
                // Enabled colliders can push or trap a character standing inside.
                blockingCollider.enabled = false;
                pendingSolidify = StartCoroutine(SolidifyWhenPlayerLeaves());
            }
            else
                blockingCollider.enabled = true;
        }

        if (visualRoot != null) visualRoot.SetActive(!passable);

        if (visual != null)
        {
            Color color = visual.color;
            color.a = passable ? 0.3f : 1f;
            visual.color = color;
        }
    }

    private IEnumerator SolidifyWhenPlayerLeaves()
    {
        var fixedStep = new WaitForFixedUpdate();
        while (PlayerInsideOriginalBounds())
            yield return fixedStep;

        pendingSolidify = null;
        if (blockingCollider != null) blockingCollider.enabled = true;
    }

    private bool PlayerInsideOriginalBounds()
    {
        if (blockingCollider == null) return false;

        // Collider2D.bounds becomes empty when disabled. Reconstruct the
        // authored BoxCollider2D shape instead of relying on that property.
        BoxCollider2D box = blockingCollider as BoxCollider2D;
        if (box != null)
        {
            Vector3 scale = box.transform.lossyScale;
            Vector2 center = box.transform.TransformPoint(box.offset);
            Vector2 dimensions = new Vector2(
                Mathf.Abs(box.size.x * scale.x),
                Mathf.Abs(box.size.y * scale.y));
            Collider2D[] overlapping = Physics2D.OverlapBoxAll(
                center, dimensions, box.transform.eulerAngles.z);
            foreach (Collider2D found in overlapping)
                if (found != null && found.GetComponentInParent<PlayerHealth>() != null)
                    return true;
            return false;
        }

        // Fallback for any manually-authored non-box barriers.
        Bounds bounds = blockingCollider.bounds;
        if (bounds.size.sqrMagnitude <= 0.0001f) return false;
        foreach (Collider2D found in Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f))
            if (found != null && found.GetComponentInParent<PlayerHealth>() != null)
                return true;
        return false;
    }

    private void CancelPendingSolidification()
    {
        if (pendingSolidify == null) return;
        StopCoroutine(pendingSolidify);
        pendingSolidify = null;
    }
}
