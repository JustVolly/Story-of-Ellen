using UnityEngine;

public class MemoryFragment : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource collectAudio;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameSession.Instance?.CollectMemory();
        if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);
        if (collectAudio != null) AudioSource.PlayClipAtPoint(collectAudio.clip, transform.position, collectAudio.volume);
        Destroy(gameObject);
    }
}
