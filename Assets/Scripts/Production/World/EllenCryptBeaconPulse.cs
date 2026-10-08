using UnityEngine;

/// <summary>
/// A lightweight Level 3-only spectral glow: visual guidance, no collision,
/// lights, physics bodies, allocations or scene lookup in Update().
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class EllenCryptBeaconPulse : MonoBehaviour
{
    [SerializeField, Range(0.1f, 2f)] private float pulsesPerSecond = 0.55f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.09f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.32f;
    [SerializeField] private float phase;
    private SpriteRenderer glow;

    public void SetPhase(float value) => phase = value;

    private void Awake() => glow = GetComponent<SpriteRenderer>();

    private void Update()
    {
        if (glow == null) return;
        float t = 0.5f + 0.5f * Mathf.Sin(
            Time.time * (Mathf.PI * 2f * pulsesPerSecond) + phase);
        Color color = glow.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        glow.color = color;
    }
}
