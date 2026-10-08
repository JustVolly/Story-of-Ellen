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
        if (collected || ability == PlayerAbilityController.Ability.None || !other.CompareTag("Player")) return;
        PlayerAbilityController controller = other.GetComponent<PlayerAbilityController>();
        if (controller == null) return;

        collected = true;
        controller.Unlock(ability);
        if (pickupEffect != null)
        {
            ParticleSystem effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = effect.main;
            Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax + 0.25f);
        }
        AudioManager.Instance?.PlaySfx(pickupSound);
        if (director != null) director.SetBeat(nextBeat);
        Destroy(gameObject);
    }
}
