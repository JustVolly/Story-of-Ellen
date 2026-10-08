using UnityEngine;

/// <summary>Legacy defence-power-up timer, safe when optional scene references are absent.</summary>
public class PowerUps : MonoBehaviour
{
    public bool isPowerDefence;
    public PowerUpSetting PowerUpSettings;
    public ParticleSystem DefenderEffect;
    public int CurrentDuration;

    private DefencePowerUp defencePowerUp;
    private TrapofEnemy trapofEnemy;

    private void Start()
    {
        defencePowerUp = FindObjectOfType<DefencePowerUp>();
        trapofEnemy = FindObjectOfType<TrapofEnemy>();

        if (PowerUpSettings == null)
        {
            Debug.LogWarning("[PowerUps] Missing PowerUpSettings; effect disabled.", this);
            enabled = false;
            return;
        }

        CurrentDuration = Mathf.Max(1, PowerUpSettings.TotalDuration);
        if (DefenderEffect != null) DefenderEffect.gameObject.SetActive(false);

        if (defencePowerUp == null)
            Debug.LogWarning("[PowerUps] DefencePowerUp not found; timer stays inactive.", this);
    }

    private void Update()
    {
        if (PowerUpSettings == null || defencePowerUp == null) return;

        if (defencePowerUp.isDefence)
        {
            isPowerDefence = true;
            CurrentDuration = Mathf.Clamp(CurrentDuration, 0, PowerUpSettings.TotalDuration);
            // Time.deltaTime is already scaled, so don't multiply it by Time.timeScale.
            PowerUpSettings.CountTime -= PowerUpSettings.TimeSpeed * Time.deltaTime;

            if (PowerUpSettings.CountTime <= 0f)
            {
                if (DefenderEffect != null)
                {
                    DefenderEffect.gameObject.SetActive(true);
                    DefenderEffect.Play();
                }
                CurrentDuration--;
                PowerUpSettings.CountTime = 10f;
            }

            if (CurrentDuration <= 0)
            {
                if (DefenderEffect != null)
                {
                    DefenderEffect.Stop();
                    DefenderEffect.gameObject.SetActive(false);
                }
                defencePowerUp.isDefence = false;
                if (trapofEnemy != null) trapofEnemy.isActiveDefence = false;

                isPowerDefence = false;
                CurrentDuration = Mathf.Max(1, PowerUpSettings.TotalDuration);
            }
        }
        else if (CurrentDuration <= 0)
        {
            CurrentDuration = Mathf.Max(1, PowerUpSettings.TotalDuration);
        }
    }
}
