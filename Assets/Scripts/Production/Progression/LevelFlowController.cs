using System;
using UnityEngine;

public class LevelFlowController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string levelId = "Level";
    [SerializeField, Min(1)] private int levelNumber = 1;

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
    public string LevelId => levelId;
    public int LevelNumber => levelNumber;
    public int RequiredMemories => requiredMemories;
    public int RequiredSecrets => requiredSecrets;
    public bool RequiresBossDefeat => requireBossDefeat;
    public bool BossDefeated => bossDefeated;
    public event Action ObjectivesChanged;
    public event Action<LevelResult> LevelCompleted;

    public void RegisterBossDefeat()
    {
        bossDefeated = true;
        ObjectivesChanged?.Invoke();
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
        LevelResult result = LevelResult.FromSession(levelId, levelNumber, totalMemories, sRankTime, aRankTime);
        LevelCompleted?.Invoke(result);
        return true;
    }
}
