using UnityEngine;

public class EncounterTrigger : MonoBehaviour
{
    [SerializeField] private GameObject encounterRoot;
    [SerializeField] private GameObject entranceBarrier;
    [SerializeField] private EnemyHealth[] enemies;
    [SerializeField] private VerticalSliceDirector director;
    [SerializeField] private VerticalSliceDirector.Beat completionBeat = VerticalSliceDirector.Beat.Ability;

    private int alive;
    private bool started;

    private void Awake()
    {
        if (encounterRoot != null) encounterRoot.SetActive(false);
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started || !other.CompareTag("Player")) return;
        started = true;
        if (encounterRoot != null) encounterRoot.SetActive(true);
        if (entranceBarrier != null) entranceBarrier.SetActive(true);

        alive = 0;
        if (enemies == null || enemies.Length == 0) { Complete(); return; }
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            alive++;
            enemy.Died += OnEnemyDied;
        }

        if (alive == 0) Complete();
    }

    private void OnEnemyDied()
    {
        alive = Mathf.Max(0, alive - 1);
        if (alive == 0) Complete();
    }

    private void Complete()
    {
        if (enemies != null)
            if (enemies != null)
            foreach (EnemyHealth enemy in enemies) if (enemy != null) enemy.Died -= OnEnemyDied;
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (director != null) director.SetBeat(completionBeat);
    }

    private void OnDestroy()
    {
        foreach (EnemyHealth enemy in enemies) if (enemy != null) enemy.Died -= OnEnemyDied;
    }
}
