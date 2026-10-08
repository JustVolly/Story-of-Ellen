using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderPanel : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image loadingBar;
    public TextMeshProUGUI loadingText;

    [SerializeField, Min(0f)] private float minimumVisibleTime = 0.35f;
    [SerializeField, Min(0.1f)] private float progressLerpSpeed = 2.5f;

    private bool loading;

    private void Start()
    {
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (loadingBar != null) loadingBar.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0";
    }

    public void LoadScene(string sceneName)
    {
        if (loading || string.IsNullOrWhiteSpace(sceneName)) return;
        StartCoroutine(LoadAsyncScene(sceneName));
    }

    private IEnumerator LoadAsyncScene(string sceneName)
    {
        loading = true;
        if (loadingScreen != null) loadingScreen.SetActive(true);
        if (loadingBar != null) loadingBar.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0";

        float startedAt = Time.realtimeSinceStartup;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        if (asyncLoad == null)
        {
            loading = false;
            yield break;
        }

        asyncLoad.allowSceneActivation = false;
        float displayedProgress = 0f;

        while (asyncLoad.progress < 0.9f)
        {
            float target = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                target,
                progressLerpSpeed * Time.unscaledDeltaTime);

            UpdateProgress(displayedProgress);
            yield return null;
        }

        while (displayedProgress < 1f)
        {
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                1f,
                progressLerpSpeed * Time.unscaledDeltaTime);

            UpdateProgress(displayedProgress);
            yield return null;
        }

        float remaining = minimumVisibleTime - (Time.realtimeSinceStartup - startedAt);
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        UpdateProgress(1f);
        asyncLoad.allowSceneActivation = true;
    }

    private void UpdateProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (loadingBar != null) loadingBar.fillAmount = progress;
        if (loadingText != null) loadingText.text = Mathf.RoundToInt(progress * 100f).ToString();
    }
}
