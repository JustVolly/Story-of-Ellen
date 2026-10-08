using UnityEngine;

public class ObjectiveBarrier : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;
    [SerializeField] private Collider2D blocker;
    [SerializeField] private GameObject visualRoot;

    private void Awake()
    {
        if (flow == null) flow = FindObjectOfType<LevelFlowController>();
        if (blocker == null) blocker = GetComponent<Collider2D>();
    }

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
        bool locked = flow != null && !flow.ObjectivesMet;
        if (blocker != null) blocker.enabled = locked;
        if (visualRoot != null) visualRoot.SetActive(locked);
    }
}
