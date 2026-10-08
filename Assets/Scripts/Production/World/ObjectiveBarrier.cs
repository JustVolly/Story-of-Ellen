using UnityEngine;

public class ObjectiveBarrier : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;
    [SerializeField] private Collider2D blocker;
    [SerializeField] private GameObject visualRoot;

    private GameSession subscribedSession;
    private bool flowSubscribed;

    private void Awake()
    {
        if (flow == null) flow = FindObjectOfType<LevelFlowController>();
        if (blocker == null) blocker = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (subscribedSession != null)
            subscribedSession.SessionChanged -= Refresh;

        if (flow != null && flowSubscribed)
            flow.ObjectivesChanged -= Refresh;

        subscribedSession = null;
        flowSubscribed = false;
    }

    private void Subscribe()
    {
        if (subscribedSession == null && GameSession.Instance != null)
        {
            subscribedSession = GameSession.Instance;
            subscribedSession.SessionChanged += Refresh;
        }

        if (!flowSubscribed && flow != null)
        {
            flow.ObjectivesChanged += Refresh;
            flowSubscribed = true;
        }
    }

    private void Refresh()
    {
        bool locked = flow != null && !flow.ObjectivesMet();
        if (blocker != null) blocker.enabled = locked;
        if (visualRoot != null) visualRoot.SetActive(locked);
    }
}
