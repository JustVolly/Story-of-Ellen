using System.Collections;
using TMPro;
using UnityEngine;

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

    private void Awake()
    {
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
    }

    private void Present(LevelResult result)
    {
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
}
