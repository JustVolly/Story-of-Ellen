using UnityEngine;

/// <summary>
/// Controls an arena attempt. Player death reopens the entrance and rearms the
/// undefeated Guardian; killing it permanently unlocks the exit for this run.
/// </summary>
public class BossArenaController : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private GameObject entranceBarrier;
    [SerializeField] private GameObject exitBarrier;
    [SerializeField] private GameObject bossHud;
    [SerializeField] private GameObject bossRoot;
    [SerializeField] private bool activateBossOnEnter = true;
    [SerializeField] private VerticalSliceDirector director;
    [SerializeField] private PlayerHealth playerHealth;

    private bool started;
    private bool defeated;
    private Vector3 bossSpawnPosition;
    private Quaternion bossSpawnRotation;
    private Rigidbody2D bossBody;

    private void Awake()
    {
        if (bossRoot == null && boss != null) bossRoot = boss.gameObject;
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (bossRoot != null)
        {
            bossSpawnPosition = bossRoot.transform.position;
            bossSpawnRotation = bossRoot.transform.rotation;
            bossBody = bossRoot.GetComponent<Rigidbody2D>();
        }
        if (bossHud != null) bossHud.SetActive(false);
        if (activateBossOnEnter && bossRoot != null) bossRoot.SetActive(false);
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(true);
    }

    private void OnEnable()
    {
        if (boss != null) boss.Defeated += OnBossDefeated;
        if (playerHealth != null) playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (boss != null) boss.Defeated -= OnBossDefeated;
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started || defeated || !other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isAlive) return;

        started = true;
        if (entranceBarrier != null) entranceBarrier.SetActive(true);
        if (activateBossOnEnter && bossRoot != null) bossRoot.SetActive(true);
        if (bossHud != null) bossHud.SetActive(true);
        if (director != null) director.SetBeat(VerticalSliceDirector.Beat.Boss);
    }

    private void OnPlayerDied()
    {
        // Never relock the finished arena after defeating the Guardian.
        if (!started || defeated) return;
        ResetUndefeatedEncounter();
    }

    private void ResetUndefeatedEncounter()
    {
        // Disabling the boss first cancels an active attack coroutine and
        // avoids an immediate re-hit while the player respawns.
        if (activateBossOnEnter && bossRoot != null)
            bossRoot.SetActive(false);

        if (bossBody != null)
        {
            bossBody.linearVelocity = Vector2.zero;
            bossBody.angularVelocity = 0f;
        }

        if (bossRoot != null)
        {
            bossRoot.transform.SetPositionAndRotation(bossSpawnPosition, bossSpawnRotation);
            Physics2D.SyncTransforms();
        }

        boss?.ResetForRetry();
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(true);
        if (bossHud != null) bossHud.SetActive(false);
        started = false;
    }

    private void OnBossDefeated()
    {
        defeated = true;
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(false);
        if (bossHud != null) bossHud.SetActive(false);
        if (director != null) director.SetBeat(VerticalSliceDirector.Beat.Complete);
    }
}
