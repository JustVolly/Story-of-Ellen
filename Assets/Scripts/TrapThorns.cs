using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class TrapThorns : MonoBehaviour
{
    PlayerMovement playerMovement;
    PlayerHealth playerHealth;
    CanvasControl canvasControl;
    TrapofEnemy trapofEnemy;
    ScenesManager scenesManager;  
    PowerUps powerUp; 
    BulletDamage bulletDamage; 

    public bool isTouchingthorn = false;

    public Tilemap tilemap;
    
    [SerializeField] GameObject Player;
    [SerializeField] Rigidbody2D PlayerRigid;

    [SerializeField] int JumpForce = 500;
    [SerializeField] float CollisionMomentTime;
    [SerializeField] float elapsedTime;
    [SerializeField] float GravityScale;
    [SerializeField] CompositeCollider2D GroundCompositeCollider;
    
    private CapsuleCollider2D PlayerCapsuleCollider;
    

    GameObject[] Traps;
    List<GameObject> TrapsList = new List<GameObject>();





    void Start()
    {
         playerMovement = FindObjectOfType<PlayerMovement>();
         playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();    
         canvasControl = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasControl>();
         PlayerCapsuleCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<CapsuleCollider2D>();
         

         trapofEnemy = FindObjectOfType<TrapofEnemy>();
         scenesManager = FindObjectOfType<ScenesManager>();  
         powerUp = FindObjectOfType<PowerUps>();
         bulletDamage = FindObjectOfType<BulletDamage>();

         Traps = GameObject.FindGameObjectsWithTag("Thorns");
         TrapsList.AddRange(Traps);

    }

   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
          StartCoroutine(bulletDamage.BlinkEffect());
          
          if (powerUp.isPowerDefence) { return; }
            
            
            isTouchingthorn = true;
            playerHealth.currenthealth = 0; 
            canvasControl.DieImmediate();
            
            playerMovement.CharacterAnimator.SetBool("fall", true);
            playerMovement.CharacterAnimator.SetBool("idle", false);
            Vector2 playerVelocity = new Vector2(PlayerRigid.linearVelocity.x, Vector2.up.y * JumpForce * Time.fixedDeltaTime);
            PlayerRigid.linearVelocity = playerVelocity;
            PlayerRigid.gravityScale = GravityScale;


            GroundCompositeCollider.isTrigger = true;
            collision.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            collision.gameObject.GetComponent<CapsuleCollider2D>().isTrigger = true;
            

        }

        else
        {
        
         playerMovement.CharacterAnimator.SetBool("fall", false);
         playerMovement.CharacterAnimator.SetBool("idle", true);

        }

            

        


    }


    public void CheckTrapsColliderRespawn()
    {
        foreach (var trap in TrapsList)
        {
            BoxCollider2D boxCollider = trap.GetComponent<BoxCollider2D>();

            if (scenesManager.isRespawn && trap.gameObject.GetComponent<BoxCollider2D>() != null)
            {
                boxCollider.isTrigger = false;

            }

            if(trap.gameObject.name == "ThornsTraps")
            {

                continue;
            }


        }


    }
}
