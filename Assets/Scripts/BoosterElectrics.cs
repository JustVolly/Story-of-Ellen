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
      
      CurrentDuration = PowerUpSettings.TotalDuration;
      ElectricEffect.gameObject.SetActive(false);
      
      boosterPowerUp = FindObjectOfType<BoosterPowerUp>();

      

   }

   void Update() 
   {
       if (boosterPowerUp.isBooster)
       {
           CurrentDuration = Mathf.Clamp(CurrentDuration,0,PowerUpSettings.TotalDuration);
           PowerUpSettings.CountTime -= Time.timeScale * PowerUpSettings.TimeSpeed * Time.deltaTime;
          

           if (PowerUpSettings.CountTime <= 0f)
            {
              ElectricEffect.gameObject.SetActive(true);
              ElectricEffect.Play();
              CurrentDuration--;
              PowerUpSettings.CountTime = 10f;
              CharacterColor.color = Color.red;
            }  

            if (CurrentDuration == 0)
            {
                ElectricEffect.Stop();
                ElectricEffect.gameObject.SetActive(false);
                boosterPowerUp.isBooster = false;
                CharacterColor.color = Color.white;
                CurrentDuration = 10;
            }

       }
       
       
   }
}
