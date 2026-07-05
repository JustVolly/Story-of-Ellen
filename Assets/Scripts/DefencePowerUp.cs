using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefencePowerUp : MonoBehaviour
{
    public bool isDefence;

    TrapofEnemy trapofEnemy;

    private void Start() 
    {
       trapofEnemy = FindObjectOfType<TrapofEnemy>();

    }
   void OnTriggerEnter2D(Collider2D other) 
   {
      if (other.gameObject.tag == "Player")
      {
        isDefence = true;
        trapofEnemy.isActiveDefence = isDefence;        
        Destroy(gameObject,0.3f);
      }

   }

   
}
