#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class EllenVerticalSliceValidator
{
    [MenuItem("Ellen/Validate Vertical Slice")]
    public static void Validate()
    {
        int errors = 0;
        errors += Require<GameSession>();
        errors += Require<GameplayBootstrap>();
        errors += Require<PlayerHealth>();
        errors += Require<PlayerAbilityController>();
        errors += Require<PlayerRespawnController>();
        errors += Require<SpiritWorldController>();
        errors += Require<LevelFlowController>();

        int memories = Object.FindObjectsByType<MemoryFragment>(FindObjectsSortMode.None).Length;
        int shrines = Object.FindObjectsByType<SpiritShrine>(FindObjectsSortMode.None).Length;
        int enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
        int bosses = Object.FindObjectsByType<BossController>(FindObjectsSortMode.None).Length;

        Debug.Log($"[Ellen Vertical Slice] Scene={SceneManager.GetActiveScene().name} Errors={errors} Memories={memories} Shrines={shrines} Enemies={enemies} Bosses={bosses}");
        if (memories < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one MemoryFragment.");
        if (shrines < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one SpiritShrine.");
        if (enemies < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one production EnemyHealth encounter.");

        ValidateMetaFiles("Assets/Scripts/Production");
    }

    private static void ValidateMetaFiles(string root)
    {
        if (!Directory.Exists(root)) return;
        foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta")) continue;
            if (!File.Exists(file + ".meta"))
                Debug.LogError("[Ellen Vertical Slice] Missing Unity meta file: " + file + ".meta");
        }
    }

    private static int Require<T>() where T : Object
    {
        if (Object.FindFirstObjectByType<T>() != null) return 0;
        Debug.LogError($"[Ellen Vertical Slice] Missing required component: {typeof(T).Name}");
        return 1;
    }
}
#endif
