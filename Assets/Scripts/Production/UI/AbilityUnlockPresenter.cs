using System.Collections;
using TMPro;
using UnityEngine;

public class AbilityUnlockPresenter : MonoBehaviour
{
    [SerializeField] private PlayerAbilityController abilities;
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField, Min(0.1f)] private float visibleDuration = 2.5f;

    private Coroutine routine;

    private void OnEnable()
    {
        if (abilities != null) abilities.AbilityUnlocked += Present;
        if (panel != null) panel.alpha = 0f;
    }

    private void OnDisable()
    {
        if (abilities != null) abilities.AbilityUnlocked -= Present;
    }

    private void Present(PlayerAbilityController.Ability ability)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Show(ability));
    }

    private IEnumerator Show(PlayerAbilityController.Ability ability)
    {
        if (title != null) title.text = ability + " UNLOCKED";
        if (description != null) description.text = DescriptionFor(ability);
        if (panel != null) panel.alpha = 1f;
        yield return new WaitForSecondsRealtime(visibleDuration);
        if (panel != null) panel.alpha = 0f;
        routine = null;
    }

    private static string DescriptionFor(PlayerAbilityController.Ability ability)
    {
        if (ability == PlayerAbilityController.Ability.Dash) return "Burst forward to cross gaps and evade danger.";
        if (ability == PlayerAbilityController.Ability.WallJump) return "Kick away from walls to reach hidden paths.";
        if (ability == PlayerAbilityController.Ability.SpiritWorld) return "Shift worlds to reveal paths that do not exist here.";
        return "A new ability is available.";
    }
}
