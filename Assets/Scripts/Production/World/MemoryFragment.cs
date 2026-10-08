using UnityEngine;

public class MemoryFragment : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource collectAudio;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameSession.Instance?.CollectMemory();
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = effect.main;
            Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax + 0.25f);
        }
        if (collectAudio != null && collectAudio.clip != null)
            AudioSource.PlayClipAtPoint(collectAudio.clip, transform.position, collectAudio.volume);
        Destroy(gameObject);
    }
}
