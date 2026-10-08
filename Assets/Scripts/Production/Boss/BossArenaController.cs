using UnityEngine;

public class BossArenaController : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private GameObject entranceBarrier;
    [SerializeField] private GameObject exitBarrier;
    [SerializeField] private GameObject bossHud;
    [SerializeField] private VerticalSliceDirector director;

    private bool started;

    private void Awake()
    {
        if (bossHud != null) bossHud.SetActive(false);
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(true);
    }

    private void OnEnable()
    {
        if (boss != null) boss.Defeated += OnBossDefeated;
    }

    private void OnDisable()
    {
        if (boss != null) boss.Defeated -= OnBossDefeated;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started || !other.CompareTag("Player")) return;
        started = true;
        if (entranceBarrier != null) entranceBarrier.SetActive(true);
        if (bossHud != null) bossHud.SetActive(true);
        if (director != null) director.SetBeat(VerticalSliceDirector.Beat.Boss);
    }

    private void OnBossDefeated()
    {
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(false);
        if (bossHud != null) bossHud.SetActive(false);
        if (director != null) director.SetBeat(VerticalSliceDirector.Beat.Complete);
    }
}
