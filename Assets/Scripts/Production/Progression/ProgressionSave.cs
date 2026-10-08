using System;
using UnityEngine;

public static class ProgressionSave
{
    private const string Key = "ellen.progress.v1";
    private const int CurrentVersion = 2;
    private const int MaxPlayableLevel = 2;

    [Serializable]
    public class LevelRecord
    {
        public string levelId;
        public int levelNumber;
        public bool completed;
        public float bestCompletionTime;
        public int bestDeaths = -1;
        public string bestRank = "";
        public int bestMemories;
        public int bestSecrets;
    }

    [Serializable]
    public class Data
    {
        public int version = CurrentVersion;
        public int highestUnlockedLevel = 1;
        public bool campaignCompleted;
        public int totalMemoryFragments;
        public int totalSecrets;
        public int unlockedAbilities = 4;
        public string bestRank = "";
        public float bestCompletionTime;
        public int bestDeaths = -1;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public LevelRecord[] levels = Array.Empty<LevelRecord>();
    }

    public static Data Load()
    {
        Data data;

        if (!PlayerPrefs.HasKey(Key))
            return new Data();

        try
        {
            data = JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key)) ?? new Data();
        }
        catch
        {
            data = new Data();
        }

        Migrate(data);
        return data;
    }

    public static void Save(Data data)
    {
        if (data == null) return;

        Migrate(data);
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void RecordLevelResult(LevelResult result)
    {
        if (string.IsNullOrWhiteSpace(result.LevelId) || result.LevelNumber <= 0) return;

        Data data = Load();
        int index = FindLevelIndex(data, result.LevelId);

        if (index < 0)
        {
            int oldLength = data.levels.Length;
            Array.Resize(ref data.levels, oldLength + 1);
            data.levels[oldLength] = new LevelRecord
            {
                levelId = result.LevelId,
                levelNumber = result.LevelNumber
            };
            index = oldLength;
        }

        LevelRecord record = data.levels[index];
        record.completed = true;
        record.levelNumber = result.LevelNumber;
        record.bestMemories = Mathf.Max(record.bestMemories, result.MemoriesFound);
        record.bestSecrets = Mathf.Max(record.bestSecrets, result.SecretsFound);

        if (record.bestCompletionTime <= 0f || result.CompletionTime < record.bestCompletionTime)
            record.bestCompletionTime = result.CompletionTime;

        if (record.bestDeaths < 0 || result.Deaths < record.bestDeaths)
            record.bestDeaths = result.Deaths;

        if (RankValue(result.Rank) > RankValue(record.bestRank))
            record.bestRank = result.Rank;

        data.highestUnlockedLevel = Mathf.Clamp(Mathf.Max(data.highestUnlockedLevel, result.LevelNumber + 1), 1, MaxPlayableLevel);

        // Legacy aggregate fields stay valid for older UI/save consumers.
        if (data.bestCompletionTime <= 0f || result.CompletionTime < data.bestCompletionTime)
            data.bestCompletionTime = result.CompletionTime;
        if (data.bestDeaths < 0 || result.Deaths < data.bestDeaths)
            data.bestDeaths = result.Deaths;
        if (RankValue(result.Rank) > RankValue(data.bestRank))
            data.bestRank = result.Rank;

        RecalculateCollectibleTotals(data);
        Save(data);
    }

    public static LevelRecord GetLevelRecord(string levelId)
    {
        Data data = Load();
        int index = FindLevelIndex(data, levelId);
        return index >= 0 ? data.levels[index] : null;
    }

    private static void Migrate(Data data)
    {
        if (data.levels == null) data.levels = Array.Empty<LevelRecord>();
        data.highestUnlockedLevel = Mathf.Clamp(data.highestUnlockedLevel, 1, MaxPlayableLevel);
        data.musicVolume = Mathf.Clamp01(data.musicVolume);
        data.sfxVolume = Mathf.Clamp01(data.sfxVolume);
        data.version = CurrentVersion;
    }

    private static int FindLevelIndex(Data data, string levelId)
    {
        if (data.levels == null) return -1;

        for (int i = 0; i < data.levels.Length; i++)
        {
            LevelRecord record = data.levels[i];
            if (record != null && string.Equals(record.levelId, levelId, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    private static void RecalculateCollectibleTotals(Data data)
    {
        int memories = 0;
        int secrets = 0;

        foreach (LevelRecord record in data.levels)
        {
            if (record == null) continue;
            memories += Mathf.Max(0, record.bestMemories);
            secrets += Mathf.Max(0, record.bestSecrets);
        }

        data.totalMemoryFragments = memories;
        data.totalSecrets = secrets;
    }

    private static int RankValue(string rank)
    {
        switch (rank)
        {
            case "S": return 4;
            case "A": return 3;
            case "B": return 2;
            case "C": return 1;
            default: return 0;
        }
    }
}
