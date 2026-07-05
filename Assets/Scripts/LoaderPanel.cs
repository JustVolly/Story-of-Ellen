using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderPanel : MonoBehaviour
{

    public GameObject loadingScreen;
    public Image loadingBar;
    public TextMeshProUGUI loadingText;

    private void Start() 
    {
       loadingScreen.SetActive(false); 
       loadingBar.fillAmount = 0;
       loadingText.text = 0.ToString();
        
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsyncScene(sceneName));
    }

    private IEnumerator LoadAsyncScene(string sceneName)
    {
        loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            loadingBar.fillAmount = progress * Time.deltaTime;
            int percent = (int)(progress * 100f);
            loadingText.text = percent.ToString();

            yield return null;
        }

        loadingScreen.SetActive(false);
    }
}
