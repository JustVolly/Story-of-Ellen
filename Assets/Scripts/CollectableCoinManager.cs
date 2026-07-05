using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectableCoinManager : MonoBehaviour
{
    public int TotalApple;
    public int EatedApple = 0;
    public int RemainApple;
    

    [SerializeField] Image[] Stars;
    public int StarOnePer = 0;
    public int StarTwoPer = 0;
    public int StarThreePer = 0;

    

    LevelUp levelUp;

    CollectCoins collectCoins;
    ScenesManager scenesManager;

    private void Start() 
    {
        Stars[0].fillAmount = 0;
        Stars[1].fillAmount = 0;
        Stars[2].fillAmount = 0;

        
        
        collectCoins = FindObjectOfType<CollectCoins>();
        TotalApple = GameObject.FindGameObjectsWithTag("CollectableCoins").Length;
        levelUp = FindObjectOfType<LevelUp>();
        scenesManager = FindObjectOfType<ScenesManager>();
    }

    void Update() 
    {
        if (levelUp.isSetWin)
        {
             int amount = TotalApple / 3;
             
             if (Stars[0].fillAmount == 0 && Stars[1].fillAmount == 0 && Stars[2].fillAmount == 0 )
             {
                Stars[0].fillAmount = (((100 * EatedApple) / amount) / (100) * Time.deltaTime);
             }

             if (Stars[0].fillAmount == 1 && Stars[1].fillAmount == 0 && Stars[2].fillAmount == 0 )
             {
                Stars[1].fillAmount = (((100 * EatedApple) / amount) / (100) * Time.deltaTime);
             }

             if (Stars[0].fillAmount == 1 && Stars[1].fillAmount == 1 && Stars[2].fillAmount == 0 )
             {
                Stars[2].fillAmount = (((100 * EatedApple) / amount) / (100) * Time.deltaTime);
             }


        }
        
       
        RemainApple = TotalApple - EatedApple;
    }

    public int CountPercentageApple()
    {
         int percentage = 0;
        
         percentage = (EatedApple * 100) / TotalApple;

         return percentage;
          
    }
}
