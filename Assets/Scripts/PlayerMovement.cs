using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    public GameObject Player;
    public Transform PlayerTransform;
    [SerializeField] Transform RespawnPoint;
    
    [SerializeField] private float runSpeed = 1500f;
    [SerializeField] float AirRunSpeed = 1200f;
    [SerializeField] private float JumpPower = 900f;

    

    public BoxCollider2D Box;
    public PolygonCollider2D Polygon;

    [SerializeField] ParticleSystem TrapEffect;
   
    
    public int TouchCountCheck = 0;

   
    CanvasControl canvasControl;
    CollectableCoins collectableCoins;
    ScenesManager scenesManager;
    PlayerHealth playerHealth;
    TrapThorns trapThorns;
    BoosterPowerUp boosterPowerUp;
    LevelUp levelUp;
    PowerUps powerUps;
    TrapofEnemy trapofEnemy;
 

    public Rigidbody2D myRigidbody;
    public Animator CharacterAnimator;
    public Transform mytransform;
    private SpriteRenderer spriteRenderer;
    
   
   
   
    public bool isOutOfViewCamera = false;
    public  bool isRunning;
    public bool isGround;
   
    public bool isinAir;
    public bool isPress_A;
    public bool isPress_D;
    public bool isPress_Up;
    public bool isFacingRight;

    public bool isPressD_Ground = false;
    public bool isPressA_Ground = false;

    


   
    private int MaxJumping = 2;
    public int RemainingJumping;
    

    private void Awake()
    {
       
        canvasControl =  FindObjectOfType<CanvasControl>(); 
        scenesManager  = FindObjectOfType<ScenesManager>(); 
        playerHealth = FindObjectOfType<PlayerHealth>();  
        trapThorns = FindObjectOfType<TrapThorns>(); 
        boosterPowerUp = FindObjectOfType<BoosterPowerUp>();
        levelUp = FindObjectOfType<LevelUp>();
        trapofEnemy = FindObjectOfType<TrapofEnemy>();
        powerUps = FindObjectOfType<PowerUps>();
        
    }
    private void Start()
    {
        
        AirRunSpeed = 1200f;
        gameObject.transform.position = RespawnPoint.position;
        RemainingJumping = MaxJumping;
        isFacingRight = true;

        
        
        myRigidbody = gameObject.GetComponent<Rigidbody2D>();
        mytransform = GetComponent<Transform>(); 
        CharacterAnimator = GetComponent<Animator>();

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        

    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag == "CheckPoint")
        {
            TouchCountCheck++;
            scenesManager.istouchCheckPoint = true;
        }
        
        
        if(other.gameObject.tag == "Trap" && trapofEnemy.isActiveDefence || other.gameObject.tag == "Trap" && powerUps.DefenderEffect.isPlaying)
        {
            Debug.Log("IsDefence çaliştiği için fonk çikildi");
            return;
        }

        if (other.gameObject.tag == "Trap" && !trapofEnemy.isActiveDefence)
        {
           
           TrapEffect.Play();
           playerHealth.currenthealth--;
        }
        
        if (other.gameObject.tag == "Enemy" && boosterPowerUp.isBooster)
        {
           other.GetComponent<BoxCollider2D>().isTrigger = true;
        }

    }

    void OnTriggerStay2D(Collider2D other) 
    {
        if(other.gameObject.tag == "Trap" && trapofEnemy.isActiveDefence)
        {
            Debug.Log("IsDefence çaliştiği için fonk çikildi");
            return;
        }
       
       
        if (other.gameObject.tag == "Trap" && !trapofEnemy.isActiveDefence)
        {
           
           TrapEffect.Play();
           playerHealth.currenthealth--;
           canvasControl.TakingDamage();
        }
        
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.tag == "CheckPoint")
        {
            
            scenesManager.istouchCheckPoint = false;
        }
       
        if (other.gameObject.tag == "Trap")
        {
            
            TrapEffect.Stop();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Grounds")
        {
            isGround = true;
            RemainingJumping = 2;
          

        }
    }

     private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Grounds")
        {
            isGround = false;
        }

    }

     private void Update() 
    {
        RemainingJumping = Mathf.Clamp(RemainingJumping, 0, 2);
        
        if (isGround)
        {
            isPressD_Ground = false;
            isPressA_Ground = false;
        }

        if(boosterPowerUp.isBooster) { runSpeed = 2000f; }
        else { runSpeed = 1500f; }

        
        

       if(playerHealth.isAliving())
       {
          if (isPress_D)
          {
           
            RightDirection();

          }

          if (isPress_A)
          {
            LeftDirection();
            
          }

         

          if((isPress_D && !isGround))
          {
                isPressD_Ground = true;
                Move(Vector2.right,AirRunSpeed);
                FlipRight();
                CharacterAnimator.SetBool("jump", true);
                CharacterAnimator.SetBool("run", false);
                isRunning = true;
                isFacingRight = true;

          }

          if (isPress_A && !isGround)
          {
               isPressA_Ground = true;
               Move(Vector2.left,AirRunSpeed);
               FlipLeft();
               CharacterAnimator.SetBool("jump", true);
               CharacterAnimator.SetBool("run", false);
               isRunning = true;
               isFacingRight = false;
          }

          

       }

    
       
    }


    IEnumerator Count()
    {
        yield return new WaitForSeconds(2f);
        
        
    }

    void OnBecameInvisible() 
    {
        isOutOfViewCamera = true;
        Debug.Log("Karakter Kamera görüş alani dişinda" + gameObject.name);
    }
    
   

    void RightDirection()
    {
           if (levelUp.isFinish || isPressD_Ground)
           {
              return;
           }
           
            if(isGround)
            {
                Move(Vector2.right, runSpeed);
                FlipRight();
                CharacterAnimator.SetBool("run", true);
                CharacterAnimator.SetBool("idle", false);
                CharacterAnimator.SetBool("jump", false);
                isRunning = true;
                isFacingRight = true;


            }
               
                
           
            
          
       if(!playerHealth.isAliving())
        {
            return;
        }

    }

    void LeftDirection()
    {
          if (levelUp.isFinish || isPressA_Ground)
           {
              return;
           }
           
           
           if(isGround)
           {
                Move(Vector2.left, runSpeed);
                FlipLeft();
                CharacterAnimator.SetBool("run", true);
                CharacterAnimator.SetBool("idle", false);
                 CharacterAnimator.SetBool("jump", false);
                isRunning = true;
                isFacingRight = false;


           }
           
              
        
        if(!playerHealth.isAliving())
        {
            return;
        }


    }

    

   private void Move(Vector2 Direction,float Speed)
    {
        Vector2 playerVelocity = new Vector2(Direction.x * Speed * Time.fixedDeltaTime, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;
       

        if(playerHealth.currenthealth <= 0 || trapThorns.isTouchingthorn)
        {
            return;
        }
    }

    public void Jump()
    {
        if(levelUp.isFinish) {  return; }
        if (isGround && RemainingJumping == 2 || !isGround && RemainingJumping == 1)
        {
            
            RemainingJumping--;
            Vector2 playerVelocity = new Vector2(myRigidbody.linearVelocity.x, Vector2.up.y * JumpPower * Time.fixedDeltaTime);
            myRigidbody.linearVelocity = playerVelocity;
            CharacterAnimator.SetBool("idle", false);
            CharacterAnimator.SetBool("jump", true);

        }

        if(!playerHealth.isAliving() || (RemainingJumping == 0 && !isGround))
        {
            return;
        }

    }

    public void OnPress_W()
    {
        isPress_Up = true;
    }

    public void OnPressUp_W()
    {
        isPress_Up = false;
    }



    public void UpStop()
    {
        isPress_Up = false;
        CharacterAnimator.SetBool("idle", true);
        CharacterAnimator.SetBool("jump", false);
    }




   


    public void OnButtonDown_D()
    {
        isPress_D = true;
    }

   


    public void OnButtonUp_D()
    {
        isPress_D = false;
        StopRight();
       
    }

   



    public void StopRight()
    {
        myRigidbody.linearVelocity = Vector2.zero;
        CharacterAnimator.SetBool("idle", true);
        CharacterAnimator.SetBool("run", false);
        CharacterAnimator.SetBool("jump", false);

        isRunning = false;
        
       
           
    }

   
    public void OnButtonDown_A()
    {
        isPress_A = true;
    }

    public void OnButtonUp_A()
    {
        isPress_A = false;
        StopLeft();
       
    }

   


    public void StopLeft()
    {
        myRigidbody.linearVelocity = Vector2.zero;
        CharacterAnimator.SetBool("idle", true);
        CharacterAnimator.SetBool("run", false);
        CharacterAnimator.SetBool("jump", false);
        isRunning = false;
        
      
            
    }

    

 

    public void FlipRight()
    {
     
        if(mytransform.localScale.x < 0)
        {
             
           
            Vector3 scale = mytransform.localScale;
            scale.x *= -1;
            mytransform.localScale = scale;

           

        }

        if(playerHealth.currenthealth <= 0 || trapThorns.isTouchingthorn)
        {
            return;
        }

    }

    public void FlipLeft()
    {
        if (mytransform.localScale.x > 0)
        {
           

            Vector3 scale = mytransform.localScale;
            scale.x *= -1;
            mytransform.localScale = scale;
          

        }

        if(playerHealth.currenthealth <= 0 || trapThorns.isTouchingthorn)
        {
            return;
        }

    }

   
}

