using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerHealth : MonoBehaviour
{
    
    PlayerMovement playerMovement;
    BulletDamage bulletDamage;

    private int health = 3;
    public int currenthealth;
    public bool isAlive = true;
  

  

    private Rigidbody2D PlayerRigid;
    private SpriteRenderer CharacterSprite;
    public CompositeCollider2D CompositeCollider;
    private BoxCollider2D PlayerBoxCollider;
    private CapsuleCollider2D PlayerCapsuleCollider;
    private int JumpForce = 500;
    [SerializeField] float GravityScale;
   
   void Awake() 
   {
      PlayerRigid = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
      PlayerBoxCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>();
      PlayerCapsuleCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<CapsuleCollider2D>();
      bulletDamage = FindObjectOfType<BulletDamage>();
      CharacterSprite = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();

   }  

   void Start() 
   {
    currenthealth = health;
    playerMovement = FindObjectOfType<PlayerMovement>();

   } 

  

    private void Update()
    {
        
        currenthealth = Mathf.Clamp(currenthealth, 0, 3);
    
    
     if(currenthealth <= 0)
       {
           
           isAlive = false;
          
          

            playerMovement.CharacterAnimator.SetBool("fall", true);
            playerMovement.CharacterAnimator.SetBool("idle", false);



            CompositeCollider.isTrigger = true;
            PlayerRigid.gravityScale = GravityScale;
            PlayerBoxCollider.isTrigger = true;
            PlayerCapsuleCollider.isTrigger = true;

        } 
       
    }

    
    public void DecreaseHealth()
    {
        currenthealth--;
        
    }

    public bool isAliving()
    {
        return isAlive;
    }


   

   
}
