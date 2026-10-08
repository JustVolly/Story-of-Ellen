#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// A single, repeatable entry point for the whole vertical-slice campaign.
// Menu: Ellen/Production/Build Complete Campaign
// CLI:  Unity -batchmode -quit -projectPath <path> -executeMethod
//       EllenCampaignContentBuilder.BuildCompleteCampaign -logFile build-campaign.log
public static class EllenCampaignContentBuilder
{
    private static readonly string[] GameplayScenes = {
        "Assets/Scenes/OneScene.unity",
        "Assets/Scenes/TwoScene.unity",
        "Assets/Scenes/ThreeScene.unity"
    };

    [MenuItem("Ellen/Production/Build Complete Campaign")]
    public static void BuildCompleteCampaign()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Campaign building is forbidden in Play Mode.");

        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Do preflight before any existing scene is modified or saved.
        EllenLevelArtBuilder.VerifyRequiredAssets();
        for (int i = 0; i < GameplayScenes.Length; i++)
            if (!File.Exists(GameplayScenes[i]))
                throw new FileNotFoundException("Missing playable scene", GameplayScenes[i]);

        string previousScene = SceneManager.GetActiveScene().path;
        try
        {
            // Installs production controllers, HUD, movement and authored beats;
            // the installer itself also saves every upgraded scene.
            EllenProductionSceneInstaller.UpgradeBuildScenes();

            foreach (string path in GameplayScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EllenProductionLevelDesigner.ApplyToActiveScene(); // idempotent
                EllenLevelArtBuilder.Build(scene); // replace generated scenery only

                int problems = ValidateLoadedScene(scene);
                if (problems > 0)
                    throw new InvalidOperationException(
                        "Refusing to save " + scene.name + ": " + problems + " validation errors.");

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new IOException("Unable to save " + path);
            }

            EnsureAllFourBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[Ellen Campaign] Levels 1–3 built and saved with production gameplay and asset-backed art.");
        }
        finally
        {
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }
    }

    [MenuItem("Ellen/Production/Validate Complete Campaign")]
    public static void ValidateCompleteCampaign()
    {
        string previousScene = SceneManager.GetActiveScene().path;
        int errors = 0;
        try
        {
            foreach (string path in GameplayScenes)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError("[Ellen Campaign] Missing " + path);
                    errors++;
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                errors += ValidateLoadedScene(scene);
            }

            string[] expected = {
                "Assets/Scenes/StartingScene.unity",
                GameplayScenes[0], GameplayScenes[1], GameplayScenes[2]
            };
            foreach (string path in expected)
            {
                bool found = false;
                foreach (EditorBuildSettingsScene item in EditorBuildSettings.scenes)
                    if (item.path == path && item.enabled) { found = true; break; }
                if (!found)
                {
                    Debug.LogError("[Ellen Campaign] Not enabled in Build Settings: " + path);
                    errors++;
                }
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }

        if (errors != 0)
            throw new InvalidOperationException("Campaign validation failed: " + errors + " errors.");
        Debug.Log("[Ellen Campaign] Structural validation passed for all playable scenes.");
    }

    private static int ValidateLoadedScene(Scene scene)
    {
        int errors = 0;
        errors += ExpectGameObject(scene, "[Production]");
        errors += ExpectGameObject(scene, "[LevelDesign]");
        errors += ExpectGameObject(scene, EllenLevelArtBuilder.RootName);
        errors += ExpectComponent<PlayerAbilityController>(scene);
        errors += ExpectComponent<SpiritWorldController>(scene);
        errors += ExpectComponent<LevelFlowController>(scene);
        errors += ExpectComponent<LevelCompletionTrigger>(scene);
        errors += ExpectComponent<LevelEntryConfigurator>(scene);
        errors += ExpectComponent<SpiritShrine>(scene);
        errors += ExpectComponent<MemoryFragment>(scene);
        if (scene.name == "OneScene" || scene.name == "ThreeScene")
            errors += ExpectComponent<SpiritGate>(scene);
        if (scene.name == "TwoScene" || scene.name == "ThreeScene")
            errors += ExpectComponent<DashBreakableBarrier>(scene);
        if (scene.name == "TwoScene")
        {
            errors += ExpectComponent<BossController>(scene);
            errors += ExpectComponent<BossArenaController>(scene);
        }
        if (scene.name == "ThreeScene")
        {
            errors += ExpectGameObject(scene, "WallJumpShaft_01");
            errors += ExpectGameObject(scene, "WallJumpShaft_02");
        }

        Debug.Log("[Ellen Campaign] " + scene.name + ": " +
            (errors == 0 ? "structural references ready" : errors + " errors"));
        return errors;
    }

    private static int ExpectGameObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == name) return 0;
        }
        Debug.LogError("[Ellen Campaign] " + scene.name + " missing " + name);
        return 1;
    }

    private static int ExpectComponent<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<T>(true) != null) return 0;
        Debug.LogError("[Ellen Campaign] " + scene.name + " missing component " + typeof(T).Name);
        return 1;
    }

    private static void EnsureAllFourBuildScenes()
    {
        string[] required = {
            "Assets/Scenes/StartingScene.unity",
            GameplayScenes[0], GameplayScenes[1], GameplayScenes[2]
        };
        var all = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string path in required)
        {
            int index = all.FindIndex(s => s.path == path);
            if (index < 0) all.Add(new EditorBuildSettingsScene(path, true));
            else all[index].enabled = true;
        }
        EditorBuildSettings.scenes = all.ToArray();
    }
}
#endif
