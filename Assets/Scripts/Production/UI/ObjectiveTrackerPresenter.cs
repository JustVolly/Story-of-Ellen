using TMPro;
using UnityEngine;

public class ObjectiveTrackerPresenter : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;
    [SerializeField] private TextMeshProUGUI memoriesText;
    [SerializeField] private TextMeshProUGUI secretsText;
    [SerializeField] private TextMeshProUGUI bossText;

    private void OnEnable()
    {
        if (GameSession.Instance != null) GameSession.Instance.SessionChanged += Refresh;
        if (flow != null) flow.ObjectivesChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameSession.Instance != null) GameSession.Instance.SessionChanged -= Refresh;
        if (flow != null) flow.ObjectivesChanged -= Refresh;
    }

    private void Refresh()
    {
        GameSession session = GameSession.Instance;
        if (session == null || flow == null) return;
        if (memoriesText != null)
        {
            memoriesText.gameObject.SetActive(flow.RequiredMemories > 0);
            memoriesText.text = $"Memories {session.MemoryFragments}/{flow.RequiredMemories}";
        }

        if (secretsText != null)
        {
            secretsText.gameObject.SetActive(flow.RequiredSecrets > 0);
            secretsText.text = $"Secrets {session.SecretsFound}/{flow.RequiredSecrets}";
        }
        if (bossText != null)
        {
            bossText.gameObject.SetActive(flow.RequiresBossDefeat);
            bossText.text = flow.BossDefeated ? "Guardian Defeated" : "Defeat the Guardian";
        }
    }
}
