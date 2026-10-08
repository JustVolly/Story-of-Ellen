using System;

[Serializable]
public struct LevelResult
{
    public string LevelId;
    public int LevelNumber;
    public float CompletionTime;
    public int Deaths;
    public int MemoriesFound;
    public int MemoriesTotal;
    public int SecretsFound;
    public string Rank;

    public static LevelResult FromSession(string levelId, int levelNumber, int memoriesTotal, float sRankTime, float aRankTime)
    {
        GameSession session = GameSession.Instance;
        if (session == null) return default;

        return new LevelResult
        {
            LevelId = levelId,
            LevelNumber = levelNumber,
            CompletionTime = session.LevelTime,
            Deaths = session.Deaths,
            MemoriesFound = session.MemoryFragments,
            MemoriesTotal = memoriesTotal,
            SecretsFound = session.SecretsFound,
            Rank = LevelRankCalculator.Calculate(
                session.LevelTime,
                sRankTime,
                aRankTime,
                session.Deaths,
                session.MemoryFragments,
                memoriesTotal)
        };
    }
}
