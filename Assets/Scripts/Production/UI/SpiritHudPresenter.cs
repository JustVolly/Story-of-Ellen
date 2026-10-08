using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpiritHudPresenter : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private Image energyFill;
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private TextMeshProUGUI stateText;

    private void Awake()
    {
        if (spiritWorld == null) spiritWorld = FindObjectOfType<SpiritWorldController>();
    }

    private void OnEnable()
    {
        if (spiritWorld == null) return;
        spiritWorld.EnergyChanged += UpdateEnergy;
        spiritWorld.WorldChanged += UpdateState;
        UpdateEnergy(spiritWorld.NormalizedEnergy);
        UpdateState(spiritWorld.IsSpiritWorld);
    }

    private void OnDisable()
    {
        if (spiritWorld == null) return;
        spiritWorld.EnergyChanged -= UpdateEnergy;
        spiritWorld.WorldChanged -= UpdateState;
    }

    private void UpdateEnergy(float normalized)
    {
        if (energyFill != null) energyFill.fillAmount = Mathf.Clamp01(normalized);
    }

    private void UpdateState(bool active)
    {
        if (activeIndicator != null) activeIndicator.SetActive(active);
        if (stateText != null) stateText.text = active ? "SPIRIT" : "MATERIAL";
    }
}
