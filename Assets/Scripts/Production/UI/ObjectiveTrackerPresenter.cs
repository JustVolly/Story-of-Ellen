using TMPro;
using UnityEngine;

public class ObjectiveTrackerPresenter : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;
    [SerializeField] private TextMeshProUGUI memoriesText;
    [SerializeField] private TextMeshProUGUI secretsText;
    [SerializeField] private TextMeshProUGUI bossText;
    [SerializeField] private GameObject root;

    private GameSession session;

    private void Awake()
    {
        if (flow == null) flow = FindObjectOfType<LevelFlowController>();
    }

    private void OnEnable()
    {
        if (flow != null) flow.ObjectivesChanged += Refresh;
        BindSession();
    }

    private void Start()
    {
        BindSession();
        Refresh();
    }

    private void OnDisable()
    {
        if (session != null) session.SessionChanged -= Refresh;
        if (flow != null) flow.ObjectivesChanged -= Refresh;
        session = null;
    }

    private void BindSession()
    {
        if (session == GameSession.Instance) return;

        if (session != null) session.SessionChanged -= Refresh;
        session = GameSession.Instance;
        if (session != null) session.SessionChanged += Refresh;
    }

    private void Refresh()
    {
        if (session == null) BindSession();
        if (session == null || flow == null) return;

        bool hasObjectives = flow.RequiredMemories > 0 || flow.RequiredSecrets > 0 || flow.RequiresBossDefeat;
        if (root != null) root.SetActive(hasObjectives);

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
