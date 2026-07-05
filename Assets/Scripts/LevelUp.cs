using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
   ScenesManager scenesManager;
   PlayerMovement playerMovement;

   [SerializeField] ParticleSystem[] FireWorks;
   //[SerializeField] Image[] WinStars;
   [SerializeField] GameObject SetWinPanel;
   public float Forcing = 1400f;
   [SerializeField] Rigidbody2D PlayerRigid;
   [SerializeField] Animator PlayerAnim;
   


   public bool isSetWin;
   public bool isFinish;

   private void Start() 
   {
        scenesManager = FindObjectOfType<ScenesManager>();  
        playerMovement = FindObjectOfType<PlayerMovement>();  
        SetWinPanel.SetActive(false);
       

   }

   void Update() 
   {

    if (isSetWin)
    {
        PlayerAnim.Play("Jump_Mask",0,0f);
        if (playerMovement.isGround)
        {
            PlayerRigid.linearVelocity = new Vector2(playerMovement.myRigidbody.linearVelocity.x, Vector2.up.y * Forcing * Time.deltaTime);
            PlayerAnim.Play("İdle_Mask",0,0f);
            Debug.Log("Çalişiyor");
        }

       
        
    }
    
   }



   void OnTriggerEnter2D(Collider2D other) 
   {
        if(other.gameObject.tag == "Player")
        {
            isSetWin = true;
            Debug.Log("Ağaca dokundu");
            isFinish = true;

            FireWorks[0].Play();
            FireWorks[1].Play();


            StartCoroutine(Win());
           
            

        }

        else
        {
            Debug.Log("Ağaca dokunmuyor");
            isSetWin = false;
        }

     
   }

   void OnTriggerStay2D(Collider2D other) 
   {
          if(other.gameObject.tag == "Player")
          {
              isSetWin = true;

          }
   }

   IEnumerator Win()
        {
            yield return new WaitForSeconds(6f);
            
            SetWinPanel.SetActive(true);
            FireWorks[0].Stop();
            FireWorks[1].Stop();
            isSetWin = false;
            
            
        }
}
