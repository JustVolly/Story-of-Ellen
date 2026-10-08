using UnityEngine;

/// <summary>One-shot narrative checkpoint triggered only by a living player.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class EllenCryptGuidanceCue : MonoBehaviour
{
    [SerializeField] private EllenCryptGuidancePresenter presenter;
    [SerializeField, TextArea(2, 3)] private string message;
    private bool displayed;

    public bool IsConfigured =>
        presenter != null && presenter.IsConfigured && !string.IsNullOrWhiteSpace(message);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (displayed || !IsConfigured) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || !player.isAlive) return;

        displayed = true;
        presenter.Show(message);
    }
}
