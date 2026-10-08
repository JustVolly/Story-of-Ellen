using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField] private PlayerAbilityController.Ability ability = PlayerAbilityController.Ability.Dash;
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private VerticalSliceDirector director;
    [SerializeField] private VerticalSliceDirector.Beat nextBeat = VerticalSliceDirector.Beat.Traversal;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        PlayerAbilityController controller = other.GetComponent<PlayerAbilityController>();
        if (controller == null) return;

        collected = true;
        controller.Unlock(ability);
        if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
        AudioManager.Instance?.PlaySfx(pickupSound);
        if (director != null) director.SetBeat(nextBeat);
        Destroy(gameObject);
    }
}
