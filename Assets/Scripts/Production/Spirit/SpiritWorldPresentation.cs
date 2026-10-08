using System.Collections;
using UnityEngine;

public class SpiritWorldPresentation : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private SpriteRenderer[] tintedSprites;
    [SerializeField] private Color materialTint = Color.white;
    [SerializeField] private Color spiritTint = new Color(0.65f, 0.85f, 1f, 1f);
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.2f;
    [SerializeField] private CameraJuice cameraJuice;

    private Coroutine transition;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindAnyObjectByType<SpiritWorldController>();
    }

    private void OnEnable()
    {
        if (spiritWorld == null) return;
        spiritWorld.WorldChanged += OnWorldChanged;
        OnWorldChanged(spiritWorld.IsSpiritWorld);
    }

    private void OnDisable()
    {
        if (spiritWorld != null) spiritWorld.WorldChanged -= OnWorldChanged;
    }

    private void OnWorldChanged(bool spirit)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(TransitionTo(spirit ? spiritTint : materialTint));
        cameraJuice?.Shake(0.1f, 0.05f);
    }

    private IEnumerator TransitionTo(Color target)
    {
        if (tintedSprites == null || tintedSprites.Length == 0)
        {
            transition = null;
            yield break;
        }

        Color[] starts = new Color[tintedSprites.Length];
        for (int i = 0; i < tintedSprites.Length; i++)
            starts[i] = tintedSprites[i] != null ? tintedSprites[i].color : Color.white;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            for (int i = 0; i < tintedSprites.Length; i++)
                if (tintedSprites[i] != null) tintedSprites[i].color = Color.Lerp(starts[i], target, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (SpriteRenderer sprite in tintedSprites) if (sprite != null) sprite.color = target;
        transition = null;
    }
}
