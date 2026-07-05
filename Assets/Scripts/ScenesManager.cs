using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;



public class ScenesManager : MonoBehaviour
{
    
    Transform EmptyTransform;
    private string[] sceneName = new string[5];
    [SerializeField] ParticleSystem BirthParticle;
    [SerializeField] Button[] Buttons;

    public GameObject EmptyCheckPoint;
    public GameObject FlexiblePoint;
    private GameObject DestroyFlexiblePoint;
    GameObject DestroyCheckPoint;
    public bool istouchCheckPoint = false;
    public bool ispressed_A = false;
    public bool isTurnedBack_A = false;
    public int SetCheckPointCount = 0;

    public Vector2 StartPos;
    public Vector2 FinisPos;
    public Vector2 PlayerWorldPosition;
    Quaternion PlayerWorldRotation;
    
    private Rigidbody2D PlayerRigid;
    private int ActiveScenebuildIndex;
    public float TotalDistance = 0;
    public float totalDistance = 0;

    public float CurrentDistance;
    public float currentDistance;
    
    
    public float LastCurrentDistance = 0;
   
    public  float minCurrentDistance;
    public Slider PercentSlider;
    public TextMeshProUGUI Percent;
    public float percentage = 0;
    private int lastAssignedPercentage = 0;
    public int textvalue;
    public float Difference;
    public float dif = 0;
   

    public bool isPressStopButton = false;
    private bool isSetLostPanel = false;
    

  
   
    

    public Transform respawnPoint;
    public Transform player;

    Scene ActiveScene;

   [SerializeField] GameObject LostPanel;
   [SerializeField] GameObject WinPanel;
   [SerializeField] GameObject StopPanel;

    public bool isbackpressHomeButton;
    private bool isTouchingFinish;
    public bool isRespawn;
    public bool isRespawnOver;

   

    [SerializeField] float WaitEffectTime = 1f;
    [SerializeField] float WaitScene = 2f;

   


    TrapofEnemy trapofEnemy;
    TrapThorns trapThorns;
    PlayerMovement playerMovement;
    PlayerHealth playerHealth;
    LevelUp levelUp;
    ReloadAndPlayEffect reloadAndPlayEffect;
    StartScene startScene;
    CanvasControl canvascontrol;
    EnemyDamage enemyDamage;
   
    private void Awake()
    {
        trapThorns = FindObjectOfType<TrapThorns>();    
        trapofEnemy = FindObjectOfType<TrapofEnemy>();
        playerMovement = FindObjectOfType<PlayerMovement>();    
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>(); 
        levelUp = FindObjectOfType<LevelUp>();  
        reloadAndPlayEffect = FindObjectOfType<ReloadAndPlayEffect>(); 
        startScene = FindObjectOfType<StartScene>(); 
        canvascontrol = FindObjectOfType<CanvasControl>();
        enemyDamage = FindObjectOfType<EnemyDamage>();  
        
        
        isbackpressHomeButton = false; 
        isTouchingFinish = false;

    }

   
    void Start()
    {
       PlayerRigid = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        
        sceneName[0] = "StartingScene";
        sceneName[1] = "OneScene";
        sceneName[2] = "TwoScene";
        sceneName[3] = "ThreeScene";
        sceneName[4] = "FourScene";

        BirthParticle.Play();
        StartCoroutine(WaitingBirthParticle());

        LostPanel.SetActive(false);
        WinPanel.SetActive(false);
        StopPanel.SetActive(false);

        player.position = respawnPoint.position;
        StartPos = respawnPoint.position;

            
           

        TotalDistance = Vector2.Distance(StartPos,FinisPos);
        totalDistance = TotalDistance / 10;
        CurrentDistance = Vector2.Distance(playerMovement.mytransform.position, FinisPos);
        currentDistance = CurrentDistance / 10;
         Difference = TotalDistance - CurrentDistance;

      
       
       
        Percent.text = 0.ToString() + "%";
        PercentSlider.value = 0;
        PercentSlider.maxValue = TotalDistance;
        
    }

    private void Update()
    {
        
        if (!playerMovement.isPress_A && playerMovement.isPress_D)
        {
            CurrentDistance = Vector2.Distance(playerMovement.transform.position,FinisPos);
            currentDistance = CurrentDistance / 10;
        }
        
        
        CurrentDistance = Mathf.Clamp(CurrentDistance, 0, TotalDistance);
       
        if (startScene.isPlay || !isPressStopButton)
        {
            Time.timeScale = 1f;
        }

        AnimationControl();
        SetLostPanel();
        CheckActiveScene();
        StoppingBirth();
        CalculateDistance();
        

    if (!ispressed_A && playerMovement.isPress_A)
    {
               Debug.Log("Collider oluştu");
               ispressed_A = true;
               Transform PlayerTransform = playerMovement.Player.transform;
               PlayerWorldPosition = PlayerTransform.position;
               PlayerWorldRotation = PlayerTransform.rotation;
               DestroyCheckPoint = Instantiate(EmptyCheckPoint,PlayerWorldPosition,PlayerWorldRotation);
    }      
        
        
        
        if (levelUp.isFinish)
        {
            Buttons[0].interactable = false;
            Buttons[1].interactable = false;
            Buttons[2].interactable = false; 
            Buttons[3].interactable = false;

        }
        

    }


public void CheckPressed_A_Down()
{
      isTurnedBack_A = true;

}

public void CheckPressed_A_Up()
{
      isTurnedBack_A = false; 

}




public void CalculateDistance()
{
     if (DestroyCheckPoint != null && DestroyCheckPoint.gameObject.activeInHierarchy
      && !istouchCheckPoint && (playerMovement.isPress_D || playerMovement.isPress_A))
    {
       Debug.Log("Return 1");
       LastCurrentDistance = CurrentDistance;
       return;
    }

    if (DestroyCheckPoint != null && DestroyCheckPoint.gameObject.activeInHierarchy
      && !istouchCheckPoint && (playerMovement.isPress_D || playerMovement.isPress_A))
    {
        Debug.Log("Return 2");
        LastCurrentDistance = CurrentDistance;
        return;
    }
    
    
    
    if (playerMovement.isPress_D && !playerMovement.isPress_A)
    {
        if (!istouchCheckPoint)
        {
            Debug.Log("Çalişiyor");
            dif = totalDistance - currentDistance;


             percentage = 100 * dif / totalDistance;
             percentage = Mathf.Clamp(percentage, 0, 100);


             PercentSlider.value = Mathf.Clamp(PercentSlider.value + percentage / 100, 0, 100);


             Percent.text = Mathf.RoundToInt(PercentSlider.value / (totalDistance / 100)).ToString() + "%";
        }
        
        

        if (istouchCheckPoint && playerMovement.TouchCountCheck > 1)
        {
            playerMovement.TouchCountCheck = 0;
            Destroy(DestroyCheckPoint);
            ispressed_A = false;

            
            CurrentDistance = LastCurrentDistance;
            dif = totalDistance - currentDistance;


            percentage = 100 * dif / totalDistance;
            percentage = Mathf.Clamp(percentage, 0, 100);


            PercentSlider.value = Mathf.Clamp(PercentSlider.value + percentage / 100, 0, 100);


            Percent.text = Mathf.RoundToInt(PercentSlider.value / (totalDistance / 100)).ToString() + "%";
            
            if (playerMovement.isPress_A || playerMovement.isPress_Up)
            {
                PercentSlider.value = lastAssignedPercentage;
            }
        }
    }
}

    

    public void StoppingBirth()
    {
        if (BirthParticle.isPlaying)
        {
            if (PlayerRigid.linearVelocity.x > 0.2f || PlayerRigid.linearVelocity.y > 0.2f)
             { 
                 BirthParticle.Stop();

             }

        }
        
    }


    public int CheckActiveScene()
    {

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
           
            ActiveScene = SceneManager.GetActiveScene();
            ActiveScenebuildIndex = 0;
            return ActiveScenebuildIndex;
           
          
        }

       else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
           
            ActiveScene = SceneManager.GetActiveScene();
            ActiveScenebuildIndex = 1;
            return ActiveScenebuildIndex;
               
             
        }

       else if(SceneManager.GetActiveScene().buildIndex == 2)
        {
           ActiveScene = SceneManager.GetActiveScene();
           ActiveScenebuildIndex = 2;
           return ActiveScenebuildIndex;
            //seviye2
        }
        
        else
        {

            return 3;
            
        }
        

    }

    public void SetLostPanel()
    {
        if (playerHealth.currenthealth <= 0)
        {
            LostPanel.gameObject.SetActive(true);
            isSetLostPanel = true;
           
          
        }
        else
        {
            LostPanel.gameObject.SetActive(false);
            isSetLostPanel = false;
         
        }

        
    }

    

    public void SetWinPanel()
    {
        if (levelUp.isFinish)
        {
            
            WinPanel.SetActive(true);
         
        }

        else
        {
            WinPanel.SetActive(false);
            
        }

    }

    public void AnimationControl()
    {
         if (canvascontrol.CurrentTime <= 0 || playerHealth.currenthealth <= 0)
        {
           Animator PlayerAnim = GameObject.FindWithTag("Player").GetComponent<Animator>();
           CompositeCollider2D GroundComposite = GameObject.FindWithTag("Grounds").GetComponent<CompositeCollider2D>();
           
           if (PlayerAnim.GetBool("idle"))
           {
               PlayerAnim.SetBool("idle",false);
               PlayerAnim.SetBool("fall",true);
               

           } 

           else if (PlayerAnim.GetBool("run"))
           {
               PlayerAnim.SetBool("run",false);
               PlayerAnim.SetBool("fall",true);
               
           }

           else if(PlayerAnim.GetBool("jump"))
           {
               PlayerAnim.SetBool("jump",false);
               PlayerAnim.SetBool("fall",true);
           }

           else
           {
             return;
           }
        }
    }

   public void Respawn()
{
    Time.timeScale = 1f;
    levelUp.isFinish = false;
    enemyDamage.TouchCount = 0;

    
    if (playerHealth.currenthealth <= 0 || playerHealth.currenthealth > 0 || trapThorns.isTouchingthorn || levelUp.isFinish)
    {
        isRespawn = true;
        levelUp.isFinish = false;
        playerHealth.currenthealth = 3;
       
        

       
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        
        playerMovement.StopRight(); 
        playerMovement.StopLeft(); 

       
        playerMovement.CharacterAnimator.SetBool("idle", true);
        playerMovement.CharacterAnimator.SetBool("run", false);
        playerMovement.CharacterAnimator.SetBool("jump", false);

        canvascontrol.Star1.fillAmount = 1f;
        canvascontrol.Star2.fillAmount = 1f;
        canvascontrol.Star3.fillAmount = 1f;


        player.rotation = respawnPoint.rotation;
        player.position = respawnPoint.position;

       
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (SceneManager.GetActiveScene().isLoaded)
        {
             BirthParticle.Play();
             StartCoroutine(WaitingBirthParticle());
        }
    }
}

IEnumerator WaitingBirthParticle()
{
    yield return new WaitForSeconds(2.5f);
    BirthParticle.Stop();
}


    public void GetBackStartScreen()
    {

        SceneManager.LoadScene(0);
        
        
    }

    public void SetNextLevel()
    {
        Time.timeScale = 1f;
        levelUp.isFinish = false;

        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            canvascontrol.Star1.fillAmount = 1f;
            canvascontrol.Star2.fillAmount = 1f;
            canvascontrol.Star3.fillAmount = 1f;
            
            SceneManager.LoadScene(sceneName[2]);

        }

        else
        {
            return;
        }

        
        
    }

    public void StopGame()
    {
          
          if (levelUp.isFinish || isSetLostPanel)
          {
            return;
          }

          isPressStopButton = true;
         
          if (SceneManager.GetActiveScene().buildIndex == 1 || SceneManager.GetActiveScene().buildIndex == 2 )
          {
            if(!isbackpressHomeButton)
            {
                 StopPanel.SetActive(true);
                 Time.timeScale = 0f;
              
            }

            else
            {
                 StopPanel.SetActive(false);
                 Time.timeScale = 1f;
            }
          }

          
          
          

    }

    public void ResumeGame()
    {
         Time.timeScale = 1f;
         StopPanel.SetActive(false);

    }

    public void StopRespawn()
    {
        Time.timeScale = 1f;
        playerHealth.currenthealth = 3;
          
        player.rotation = respawnPoint.rotation;
        player.position = respawnPoint.position;
            
           

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
           

    } 

}

   