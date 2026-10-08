using UnityEngine;

public class CollectableBullets : MonoBehaviour
{
    [SerializeField, Min(1)] private int ammoAmount = 1;
    [SerializeField] private bool increaseCapacity = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterAttack attack = other.GetComponent<CharacterAttack>();
        if (attack == null) return;

        attack.AddAmmo(ammoAmount, increaseCapacity);
        Destroy(gameObject);
    }
}
