using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject Bullet;
     [SerializeField] ParticleSystem BulletParticle;
    GameObject DestroyableBullet;
    
    GameObject DestroyableBulletEffect;
    public Transform FirePoint;
    public float BulletForcing = 7f;
    public int AllBullet = 5;
    public int CurrentBullet;
    public int NumberConfinerofBullet = 5;
    SpriteRenderer CharacterSprite;
   

    PlayerMovement playerMovement;
    CanvasControl canvasControl;
    LevelUp levelUp;
   
    private void Start() 
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        CanvasControl canvasControl = FindObjectOfType<CanvasControl>();
        levelUp = FindObjectOfType<LevelUp>();
        BulletParticle.Stop();
        CurrentBullet = AllBullet;
       
       

       
    
    }
    void Update() 
    {
      CurrentBullet  =  Mathf.Clamp(CurrentBullet, 0, NumberConfinerofBullet);
    }

   

public void AttackStart()
{
   if(levelUp.isFinish) {  return; }
   if(CurrentBullet == 0)
    {
         return;
    }
   
    if (playerMovement.isFacingRight)
    {
        CurrentBullet--;
        

        if (Bullet != null && BulletParticle != null)
         {
            DestroyableBullet = Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
            BulletParticle.Play();
           
         }
        
        else
         {
               Debug.LogError("Bullet veya BulletEffect değişkeni atanmamiş Right!");
         } 

        Rigidbody2D rb = DestroyableBullet.GetComponent<Rigidbody2D>(); 
        rb.AddForce(-Vector2.left * BulletForcing, ForceMode2D.Impulse);

        Destroy(DestroyableBullet, 4f);
        
    }
    else
    {
        CurrentBullet--;
        
     if (Bullet != null && BulletParticle != null)
         {
            DestroyableBullet = Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
            BulletParticle.Play();
            
         }
        
        else
         {
               Debug.LogError("Bullet veya BulletEffect değişkeni atanmamiş Left!");
         }   
        
        Rigidbody2D rb = DestroyableBullet.GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.left * BulletForcing, ForceMode2D.Impulse);

        Destroy(DestroyableBullet, 4f);
        
    }

}

}
