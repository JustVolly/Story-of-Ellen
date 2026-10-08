using System;
using UnityEngine;

public class SpiritWorldController : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float maxEnergy = 5f;
    [SerializeField, Min(0f)] private float drainPerSecond = 1f;
    [SerializeField, Min(0f)] private float rechargePerSecond = 0.65f;
    [SerializeField] private GameObject[] spiritOnlyObjects;
    [SerializeField] private GameObject[] materialOnlyObjects;
    [SerializeField] private ParticleSystem transitionEffect;
    [SerializeField] private AudioSource transitionAudio;

    public bool IsSpiritWorld { get; private set; }
    public float Energy { get; private set; }
    public float NormalizedEnergy => maxEnergy <= 0f ? 0f : Energy / maxEnergy;

    public event Action<bool> WorldChanged;
    public event Action<float> EnergyChanged;
    private bool initialized;

    private void Awake()
    {
        Energy = maxEnergy;
        ApplyWorld(false, true, false);
    }

    private void Update()
    {
        float previous = Energy;
        if (IsSpiritWorld)
        {
            Energy = Mathf.Max(0f, Energy - drainPerSecond * Time.deltaTime);
            if (Energy <= 0f) ApplyWorld(false);
        }
        else
        {
            Energy = Mathf.Min(maxEnergy, Energy + rechargePerSecond * Time.deltaTime);
        }

        if (!Mathf.Approximately(previous, Energy)) EnergyChanged?.Invoke(NormalizedEnergy);
    }

    public void ToggleSpiritWorld()
    {
        if (IsSpiritWorld) { ApplyWorld(false); return; }
        if (Energy > 0.1f) ApplyWorld(true);
    }

    public void ExitSpiritWorld() => ApplyWorld(false);

    private void ApplyWorld(bool spirit, bool force = false, bool playFeedback = true)
    {
        if (!force && initialized && IsSpiritWorld == spirit) return;
        initialized = true;
        IsSpiritWorld = spirit;
        SetObjects(spiritOnlyObjects, spirit);
        SetObjects(materialOnlyObjects, !spirit);
        if (playFeedback && transitionEffect != null) transitionEffect.Play();
        if (playFeedback && transitionAudio != null && transitionAudio.clip != null) transitionAudio.Play();
        WorldChanged?.Invoke(spirit);
    }

    private static void SetObjects(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        foreach (GameObject target in objects) if (target != null) target.SetActive(active);
    }
}
