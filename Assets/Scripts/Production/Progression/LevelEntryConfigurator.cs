using UnityEngine;

public class LevelEntryConfigurator : MonoBehaviour
{
    [SerializeField, Min(1)] private int levelNumber = 1;
    [SerializeField] private PlayerAbilityController.Ability grantOnStart = PlayerAbilityController.Ability.None;
    [SerializeField] private PlayerAbilityController abilities;

    private void Start()
    {
        if (abilities == null) abilities = FindAnyObjectByType<PlayerAbilityController>();

        ProgressionSave.Data data = ProgressionSave.Load();
        data.highestUnlockedLevel = Mathf.Max(data.highestUnlockedLevel, levelNumber);
        ProgressionSave.Save(data);

        GrantConfiguredAbilities();
    }

    private void GrantConfiguredAbilities()
    {
        if (abilities == null || grantOnStart == PlayerAbilityController.Ability.None) return;

        GrantIfConfigured(PlayerAbilityController.Ability.Dash);
        GrantIfConfigured(PlayerAbilityController.Ability.WallJump);
        GrantIfConfigured(PlayerAbilityController.Ability.SpiritWorld);
    }

    private void GrantIfConfigured(PlayerAbilityController.Ability ability)
    {
        if ((grantOnStart & ability) == ability)
            abilities.Unlock(ability);
    }
}
