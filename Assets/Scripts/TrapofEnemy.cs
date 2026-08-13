using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrapofEnemy : MonoBehaviour
{
    PlayerHealth playerHealth;
    CanvasControl canvasControl;
    PlayerMovement playerMovement;
    DefencePowerUp defencePowerUp;
    BulletDamage bulletDamage;
    PowerUps powerUps;
    
    
    
    [SerializeField] Tilemap tile;
    
    public bool isTakingDamage = false;
    public bool isSawTouch;
    public bool isTouch = false;
    public bool isActiveDefence = false;
    private Animator Sawanimator;


                 

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>(); 
        canvasControl = FindObjectOfType<CanvasControl>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        defencePowerUp = FindObjectOfType<DefencePowerUp>();
        Sawanimator = GetComponent<Animator>();
        bulletDamage = FindObjectOfType<BulletDamage>();
        powerUps = FindObjectOfType<PowerUps>();
      
       
    }

    
   



private void OnTriggerEnter2D(Collider2D collision)
{
        
    if(collision.gameObject.tag == "Player" && (isActiveDefence || powerUps.DefenderEffect.isPlaying)) 
    {    
               Debug.Log("Saw zarar vermiyor");
                return;
          
    }
       
       
       
    if (collision.gameObject.tag == "Player")
    {
           
        StartCoroutine(bulletDamage.BlinkEffect());
        isTakingDamage = true;
        isSawTouch = true;
            
            if (isSawTouch)
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
       Sawanimator.SetBool("idle",false);
       Sawanimator.SetBool("running",true);

   }

   void OnBecameInvisible() 
   {
       Sawanimator.SetBool("running",false);
       Sawanimator.SetBool("idle",true);
   }

    

   
   






}
