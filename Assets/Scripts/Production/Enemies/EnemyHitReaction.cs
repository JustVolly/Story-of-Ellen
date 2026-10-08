using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitReaction : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float knockbackForce = 2.5f;
    [SerializeField] private CameraJuice cameraJuice;
    [SerializeField] private HitStop hitStop;

    private SpriteRenderer sprite;
    private Rigidbody2D body;
    private Coroutine flashRoutine;
    private Color baseColor;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        baseColor = sprite.color;
        if (health == null) health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null) health.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (health != null) health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
        if (body != null) body.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
        cameraJuice?.Shake(0.08f, 0.07f);
        hitStop?.Play(0.035f, 0.08f);
    }

    private IEnumerator Flash()
    {
        sprite.color = Color.white;
        yield return new WaitForSecondsRealtime(flashDuration);
        sprite.color = baseColor;
        flashRoutine = null;
    }
}
