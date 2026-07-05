using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OnTraps : MonoBehaviour
{
   PlayerHealth playerHealth;
    CanvasControl canvasControl;
    PlayerMovement playerMovement;
    DefencePowerUp defencePowerUp;
    
    [SerializeField] Tilemap tile;
    

   
    

    public bool isTakingDamage = false;
    public bool isOnTouch;
    
    private Animator Onanimator;


                 

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>(); 
        canvasControl = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasControl>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        defencePowerUp = FindObjectOfType<DefencePowerUp>();
        
        Onanimator = GetComponent<Animator>();
       
        
 
       
    }

    
    



    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "Player")
        {
            if(defencePowerUp.isDefence) { return;}
            
            
            
            
            isTakingDamage = true;
            isOnTouch = true;
            
            
            if (isOnTouch)
            {
               canvasControl.TakingDamage();
               playerHealth.DecreaseHealth();
            }
           

            if(playerHealth.currenthealth == 0)
            {
                 playerMovement.CharacterAnimator.SetBool("fall", true);
                 playerMovement.CharacterAnimator.SetBool("idle", false);
                 
                 tile.gameObject.GetComponent<TilemapCollider2D>().isTrigger = true;
                 collision.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            }
            
        }


        
    }


   void OnBecameVisible() 
   {
       Onanimator.SetBool("idle",false);
       Onanimator.SetBool("running",true);

   }

   void OnBecameInvisible() 
   {
       Onanimator.SetBool("running",false);
       Onanimator.SetBool("idle",true);
   }
}
