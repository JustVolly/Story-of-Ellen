using UnityEngine;

public class LevelEntryConfigurator : MonoBehaviour
{
    [SerializeField, Min(1)] private int levelNumber = 1;
    [SerializeField] private PlayerAbilityController.Ability grantOnStart = PlayerAbilityController.Ability.None;
    [SerializeField] private PlayerAbilityController abilities;

    private void Start()
    {
        if (abilities == null) abilities = FindObjectOfType<PlayerAbilityController>();

        ProgressionSave.Data data = ProgressionSave.Load();
        data.highestUnlockedLevel = Mathf.Max(data.highestUnlockedLevel, levelNumber);
        ProgressionSave.Save(data);

        if (abilities != null && grantOnStart != PlayerAbilityController.Ability.None)
            abilities.Unlock(grantOnStart);
    }
}
