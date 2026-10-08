using System;
using UnityEngine;

public class LevelFlowController : MonoBehaviour
{
    [Header("Objectives")]
    [SerializeField, Min(0)] private int requiredMemories;
    [SerializeField, Min(0)] private int requiredSecrets;
    [SerializeField] private bool requireBossDefeat;

    [Header("Ranking")]
    [SerializeField, Min(1f)] private float sRankTime = 90f;
    [SerializeField, Min(1f)] private float aRankTime = 150f;
    [SerializeField, Min(0)] private int totalMemories = 3;

    private bool bossDefeated;
    private bool completed;

    public bool Completed => completed;
    public event Action<LevelResult> LevelCompleted;

    public void RegisterBossDefeat()
    {
        bossDefeated = true;
        TryComplete();
    }

    public bool ObjectivesMet()
    {
        GameSession session = GameSession.Instance;
        if (session == null) return false;
        return session.MemoryFragments >= requiredMemories
            && session.SecretsFound >= requiredSecrets
            && (!requireBossDefeat || bossDefeated);
    }

    public bool TryComplete()
    {
        if (completed || !ObjectivesMet()) return false;
        completed = true;
        LevelResult result = LevelResult.FromSession(totalMemories, sRankTime, aRankTime);
        LevelCompleted?.Invoke(result);
        return true;
    }
}
