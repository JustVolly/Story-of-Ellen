using System;
using UnityEngine;

public static class ProgressionSave
{
    private const string Key = "ellen.progress.v1";

    [Serializable]
    public class Data
    {
        public int highestUnlockedLevel = 1;
        public int totalMemoryFragments;
        public int totalSecrets;
        public int unlockedAbilities = 4;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
    }

    public static Data Load()
    {
        if (!PlayerPrefs.HasKey(Key)) return new Data();
        try { return JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key)) ?? new Data(); }
        catch { return new Data(); }
    }

    public static void Save(Data data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
