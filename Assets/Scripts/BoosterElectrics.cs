using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterElectrics : MonoBehaviour
{
   public PowerUpSetting PowerUpSettings;
   public ParticleSystem ElectricEffect;
   public int CurrentDuration;

   public SpriteRenderer CharacterColor;
   public string TargetHexadecimalOfColor;
   public string NormalHexadecimalOfColor;
   public float InterpolationSpeed = 0.7f;

   Color NewColor;

   BoosterPowerUp boosterPowerUp;

   private void Start() 
   {
      
      if (PowerUpSettings == null)
      {
          Debug.LogWarning("[BoosterElectrics] Missing PowerUpSettings; effect disabled.", this);
          enabled = false;
          return;
      }
      CurrentDuration = Mathf.Max(1, PowerUpSettings.TotalDuration);
      if (ElectricEffect != null) ElectricEffect.gameObject.SetActive(false);
      if (CharacterColor == null)
      {
          PlayerMovement player = FindObjectOfType<PlayerMovement>();
          if (player != null) CharacterColor = player.GetComponentInChildren<SpriteRenderer>();
      }
      
      boosterPowerUp = FindObjectOfType<BoosterPowerUp>();

      

   }

   void Update() 
   {
       if (boosterPowerUp == null || PowerUpSettings == null) return;
       if (boosterPowerUp.isBooster)
       {
           CurrentDuration = Mathf.Clamp(CurrentDuration,0,PowerUpSettings.TotalDuration);
           // Time.deltaTime already includes the time scale.
           PowerUpSettings.CountTime -= PowerUpSettings.TimeSpeed * Time.deltaTime;
          

           if (PowerUpSettings.CountTime <= 0f)
            {
              if (ElectricEffect != null)
              {
                  ElectricEffect.gameObject.SetActive(true);
                  ElectricEffect.Play();
              }
              CurrentDuration--;
              PowerUpSettings.CountTime = 10f;
              if (CharacterColor != null) CharacterColor.color = Color.red;
            }  

            if (CurrentDuration == 0)
            {
                if (ElectricEffect != null)
                {
                    ElectricEffect.Stop();
                    ElectricEffect.gameObject.SetActive(false);
                }
                boosterPowerUp.isBooster = false;
                if (CharacterColor != null) CharacterColor.color = Color.white;
                CurrentDuration = Mathf.Max(1, PowerUpSettings.TotalDuration);
            }

       }
       
       
   }
}
