using UnityEngine;

public class GameplayBootstrap : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private GameSession gameSession;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAbilityController abilities;
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private LevelFlowController levelFlow;

    [Header("Optional")]
    [SerializeField] private LevelResultPresenter resultPresenter;

    private void Awake()
    {
        if (gameSession == null) gameSession = FindAnyObjectByType<GameSession>();
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (abilities == null) abilities = FindAnyObjectByType<PlayerAbilityController>();
        if (spiritWorld == null) spiritWorld = FindAnyObjectByType<SpiritWorldController>();
        if (levelFlow == null) levelFlow = FindAnyObjectByType<LevelFlowController>();

        ValidateScene();
    }

    private void Start()
    {
        gameSession?.ResetRun();
    }

    private void ValidateScene()
    {
        if (gameSession == null) Debug.LogError("[GameplayBootstrap] Missing GameSession.", this);
        if (playerHealth == null) Debug.LogError("[GameplayBootstrap] Missing PlayerHealth.", this);
        if (abilities == null) Debug.LogWarning("[GameplayBootstrap] PlayerAbilityController not configured.", this);
        if (spiritWorld == null) Debug.LogWarning("[GameplayBootstrap] SpiritWorldController not configured.", this);
        if (levelFlow == null) Debug.LogWarning("[GameplayBootstrap] LevelFlowController not configured.", this);
    }
}
