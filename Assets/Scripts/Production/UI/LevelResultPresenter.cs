using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// End-of-level state and navigation. The result freezes gameplay with unscaled
/// UI timing, then offers explicit Continue/Replay/Menu actions.
/// </summary>
public class LevelResultPresenter : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI memoriesText;
    [SerializeField] private TextMeshProUGUI secretsText;
    [SerializeField, Min(0f)] private float revealDelay = 1f;

    private Coroutine presentation;
    private bool resultShown;
    private bool ownsPause;

    private void Awake()
    {
        if (flow == null) flow = FindObjectOfType<LevelFlowController>();
        if (panel != null) panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (flow != null) flow.LevelCompleted += Present;
    }

    private void OnDisable()
    {
        if (flow != null) flow.LevelCompleted -= Present;
        if (presentation != null) StopCoroutine(presentation);
        presentation = null;
        // Also recover time scale if the presenter gets disabled unexpectedly.
        RestoreTimeScale();
    }

    private void Present(LevelResult result)
    {
        if (resultShown) return;
        resultShown = true;
        ownsPause = Time.timeScale > 0f;
        if (ownsPause) Time.timeScale = 0f;

        if (presentation != null) StopCoroutine(presentation);
        presentation = StartCoroutine(PresentRoutine(result));
    }

    private IEnumerator PresentRoutine(LevelResult result)
    {
        if (revealDelay > 0f)
            yield return new WaitForSecondsRealtime(revealDelay);

        if (rankText != null) rankText.text = result.Rank;
        if (timeText != null) timeText.text = "Time  " + result.CompletionTime.ToString("0.0") + "s";
        if (deathsText != null) deathsText.text = "Deaths  " + result.Deaths;
        if (memoriesText != null) memoriesText.text = "Memories  " + result.MemoriesFound + "/" + result.MemoriesTotal;
        if (secretsText != null) secretsText.text = "Secrets  " + result.SecretsFound;

        if (panel != null) panel.SetActive(true);
        presentation = null;
    }

    public void ContinueCampaign()
    {
        if (!resultShown) return;
        string current = SceneManager.GetActiveScene().name;
        string next = current == "OneScene" ? "TwoScene"
            : current == "TwoScene" ? "ThreeScene" : "StartingScene";
        TravelTo(next);
    }

    public void ReplayLevel()
    {
        if (!resultShown) return;
        TravelTo(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        if (!resultShown) return;
        TravelTo("StartingScene");
    }

    private void TravelTo(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("[Ellen Result] Next scene missing from Build Settings: " + sceneName, this);
            return;
        }

        RestoreTimeScale();
        SceneManager.LoadScene(sceneName);
    }

    private void RestoreTimeScale()
    {
        if (!ownsPause) return;
        Time.timeScale = 1f;
        ownsPause = false;
    }
}
