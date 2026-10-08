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
    private bool isBlinking;
    
    
    
    

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
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        canvasControl = FindAnyObjectByType<CanvasControl>(); 
        defencePowerUp = FindAnyObjectByType<DefencePowerUp>();
        powerUps = FindAnyObjectByType<PowerUps>(); 
        plant = FindAnyObjectByType<Plant>();
        damageBlinking = FindAnyObjectByType<DamageBlinking>();    
        playerMovement = FindAnyObjectByType<PlayerMovement>();    
        trapofEnemy = FindAnyObjectByType<TrapofEnemy>();  
    }

   


    public IEnumerator BlinkEffect()
    {
        if (isBlinking) yield break;
        isBlinking = true;
        elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            CharacterSprite.color = Color.red;
            yield return new WaitForSeconds(blinkTime);
            CharacterSprite.color = Color.white;
            yield return new WaitForSeconds(blinkTime);
            

            elapsedTime += 2.3f * blinkTime;

        }

        CharacterSprite.color = Color.white;
        elapsedTime = 0f;
        isBlinking = false;
    }

  

    



    private void OnTriggerEnter2D(Collider2D collision)
    {
        
      
       
       
       
        if (collision.CompareTag("Player") && ((trapofEnemy != null && trapofEnemy.isActiveDefence) || (powerUps != null && powerUps.IsActive)))
        {
            
            isDefenceSupport = true;
            Debug.Log("Mermi karaktere değdi");

         if(plant != null && plant.DestroyableBullet != null)
          {
                  Rigidbody2D BulletRb =   plant.DestroyableBullet.GetComponent<Rigidbody2D>();
                  BulletRb.gravityScale = 4.5f;
                  Debug.Log("Gravity Scale Bullet çalişti ve fonksiyondan çikildi");
                  return;
                  
          } 

        } 

        
        
        if (collision.CompareTag("Player") && (trapofEnemy == null || !trapofEnemy.isActiveDefence) && (powerUps == null || !powerUps.IsActive))
             {
              if (playerHealth.TakeDamage())
              {
                  StartCoroutine(BlinkEffect());
                  istakingDamagePlant = true;
              }
              Destroy(gameObject, 4f);

             } 
               
      

    }

}
