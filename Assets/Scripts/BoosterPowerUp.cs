using UnityEngine;

public class BoosterPowerUp : MonoBehaviour
{
    // Retained for existing prefab serialization; runtime state lives in BoosterElectrics.
    public bool isBooster;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        BoosterElectrics controller = other.GetComponentInParent<BoosterElectrics>();
        if (controller == null) controller = FindAnyObjectByType<BoosterElectrics>();

        if (controller == null)
        {
            Debug.LogWarning("Booster pickup requires an active BoosterElectrics controller.", this);
            return;
        }

        collected = true;
        isBooster = true;
        controller.ActivateBooster();
        Destroy(gameObject);
    }
}
