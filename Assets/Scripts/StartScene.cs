using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
  public bool isPlay = false;
  public bool isPressHome;
  public bool isPlayingSound = true;
  public bool isPressSoundButton = false;

  public Image SoundsImage;
  public Sprite Silent;
  public Sprite Sound;


  [SerializeField] AudioClip[] Clips; 
  public AudioSource Audio;
  

  private bool isPressAudioControl;
  

  

  LoaderPanel loaderPanel;
  ScenesManager scenesManager;

  private void Start() 
  {
     loaderPanel = FindObjectOfType<LoaderPanel>();
     scenesManager = FindObjectOfType<ScenesManager>();
    
     
     if (SceneManager.GetActiveScene().buildIndex == 0)
     {
        Audio.clip = Clips[0];
        Audio.Play();
     }

     if (SceneManager.GetActiveScene().buildIndex == 1 || SceneManager.GetActiveScene().buildIndex == 2)
     {
        Audio.clip = Clips[1];
        Audio.Play();
     }

     
     
     
     
  }

  private void Update() 
  {
    if(SceneManager.GetActiveScene().buildIndex == 1 || SceneManager.GetActiveScene().buildIndex == 2)
            {
                if (!Audio.isPlaying && !isPressAudioControl)
                {
                    Audio.clip = Clips[1];
                    Audio.Play();
                }

            }

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                if (!Audio.isPlaying && !isPressAudioControl)
                {
                    Audio.clip = Clips[0];
                    Audio.Play();
                }
            }


  }

  
public void PressAudioControl()
{
   
   if (SceneManager.GetActiveScene().buildIndex == 0)
   {
        isPressAudioControl = true;
         
        if (isPlayingSound)
        {
            Audio.Stop();
            SoundsImage.sprite = Silent;
            isPlayingSound = false;
        }

        else
        {
            Audio.Play();
            SoundsImage.sprite = Sound;
            isPlayingSound = true;
            isPressAudioControl = false;
        }
        
   }

   if (SceneManager.GetActiveScene().buildIndex == 1 || SceneManager.GetActiveScene().buildIndex == 2)
   {
        isPressAudioControl = true;
         
        if (isPlayingSound)
        {
            Audio.Stop();
            SoundsImage.sprite = Silent;
            isPlayingSound = false;
        }

        else
        {
            Audio.Play();
            SoundsImage.sprite = Sound;
            isPlayingSound = true;
            isPressAudioControl = false;
        }
        
   }

}

public void UpPressAudioControl()
{
      isPressAudioControl = false;

}




    
public void StartGame()
{
    if (!isPlay)
    {
        isPlay = true;

        loaderPanel.LoadScene("OneScene");

        if (loaderPanel.loadingBar.fillAmount == 1)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                  SceneManager.LoadScene(1);
                  Time.timeScale = 1f;
            }

        }

        
    }
}
    public void Quit()
    {
        Application.Quit();
    }
}
