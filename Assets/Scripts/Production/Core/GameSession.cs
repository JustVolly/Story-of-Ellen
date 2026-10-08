using System;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int Deaths { get; private set; }
    public int MemoryFragments { get; private set; }
    public int SecretsFound { get; private set; }
    public float LevelTime { get; private set; }
    public Transform ActiveCheckpoint { get; private set; }

    public event Action SessionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update() => LevelTime += Time.deltaTime;

    public void RegisterDeath() { Deaths++; SessionChanged?.Invoke(); }
    public void CollectMemory() { MemoryFragments++; SessionChanged?.Invoke(); }
    public void DiscoverSecret() { SecretsFound++; SessionChanged?.Invoke(); }
    public void SetCheckpoint(Transform checkpoint) { ActiveCheckpoint = checkpoint; SessionChanged?.Invoke(); }

    public void ResetRun()
    {
        Deaths = 0; MemoryFragments = 0; SecretsFound = 0; LevelTime = 0f; ActiveCheckpoint = null;
        SessionChanged?.Invoke();
    }
}
