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
    }

    private void Present(LevelResult result)
    {
        if (panel != null) panel.SetActive(true);
        if (rankText != null) rankText.text = result.Rank;
        if (timeText != null) timeText.text = result.CompletionTime.ToString("0.0") + "s";
        if (deathsText != null) deathsText.text = result.Deaths.ToString();
        if (memoriesText != null) memoriesText.text = result.MemoriesFound + "/" + result.MemoriesTotal;
        if (secretsText != null) secretsText.text = result.SecretsFound.ToString();
    }
}
