#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EllenVerticalSliceValidator
{
    [MenuItem("Ellen/Validate Vertical Slice")]
    public static void ValidateVerticalSlice()
    {
        ValidateGameplayScene(true);
    }

    [MenuItem("Ellen/Production/Validate Current Scene")]
    public static void ValidateProduction()
    {
        Scene scene = SceneManager.GetActiveScene();
        int errors = 0;

        if (scene.name == "StartingScene")
            errors += ValidateStartingScene();
        else
            errors += ValidateGameplayScene(false);

        errors += ValidateMetaFiles("Assets/Scripts/Production");
        errors += ValidateMetaFiles("Assets/Editor");
        errors += ValidateBuildScene("StartingScene");
        errors += ValidateBuildScene("OneScene");
        errors += ValidateBuildScene("TwoScene");
        errors += ValidateBuildScene("ThreeScene");

        if (errors == 0)
            Debug.Log($"[Ellen Production] PASS — {scene.name} has no static production-readiness errors.");
        else
            Debug.LogError($"[Ellen Production] FAIL — {scene.name} has {errors} production-readiness error(s).");
    }

    private static int ValidateStartingScene()
    {
        int errors = 0;
        errors += Require<StartScene>();
        errors += Require<LoaderPanel>();
        errors += Require<MainMenuPresentation>();
        errors += Require<MainMenuSettingsController>();

        errors += RequireNamedObject("Play");
        errors += RequireNamedObject("Quit");
        errors += RequireNamedObject("Settings");
        errors += RequireNamedObject("NewJourney");
        errors += RequireNamedObject("SettingsPanel");

        Canvas canvas = FindFirstInScene<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] StartingScene requires a Canvas.");
            errors++;
        }
        else
        {
            CanvasScalerCheck(canvas, ref errors);
        }

        return errors;
    }

    private static int ValidateGameplayScene(bool includeContentWarnings)
    {
        int errors = 0;
        errors += Require<GameSession>();
        errors += Require<GameplayBootstrap>();
        errors += Require<PlayerHealth>();
        errors += Require<PlayerMovement>();
        errors += Require<PlayerAdvancedMovement>();
        errors += Require<PlayerAbilityController>();
        errors += Require<PlayerRespawnController>();
        errors += Require<PlayerMovementFeedback>();
        errors += Require<SpiritWorldController>();
        errors += Require<LevelFlowController>();
        errors += Require<LevelEntryConfigurator>();
        errors += Require<LevelResultRecorder>();
        errors += Require<LevelResultPresenter>();
        errors += Require<SpiritWorldPresentation>();
        errors += Require<HitStop>();
        errors += Require<AudioManager>();

        errors += RequireNamedObject("ProductionHUD");
        errors += RequireNamedObject("SpiritHUD");
        errors += RequireNamedObject("ObjectiveHUD");
        errors += RequireNamedObject("AbilityToast");
        errors += RequireNamedObject("SpiritTutorialToast");
        errors += RequireNamedObject("ResultHUD");
        errors += RequireNamedObject("LevelResultPanel");
        errors += RequireNamedObject("[LevelDesign]");

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OneScene")
        {
            errors += RequireCount<MemoryFragment>(3, "MemoryFragment");
            errors += RequireCount<SpiritShrine>(2, "SpiritShrine");
            errors += RequireCount<SpiritGate>(2, "SpiritGate");
            errors += RequireCount<ObjectiveBarrier>(1, "ObjectiveBarrier");
            errors += RequireCount<LevelCompletionTrigger>(1, "LevelCompletionTrigger");
        }

        if (sceneName == "TwoScene")
        {
            errors += RequireNamedObject("Dash");
            errors += RequireNamedObject("SpiritGuardian");
            errors += RequireNamedObject("BossHUD");
            errors += RequireCount<MemoryFragment>(3, "MemoryFragment");
            errors += RequireCount<SpiritShrine>(2, "SpiritShrine");
            errors += RequireCount<DashBreakableBarrier>(2, "DashBreakableBarrier");
            errors += RequireCount<BossController>(1, "BossController");
            errors += RequireCount<BossArenaController>(1, "BossArenaController");
            errors += RequireCount<LevelCompletionTrigger>(1, "LevelCompletionTrigger");
        }

        if (sceneName == "ThreeScene")
        {
            errors += RequireNamedObject("Dash");
            errors += RequireNamedObject("WallJump");
            errors += RequireNamedObject("WallJumpShaft_01");
            errors += RequireNamedObject("WallJumpShaft_02");
            errors += RequireCount<MemoryFragment>(3, "MemoryFragment");
            errors += RequireCount<SpiritShrine>(2, "SpiritShrine");
            errors += RequireCount<SpiritGate>(1, "SpiritGate");
            errors += RequireCount<DashBreakableBarrier>(1, "DashBreakableBarrier");
            errors += RequireCount<ObjectiveBarrier>(1, "ObjectiveBarrier");
            errors += RequireCount<LevelCompletionTrigger>(1, "LevelCompletionTrigger");
        }

        Canvas canvas = FindFirstInScene<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] Gameplay scene requires a Canvas.");
            errors++;
        }
        else
        {
            CanvasScalerCheck(canvas, ref errors);
        }

        int memories = Count<MemoryFragment>();
        int shrines = Count<SpiritShrine>();
        int enemies = Count<EnemyHealth>();
        int bosses = Count<BossController>();

        Debug.Log($"[Ellen Vertical Slice] Scene={SceneManager.GetActiveScene().name} Errors={errors} Memories={memories} Shrines={shrines} Enemies={enemies} Bosses={bosses}");

        if (includeContentWarnings)
        {
            if (memories < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one MemoryFragment.");
            if (shrines < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one SpiritShrine.");
            if (enemies < 1) Debug.LogWarning("[Ellen Vertical Slice] Add at least one production EnemyHealth encounter.");
        }

        return errors;
    }

    private static void CanvasScalerCheck(Canvas canvas, ref int errors)
    {
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null || scaler.uiScaleMode != UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            Debug.LogError("[Ellen Production] Canvas must use Scale With Screen Size.", canvas);
            errors++;
        }
    }

    private static int ValidateMetaFiles(string root)
    {
        if (!Directory.Exists(root)) return 0;

        int missing = 0;
        foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta")) continue;
            if (File.Exists(file + ".meta")) continue;

            missing++;
            Debug.LogError("[Ellen Production] Missing Unity meta file: " + file + ".meta");
        }

        return missing;
    }

    private static int ValidateBuildScene(string sceneName)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            if (Path.GetFileNameWithoutExtension(scene.path) == sceneName) return 0;
        }

        Debug.LogError("[Ellen Production] Required scene is not enabled in Build Settings: " + sceneName);
        return 1;
    }

    private static int Require<T>() where T : Object
    {
        if (FindFirstInScene<T>() != null) return 0;

        Debug.LogError($"[Ellen Production] Missing required component: {typeof(T).Name}");
        return 1;
    }

    private static int RequireNamedObject(string objectName)
    {
        if (FindNamedObject(objectName) != null) return 0;

        Debug.LogError("[Ellen Production] Missing required scene object: " + objectName);
        return 1;
    }

    private static int RequireCount<T>(int minimum, string label) where T : Component
    {
        int count = Count<T>();
        if (count >= minimum) return 0;

        Debug.LogError($"[Ellen Production] {label} count is {count}; expected at least {minimum}.");
        return 1;
    }

    private static int Count<T>() where T : Component
    {
        int count = 0;
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
            count += root.GetComponentsInChildren<T>(true).Length;

        return count;
    }

    private static T FindFirstInScene<T>() where T : Object
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                Component component = root.GetComponentInChildren(typeof(T), true);
                if (component != null) return component as T;
            }
        }

        return null;
    }

    private static GameObject FindNamedObject(string name)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, name);
            if (found != null) return found.gameObject;
        }

        return null;
    }

    private static Transform FindRecursive(Transform root, string name)
    {
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }
}
#endif
