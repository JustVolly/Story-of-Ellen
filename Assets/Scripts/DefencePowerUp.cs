using UnityEngine;

public class DefencePowerUp : MonoBehaviour
{
    // Retained for existing prefab serialization; runtime state lives in PowerUps.
    public bool isDefence;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        PowerUps controller = other.GetComponentInParent<PowerUps>();
        if (controller == null) controller = FindAnyObjectByType<PowerUps>();

        if (controller == null)
        {
            Debug.LogWarning("Defence pickup requires an active PowerUps controller.", this);
            return;
        }

        collected = true;
        isDefence = true;
        controller.ActivateDefence();
        Destroy(gameObject);
    }
}
