using UnityEngine;

public class SecretArea : MonoBehaviour
{
    [SerializeField] private GameObject revealEffect;
    private bool discovered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (discovered || !other.CompareTag("Player")) return;
        discovered = true;
        GameSession.Instance?.DiscoverSecret();
        if (revealEffect != null) revealEffect.SetActive(true);
    }
}
