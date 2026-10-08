#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Rebuilds only ThreeScene. Unlike Build Complete Campaign, this workflow
// never opens or saves OneScene/TwoScene. Run it after pulling current main.
// CLI: Unity.exe -batchmode -quit -projectPath <project> -executeMethod
//      EllenLevelThreeBuilder.BuildLevelThree -logFile <log>
public static class EllenLevelThreeBuilder
{
    public const string ScenePath = "Assets/Scenes/ThreeScene.unity";

    [MenuItem("Ellen/Production/Build Level 3 - Astral Crypt")]
    public static void BuildLevelThree()
    {
        EnsureNotPlaying();
        if (!EditorApplication.isPlaying && !Application.isBatchMode
            && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("Level Three scene is missing", ScenePath);

        // Fail before modifying the loaded scene if git-lfs/imported art is absent.
        EllenLevelArtBuilder.VerifyRequiredAssets();

        string previousScene = SceneManager.GetActiveScene().path;
        bool saved = false;
        try
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Installs production player input/ability flow/HUD, preserving
            // legacy geometry and editor-authored scene objects.
            EllenProductionSceneInstaller.UpgradeCurrentScene();

            // Replace only the generated design and art layers. Existing
            // player/enemies/cameras/terrain remain in place.
            EllenProductionLevelDesigner.RebuildCurrentLevelDesign();
            EllenLevelArtBuilder.Build(scene);

            int errors = CheckLevelThree(scene);
            if (errors > 0)
                throw new InvalidOperationException(
                    "[Ellen Level 3] Validation failed (" + errors +
                    " checks). ThreeScene has NOT been saved.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Could not save Level Three scene.");

            AssetDatabase.SaveAssets();
            saved = true;
            Debug.Log("[Ellen Level 3] Astral Crypt saved to " + ScenePath +
                "; OneScene and TwoScene were not modified.");
        }
        finally
        {
            if (!saved)
            {
                // Discard unsaved generated content if validation or asset import
                // failed; leave the committed scene file untouched.
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            if (!string.IsNullOrEmpty(previousScene)
                && previousScene != ScenePath && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }
    }

    [MenuItem("Ellen/Production/Validate Level 3 - Astral Crypt")]
    public static void ValidateLevelThree()
    {
        EnsureNotPlaying();
        if (!Application.isBatchMode
            && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string previousScene = SceneManager.GetActiveScene().path;
        try
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int errors = CheckLevelThree(scene);
            if (errors != 0)
                throw new InvalidOperationException(
                    "[Ellen Level 3] " + errors + " structural validation errors.");
            Debug.Log("[Ellen Level 3] Structural checks passed.");
        }
        finally
        {
            if (!string.IsNullOrEmpty(previousScene)
                && previousScene != ScenePath && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }
    }

    private static void EnsureNotPlaying()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException(
                "Exit Play Mode before building or validating Level Three.");
    }

    private static int CheckLevelThree(Scene scene)
    {
        int errors = 0;
        errors += RequireObject(scene, "[Production]");
        errors += RequireObject(scene, "[LevelDesign]");
        errors += RequireObject(scene, EllenLevelArtBuilder.RootName);
        errors += RequireObject(scene, "AstralCrypt_TraversalRoutes");
        errors += RequireObject(scene, "WallJumpShaft_01");
        errors += RequireObject(scene, "WallJumpShaft_02");
        errors += RequireObject(scene, "Secret_SpiritBalcony");
        errors += RequireObject(scene, "AbilitySeal_SpiritChain");
        errors += RequireObject(scene, "AbilitySeal_DashChain");
        errors += RequireObject(scene, "MasteryObjectiveGate");
        errors += RequireObject(scene, "LevelResultTrigger");

        errors += RequireComponent<EllenGameplayInput>(scene);
        errors += RequireComponent<EllenFallRecovery>(scene);
        errors += RequireComponent<PlayerAbilityController>(scene);
        errors += RequireComponent<SpiritWorldController>(scene);
        errors += RequireComponent<LevelFlowController>(scene);
        errors += RequireComponent<LevelEntryConfigurator>(scene);
        errors += RequireComponent<LevelCompletionTrigger>(scene);
        errors += RequireComponent<SpiritGate>(scene);
        errors += RequireComponent<DashBreakableBarrier>(scene);
        errors += RequireCount<SpiritWell>(scene, 2);
        errors += RequireCount<SpiritShrine>(scene, 2);
        errors += RequireCount<MemoryFragment>(scene, 3);
        errors += RequireCount<AbilitySeal>(scene, 2);

        LevelFlowController flow = First<LevelFlowController>(scene);
        if (flow != null && (flow.LevelNumber != 3 || flow.RequiredMemories != 2
            || flow.RequiredSecrets != 0 || flow.RequiresBossDefeat))
        {
            Debug.LogError("[Ellen Level 3] Incorrect mastery progression requirements.");
            errors++;
        }

        GameObject route = Find(scene, "AstralCrypt_TraversalRoutes");
        if (route != null)
        {
            PlatformEffector2D[] platforms =
                route.GetComponentsInChildren<PlatformEffector2D>(true);
            // First shaft 12 + second shaft 12 + balcony 8 +
            // optional memory detour 5 + final descent 7.
            if (platforms.Length != 44)
            {
                Debug.LogError("[Ellen Level 3] Expected 44 authored traversal ledges; found "
                    + platforms.Length + ".");
                errors++;
            }

            foreach (PlatformEffector2D platform in platforms)
            {
                BoxCollider2D body = platform.GetComponent<BoxCollider2D>();
                if (!platform.useOneWay || body == null || !body.usedByEffector)
                {
                    Debug.LogError("[Ellen Level 3] Invalid one-way ledge: "
                        + platform.name);
                    errors++;
                }
            }
        }

        bool inBuild = false;
        foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            if (entry.path == ScenePath && entry.enabled) inBuild = true;
        if (!inBuild)
        {
            Debug.LogError("[Ellen Level 3] ThreeScene is not enabled in Build Settings.");
            errors++;
        }

        Debug.Log("[Ellen Level 3] " + scene.name + " validation: " + errors + " errors.");
        return errors;
    }

    private static int RequireObject(Scene scene, string name)
    {
        if (Find(scene, name) != null) return 0;
        Debug.LogError("[Ellen Level 3] Missing object: " + name);
        return 1;
    }

    private static int RequireComponent<T>(Scene scene) where T : Component
    {
        return RequireCount<T>(scene, 1);
    }

    private static int RequireCount<T>(Scene scene, int expected) where T : Component
    {
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
            count += root.GetComponentsInChildren<T>(true).Length;
        if (count >= expected) return 0;
        Debug.LogError("[Ellen Level 3] Missing " + typeof(T).Name +
            " (" + count + "/" + expected + ").");
        return 1;
    }

    private static T First<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T match = root.GetComponentInChildren<T>(true);
            if (match != null) return match;
        }
        return null;
    }

    private static GameObject Find(Scene scene, string name)
    {
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
        foreach (Transform child in root)
        {
            Transform match = FindRecursive(child, name);
            if (match != null) return match;
        }
        return null;
    }
}
#endif
