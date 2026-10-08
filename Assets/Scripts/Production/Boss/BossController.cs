using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    public enum Phase { One, Two, Three }

    [SerializeField] private GameObject[] phaseTwoObjects;
    [SerializeField] private GameObject[] phaseThreeObjects;
    [SerializeField] private LevelFlowController levelFlow;
    private EnemyHealth health;
    public Phase CurrentPhase { get; private set; }
    public event Action<Phase> PhaseChanged;
    public event Action Defeated;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        CurrentPhase = Phase.One;
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.HealthChanged += OnHealthChanged;
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
    }

    private void OnHealthChanged(int current, int max)
    {
        float ratio = max <= 0 ? 0f : (float)current / max;
        Phase next = ratio <= 0.33f ? Phase.Three : ratio <= 0.66f ? Phase.Two : Phase.One;
        if (next == CurrentPhase) return;
        CurrentPhase = next;
        if (next >= Phase.Two) Activate(phaseTwoObjects);
        if (next >= Phase.Three) Activate(phaseThreeObjects);
        PhaseChanged?.Invoke(next);
    }

    private void OnDied()
    {
        if (levelFlow != null) levelFlow.RegisterBossDefeat();
        Defeated?.Invoke();
    }

    private static void Activate(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (GameObject item in objects) if (item != null) item.SetActive(true);
    }
}
