using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadAndPlayEffect : MonoBehaviour
{
   

    private float Collision = 0f;
    public float CurrentCollision;

    public bool isEffectPlayed = false;

    [SerializeField] ParticleSystem BirthParticle;
    
    
    

    public Transform RespawnPoint;
  
    public Transform Player;


    ScenesManager scenesManager;

 
private void Awake() 
    {
        scenesManager = FindObjectOfType<ScenesManager>(); 
    }
   void Start() 
   { 
        CurrentCollision = Collision;

   } 

   
   
    private void Update()
    {
        
        CurrentCollision = Mathf.Clamp(CurrentCollision, 0, 1);   

        if(CurrentCollision == 1 && !isEffectPlayed)
        {
            isRespawnPlayer();
            isEffectPlayed = true;
        }

    }

    

    public void isRespawnPlayer()
    {
       
                  BirthParticle.transform.position = RespawnPoint.position;
                  BirthParticle.Play();
                  
                  StartCoroutine(StopEffect(2.5f));
                 

                  isEffectPlayed = true;
                  
        

       
    }


    IEnumerator StopEffect(float second)
    {

        yield return new WaitForSeconds(second);
        
        BirthParticle.Stop();
        
        
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
          if (other.gameObject.tag == "Respawn")
        {
                CurrentCollision++;
                
        } 
    }


   
    void OnTriggerExit2D(Collider2D other) 
    {
          if (other.gameObject.tag == "Respawn")
        {
                CurrentCollision--;
                other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        } 

    }

   
}
