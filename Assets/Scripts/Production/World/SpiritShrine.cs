using UnityEngine;

public class SpiritShrine : MonoBehaviour
{
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioSource activationAudio;
    [SerializeField] private Transform respawnPoint;
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player")) return;
        activated = true;

        GameSession.Instance?.SetCheckpoint(respawnPoint != null ? respawnPoint : transform);
        other.GetComponent<PlayerHealth>()?.ResetHealth();

        if (activationEffect != null) activationEffect.Play();
        if (activationAudio != null) activationAudio.Play();
    }
}
