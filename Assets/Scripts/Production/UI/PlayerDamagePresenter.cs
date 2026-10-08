using System.Collections;
using UnityEngine;

public class PlayerDamagePresenter : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private CameraJuice cameraJuice;
    [SerializeField] private HitStop hitStop;
    [SerializeField] private CanvasGroup deathOverlay;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;

    private int previousHealth;
    private Color baseColor;

    private void Awake()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
        if (health != null) previousHealth = health.currenthealth;
        if (deathOverlay != null) deathOverlay.alpha = 0f;
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.HealthChanged += OnHealthChanged;
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < previousHealth && current > 0)
        {
            StartCoroutine(Flash());
            cameraJuice?.Shake(0.12f, 0.1f);
            hitStop?.Play(0.045f, 0.08f);
        }
        if (current > 0 && deathOverlay != null) deathOverlay.alpha = 0f;
        previousHealth = current;
    }

    private void OnDied()
    {
        cameraJuice?.Shake(0.22f, 0.16f);
        if (deathOverlay != null) deathOverlay.alpha = 1f;
    }

    private IEnumerator Flash()
    {
        if (sprite == null) yield break;
        sprite.color = Color.white;
        yield return new WaitForSecondsRealtime(flashDuration);
        sprite.color = baseColor;
    }
}
