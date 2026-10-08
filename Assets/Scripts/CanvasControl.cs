using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CanvasControl : MonoBehaviour
{
   public int GravityForPlayer = 1;
   public float WaitAnimationDuration = 1.5f;

   
    TextMeshProUGUI  AppleExperience;
    TextMeshProUGUI BulletStrawberry;

    [SerializeField] TextMeshProUGUI Times;
    [SerializeField] Image ClockFire;

    [Header("Time")]
    [SerializeField] int TotalTime = 180;
    [SerializeField] int TimeSpeed = 1;
    public float Timer = 1f;
    public float DecreaseTimerFillAmount;
    public float DecreaseSpeed = 10f;
    public int CurrentTime;
    
    [Header("Health Stars")]
    public Image Star1;
    public Image Star2;
    public Image Star3;

    



    [SerializeField] Button Left;
    [SerializeField] Button Right;
    [SerializeField] Button Up;
    [SerializeField] Button Fire;
    [SerializeField] Button Stop;

    

  

    

    


    int exp_score = 0;
    int bullet_fired = 0;


    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    TrapofEnemy trapofEnemy;
    EatingFruits eatingFruits;
    CollectCoins collectCoins;
    TrapThorns trapThorns;
    ScenesManager scenesManager;
    BulletDamage bulletDamage;

    EnemyDamage enemyDamage;

    CharacterAttack characterAttack;
    LevelUp levelUp;


   void Awake() 
   {
            Star1.fillAmount = 1f;
            Star2.fillAmount = 1f;
            Star3.fillAmount = 1f;
   }
    void Start()
    {
        AppleExperience = GameObject.FindWithTag("StarExpUI").GetComponent<TextMeshProUGUI>();
        BulletStrawberry = GameObject.FindWithTag("BulletCount").GetComponent<TextMeshProUGUI>();
        Times = GameObject.FindWithTag("Tmer").GetComponent<TextMeshProUGUI>();
        ClockFire = GameObject.FindWithTag("ClockFire").GetComponent<Image>();

        

        CurrentTime = TotalTime;
        
        playerHealth = FindObjectOfType<PlayerHealth>();
        trapofEnemy = FindObjectOfType<TrapofEnemy>();
        eatingFruits = FindObjectOfType<EatingFruits>();
        collectCoins = FindObjectOfType<CollectCoins>();
        trapThorns = FindObjectOfType<TrapThorns>();
        playerMovement = FindObjectOfType<PlayerMovement>();  
        scenesManager = FindObjectOfType<ScenesManager>(); 
        bulletDamage = FindObjectOfType<BulletDamage>();  
        enemyDamage = FindObjectOfType<EnemyDamage>();
        characterAttack = FindObjectOfType<CharacterAttack>();
        levelUp = FindObjectOfType<LevelUp>();

         AppleExperience.text = 0.ToString();  
         BulletStrawberry.text = characterAttack.CurrentBullet.ToString(); 
         Times.text = CurrentTime.ToString();
         ClockFire.fillAmount = 1;

    }

    private void Update()
    {
       SyncHealthUI();
       DecreaseBullet(characterAttack.CurrentBullet);
       
       CanvasTimer();
      

        if(playerHealth.currenthealth <= 0)
        {
            Left.interactable = false;
            Right.interactable = false;
            Up.interactable = false;
            Fire.interactable = false;
            Stop.interactable = false;

        }

        
        
    }

    public void CanvasTimer()
    {
       if(playerHealth.currenthealth <= 0 || levelUp.isFinish || Time.timeScale == 0 || playerMovement.isOutOfViewCamera)
       {
         return;
       }
       
       if (!scenesManager.isPressStopButton && playerHealth.currenthealth > 0)
       {
           CurrentTime = Mathf.Clamp(CurrentTime, 0, TotalTime);
           Timer -= Time.deltaTime * TimeSpeed;
       }

       
       
       
       if (Timer <= 0f)
       {
          CurrentTime--;
          Times.text = CurrentTime.ToString();
    
   
          DecreaseTimerFillAmount = 1f / Mathf.Max(1, TotalTime);
          ClockFire.fillAmount = Mathf.Clamp01(ClockFire.fillAmount - DecreaseTimerFillAmount);

          Timer = 1f;
       }  

       if (CurrentTime == 0 || CurrentTime < 0)
       {
           GameObject.FindWithTag("Player").GetComponent<Animator>().SetBool("idle",false);
           GameObject.FindWithTag("Player").GetComponent<Animator>().SetBool("fall",true);
           StartCoroutine(WaitAnimation());
           

       }


    }

    IEnumerator WaitAnimation()
    {
        yield return new WaitForSeconds(WaitAnimationDuration);
        playerMovement.myRigidbody.gravityScale = GravityForPlayer;
        GameObject.FindWithTag("Player").GetComponent<CapsuleCollider2D>().isTrigger = true;
        GameObject.FindWithTag("Player").GetComponent<BoxCollider2D>().isTrigger = true;

    }

    public void DecreaseBullet(int amount)
    {
        bullet_fired = amount;
        
        BulletStrawberry.text = bullet_fired.ToString();

    }

    public void IncreaseBullet(int amount)
    {
         
        BulletStrawberry.text = amount.ToString();
    }


    public void IncreaseExperience()
    {
        exp_score += collectCoins.experienceValue;
        AppleExperience.text = exp_score.ToString();
    }

    public void IncreaseHealth()
    {
        if (eatingFruits.isEating)
        {
            playerHealth.Heal();
            SyncHealthUI();
        }
    }

    public void FillHealth()
    {
        if (scenesManager.isRespawn)
        {
            playerHealth.ResetHealth();
        }

        SyncHealthUI();
    }

    public void TakingDamage()
    {
        SyncHealthUI();
    }

    private void SyncHealthUI()
    {
        if (playerHealth == null)
        {
            return;
        }

        Star1.fillAmount = playerHealth.currenthealth >= 3 ? 1f : 0f;
        Star2.fillAmount = playerHealth.currenthealth >= 2 ? 1f : 0f;
        Star3.fillAmount = playerHealth.currenthealth >= 1 ? 1f : 0f;
    }

    public void DieImmediate()
    {
        
        if(trapThorns.isTouchingthorn)
        {
            Star1.fillAmount = 0;
            Star2.fillAmount = 0;
            Star3.fillAmount = 0;


        }


    }

    
}
