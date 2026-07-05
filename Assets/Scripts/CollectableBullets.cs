using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableBullets : MonoBehaviour
{
     CharacterAttack characterAttack;
     CanvasControl canvasControl;

     private void Start() 
     {
        characterAttack = FindObjectOfType<CharacterAttack>();
        canvasControl = FindObjectOfType<CanvasControl>();
     }


  void OnTriggerEnter2D(Collider2D other) 
  {
    if (other.gameObject.tag == "Player")
    {
       characterAttack.CurrentBullet += 1;         
       characterAttack.NumberConfinerofBullet += 1;
       canvasControl.IncreaseBullet(characterAttack.CurrentBullet);
        
        
        

        Destroy(gameObject);
    }
  }
   
}
