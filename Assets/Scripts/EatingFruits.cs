using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatingFruits : MonoBehaviour
{
    PlayerHealth playerHealth;
    CanvasControl canvasControl;
   
  
    private GameObject DestroyStarEffect;
    [SerializeField] ParticleSystem HealingStarEffect;
    private GameObject DestroyHealingEffect;
    public bool isEating = false;


    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        canvasControl = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasControl>();

       
        HealingStarEffect.Stop();  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "StrawberryRB")
        {
            int confinerhealth = Mathf.Clamp(playerHealth.currenthealth, 0, 3);
            playerHealth.currenthealth++;
            canvasControl.IncreaseHealth(); 

            
            
            StartCoroutine(WaitSecond());
            
            Destroy(collision.gameObject);
            isEating = true;  

           
            
            
           if(playerHealth.currenthealth < 3)
           {
                playerHealth.currenthealth++;

           }

           else
           {
                playerHealth.currenthealth = confinerhealth;
           }

        }
    }


    IEnumerator WaitSecond()
    {
       
       yield return new WaitForSeconds(0.5f);
       HealingStarEffect.Play();
       yield return new WaitForSeconds(3f);
       HealingStarEffect.Stop();
       
    }
}
