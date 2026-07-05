using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUps : MonoBehaviour
{
   public bool isPowerDefence;
   public PowerUpSetting PowerUpSettings;
   public ParticleSystem DefenderEffect;
   public int CurrentDuration;
   DefencePowerUp defencePowerUp;
   TrapofEnemy trapofEnemy;

   

   private void Start() 
   {
      defencePowerUp = FindObjectOfType<DefencePowerUp>();
      trapofEnemy = FindObjectOfType<TrapofEnemy>();

      CurrentDuration = PowerUpSettings.TotalDuration;
      

      DefenderEffect.gameObject.SetActive(false);
   }

   void Update() 
   {
    
    if (defencePowerUp.isDefence)
    {
        isPowerDefence = defencePowerUp.isDefence;
        CurrentDuration = Mathf.Clamp(CurrentDuration, 0, PowerUpSettings.TotalDuration);
        PowerUpSettings.CountTime -= Time.timeScale * PowerUpSettings.TimeSpeed * Time.deltaTime;

        if (PowerUpSettings.CountTime <= 0f)
        {
            DefenderEffect.gameObject.SetActive(true);
            DefenderEffect.Play();
            CurrentDuration--;
            PowerUpSettings.CountTime = 10f;
            
        }

        if (CurrentDuration == 0)
        {
            DefenderEffect.Stop();
            DefenderEffect.gameObject.SetActive(false);
            defencePowerUp.isDefence = false;
            trapofEnemy.isActiveDefence = false;
            
            isPowerDefence = false;
            CurrentDuration = 10;
        }
    }
    else if (CurrentDuration == 0)
    {
        
        CurrentDuration = PowerUpSettings.TotalDuration;
       
    }
       
       
   }

}



