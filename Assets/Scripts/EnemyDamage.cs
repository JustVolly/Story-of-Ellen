using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public ParticleSystem EnemyEffect;
    public Transform EffectPos;
    
    public bool isTouchingEnemy = false;
    public bool isPlaySnailHitted;

    public int TouchCount = 0;

    
    PlayerHealth playerHealth;
    CanvasControl canvasControl;
    DefencePowerUp defencePowerUp;
    PlayerBulletDamage playerBulletDamage;
    BulletDamage bulletDamage;
    PowerUps powerUps;

    

    private void Start() 
    {
        TouchCount = 0;
        playerHealth = FindObjectOfType<PlayerHealth>();    
        canvasControl = FindObjectOfType<CanvasControl>();
        defencePowerUp = FindObjectOfType<DefencePowerUp>();  
        playerBulletDamage = FindObjectOfType<PlayerBulletDamage>();
        bulletDamage = FindObjectOfType<BulletDamage>();
        powerUps = FindObjectOfType<PowerUps>();



    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
      if(collision.gameObject.tag == "Player" && (defencePowerUp.isDefence || powerUps.DefenderEffect.isPlaying || isPlaySnailHitted))
       {
          return;
       } 
           
          
           
        if(collision.gameObject.tag == "Player")
        {
             isTouchingEnemy = true;
             if (playerHealth.TakeDamage())
             {
                 StartCoroutine(bulletDamage.BlinkEffect());
                 EnemyEffect.transform.position = EffectPos.position;
                 EnemyEffect.Play();
             }
        }
           

        

    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.gameObject.tag == "Player" && isTouchingEnemy)
       {
            isTouchingEnemy = false;      
           
       }
    }


    

}
