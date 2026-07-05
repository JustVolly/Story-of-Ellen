using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterPowerUp : MonoBehaviour
{
   public bool isBooster;
    
    void OnTriggerEnter2D(Collider2D other) 
   {
      if (other.gameObject.tag == "Player")
      {
        isBooster = true;
        Destroy(gameObject,0.3f);
      }

   }

}
