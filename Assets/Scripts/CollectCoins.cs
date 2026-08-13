using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCoins : MonoBehaviour
{
    
    [SerializeField] GameObject CoinEffect;
    public GameObject coinToRemove;
    CanvasControl canvasControl;
    CollectableCoinManager collectableCoinManager;

    GameObject[] Coins;
    List<GameObject> CoinsList = new List<GameObject>();

    public bool isCollect;
    public int experienceValue;


    public CollectableCoins collectableCoins;

    private void Start()
    {
        canvasControl = FindObjectOfType<CanvasControl>();
        collectableCoinManager = FindObjectOfType<CollectableCoinManager>();

        Coins = GameObject.FindGameObjectsWithTag("CollectableCoins");
        CoinsList.AddRange(Coins);  
    }

   





    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "CollectableCoins")
        {
            coinToRemove = null; 
            GameObject effectToRemove = null;

            CoinEffect.transform.position = collision.transform.position;
            effectToRemove = Instantiate(CoinEffect, collision.transform.position, collision.transform.rotation);
           
            
            if(effectToRemove != null)
            {
                 Destroy(effectToRemove, 1f);
            }



            isCollect = true;

           
            foreach (var coin in CoinsList)
            {
                if (collision.gameObject.name == coin.name)
                {
                    coinToRemove = coin;
                    experienceValue = gameObject.GetComponent<CollectCoins>().collectableCoins.experience;
                    collectableCoinManager.EatedApple++;

                    
                    break;
                }
            }

            canvasControl.IncreaseExperience();

            if (coinToRemove != null)
            {
                
                collectableCoinManager.RemainApple = collectableCoinManager.TotalApple - collectableCoinManager.EatedApple;
                
                CoinsList.Remove(coinToRemove); 
                Destroy(coinToRemove);
            }

           


        }


    }

}
