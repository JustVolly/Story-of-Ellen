using UnityEngine;

public class PowerUps : MonoBehaviour
{
    public bool isPowerDefence;
    public PowerUpSetting PowerUpSettings;
    public ParticleSystem DefenderEffect;
    public int CurrentDuration;

    private float remainingSeconds;
    private float secondsPerTick = 1f;

    public bool IsActive => isPowerDefence && remainingSeconds > 0f;

    private void Awake()
    {
        isPowerDefence = false;
        CurrentDuration = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;
        SetEffectActive(false);
    }

    public void ActivateDefence()
    {
        int ticks = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;
        float speed = PowerUpSettings != null ? Mathf.Max(0.01f, PowerUpSettings.TimeSpeed) : 4f;

        // Keep the original duration scale without mutating shared ScriptableObject data.
        secondsPerTick = 10f / speed;
        remainingSeconds = ticks * secondsPerTick;
        CurrentDuration = ticks;
        isPowerDefence = true;

        SetTrapDefence(true);
        SetEffectActive(true);
    }

    private void Update()
    {
        if (!IsActive) return;

        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
        CurrentDuration = Mathf.CeilToInt(remainingSeconds / secondsPerTick);
        if (remainingSeconds <= 0f) StopDefence();
    }

    private void OnDisable()
    {
        StopDefence();
    }

    private void StopDefence()
    {
        remainingSeconds = 0f;
        isPowerDefence = false;
        CurrentDuration = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;

        SetTrapDefence(false);
        SetEffectActive(false);
    }

    private void SetTrapDefence(bool active)
    {
        // Multiple traps can exist in one level; all should receive the same state.
        foreach (TrapofEnemy trap in FindObjectsByType<TrapofEnemy>(FindObjectsSortMode.None))
            trap.isActiveDefence = active;
    }

    private void SetEffectActive(bool active)
    {
        if (DefenderEffect == null) return;
        if (!active) DefenderEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (DefenderEffect.gameObject != gameObject) DefenderEffect.gameObject.SetActive(active);
        if (active) DefenderEffect.Play();
    }
}
