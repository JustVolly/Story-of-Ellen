using System;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    [Flags]
    public enum Ability { None = 0, Dash = 1, WallJump = 2, SpiritWorld = 4 }

    [SerializeField] private Ability unlocked = Ability.SpiritWorld;
    [SerializeField] private PlayerAdvancedMovement advancedMovement;
    [SerializeField] private SpiritWorldController spiritWorld;

    public event Action<Ability> AbilityUnlocked;

    private void Awake()
    {
        ProgressionSave.Data data = ProgressionSave.Load();
        unlocked |= (Ability)data.unlockedAbilities;
    }
    public bool Has(Ability ability) => (unlocked & ability) == ability;

    public void Unlock(Ability ability)
    {
        if (Has(ability)) return;
        unlocked |= ability;
        ProgressionSave.Data data = ProgressionSave.Load();
        data.unlockedAbilities = (int)unlocked;
        ProgressionSave.Save(data);
        AbilityUnlocked?.Invoke(ability);
    }

    public void TryDash() { if (Has(Ability.Dash) && advancedMovement != null) advancedMovement.Dash(); }
    public void TryWallJump() { if (Has(Ability.WallJump) && advancedMovement != null) advancedMovement.WallJump(); }
    public void TryToggleSpirit() { if (Has(Ability.SpiritWorld) && spiritWorld != null) spiritWorld.ToggleSpiritWorld(); }
}
