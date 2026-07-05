using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Plants;

public class BulletDamage : MonoBehaviour
{
    public bool istakingDamagePlant;
    public bool isDefenceSupport;
    [Range(0,60)]
    public int BlinkingTime = 0;
    public float blinkDuration = 2.3f;
    public float blinkTime = 0.15f;
    public float elapsedTime = 0.0f;

    SpriteRenderer CharacterSprite;
    private Coroutine Blinking;
    
    
    
    

    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    CanvasControl canvasControl;
    DefencePowerUp defencePowerUp;
    PowerUps powerUps;
    Plant plant;
    DamageBlinking damageBlinking;
    TrapofEnemy trapofEnemy;
    

    private void Start()
    {
        CharacterSprite = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        canvasControl = FindObjectOfType<CanvasControl>(); 
        defencePowerUp = FindObjectOfType<DefencePowerUp>();
        powerUps = FindObjectOfType<PowerUps>(); 
        plant = FindObjectOfType<Plant>();
        damageBlinking = FindObjectOfType<DamageBlinking>();    
        playerMovement = FindObjectOfType<PlayerMovement>();    
        trapofEnemy = FindObjectOfType<TrapofEnemy>();  
    }

   


    public IEnumerator BlinkEffect()
    {
       
        while (elapsedTime < blinkDuration)
        {
            CharacterSprite.color = Color.red;
            yield return new WaitForSeconds(blinkTime);
            CharacterSprite.color = Color.white;
            yield return new WaitForSeconds(blinkTime);
            

            elapsedTime += 2.3f * blinkTime;

        }

        CharacterSprite.color = Color.white;
       

    }

  

    



    private void OnTriggerEnter2D(Collider2D collision)
    {
        
      
       
       
       
        if (collision.gameObject.tag == "Player" && (trapofEnemy.isActiveDefence || powerUps.DefenderEffect.isPlaying))
        {
            
            isDefenceSupport = true;
            Debug.Log("Mermi karaktere değdi");

         if(plant.DestroyableBullet != null)
          {
                  Rigidbody2D BulletRb =   plant.DestroyableBullet.GetComponent<Rigidbody2D>();
                  BulletRb.gravityScale = 4.5f;
                  Debug.Log("Gravity Scale Bullet çalişti ve fonksiyondan çikildi");
                  return;
                  
          } 

        } 

        
        
        if(collision.gameObject.tag == "Player" && (!trapofEnemy.isActiveDefence || !powerUps.DefenderEffect.isPlaying))
             {
              StartCoroutine(BlinkEffect());
              istakingDamagePlant = true;
              playerHealth.DecreaseHealth();
              canvasControl.TakingDamage();
              Destroy(gameObject,4f);

             } 
               
      

    }

}
