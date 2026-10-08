#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// This intentionally creates only scenery. The playable terrain, authored obstacles,
// and gameplay triggers continue to live in the existing scenes and [LevelDesign].
// It is safe to run repeatedly: generated artwork is replaced as a single unit.
public static class EllenLevelArtBuilder
{
    public const string RootName = "[ProductionArt]";
    private const string GrasslandPrefabRoot =
        "Assets/BigManJD/Platformer Tileset - Pixelart Grasslands/Prefabs/";
    private const string CryptSpriteRoot = "Assets/Environment/Sprites/Crypt/";
    private const string AltarSpriteRoot = "Assets/Environment/Sprites/Altar/Altar Packed/";
    private const float GroundBottom = -41.5f;

    private sealed class LevelPalette
    {
        public string BackgroundFolder;
        public int LayerCount;
        public Color Tint;
        public Color Accent;
        public string AmbiencePath;
        public bool Crypt;
    }

    public static void VerifyRequiredAssets()
    {
        string[] sprites = {
            "Assets/Background/!_Moutain/Layer_0.png",
            "Assets/Background/2_Desert/Layer_0.png",
            "Assets/Background/3_Graveyard/Layer_0.png",
            CryptSpriteRoot + "Sprite_Ruins_1.png",
            CryptSpriteRoot + "Sprite_Pillar_1_Color.png",
            CryptSpriteRoot + "Sprite_Wall_1_Color.png",
            AltarSpriteRoot + "Sprite_Altar_1.png"
        };
        foreach (string path in sprites)
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
                throw new InvalidOperationException(
                    "Required sprite not imported: " + path + ". Run git lfs pull, then reimport.");

        string[] prefabs = { "Tree1.prefab", "Bush1.prefab", "Rock1.prefab", "WoodenSign.prefab" };
        foreach (string prefab in prefabs)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GrasslandPrefabRoot + prefab) == null)
                throw new InvalidOperationException("Missing scenery prefab: " + GrasslandPrefabRoot + prefab);
    }

    [MenuItem("Ellen/Production/Rebuild Art In Current Level")]
    public static void RebuildActiveLevel()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        VerifyRequiredAssets();
        Scene scene = SceneManager.GetActiveScene();
        Build(scene);
        if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
    }

    public static void Build(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            throw new InvalidOperationException("Open a playable scene before building its art.");

        LevelPalette palette = PaletteForScene(scene.name);
        if (palette == null)
            throw new InvalidOperationException("Unsupported scene for art: " + scene.name);

        GameObject design = FindInScene(scene, "[LevelDesign]");
        if (design == null)
            throw new InvalidOperationException("Build [LevelDesign] before production art in " + scene.name);

        GameObject old = FindInScene(scene, RootName);
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        GameObject root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        Undo.RegisterCreatedObjectUndo(root, "Build Ellen level artwork");

        GameObject background = AddGroup(root.transform, "01 | Background Layers");
        GameObject silhouette = AddGroup(root.transform, "02 | World Silhouettes");
        GameObject setDressing = AddGroup(root.transform, "03 | Environment Props");
        GameObject landmarks = AddGroup(root.transform, "04 | Landmarks");

        BuildBackground(background.transform, palette);
        BuildSilhouettes(silhouette.transform, palette);
        BuildProps(setDressing.transform, scene.name);
        BuildLandmarks(landmarks.transform, design.transform, scene.name, palette);
        BuildAmbience(root.transform, palette);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[Ellen Art] Installed existing asset scenery into " + scene.name);
    }

    private static LevelPalette PaletteForScene(string name)
    {
        switch (name)
        {
            case "OneScene": return new LevelPalette {
                BackgroundFolder = "Assets/Background/!_Moutain/",
                LayerCount = 4,
                Tint = new Color(0.84f, 0.94f, 1f, 0.9f),
                Accent = new Color(0.36f, 0.94f, 0.77f, 0.95f),
                AmbiencePath = "Assets/Audio/SFX/Ambience/SFX_Ambience_Forest_Day_Loop.wav"
            };
            case "TwoScene": return new LevelPalette {
                BackgroundFolder = "Assets/Background/2_Desert/",
                LayerCount = 5,
                Tint = new Color(1f, 0.83f, 0.7f, 0.90f),
                Accent = new Color(1f, 0.58f, 0.26f, 0.95f),
                AmbiencePath = "Assets/Audio/SFX/Ambience/SFX_Ambience_Forest_Night_Loop.wav"
            };
            case "ThreeScene": return new LevelPalette {
                BackgroundFolder = "Assets/Background/3_Graveyard/",
                LayerCount = 5,
                Tint = new Color(0.68f, 0.78f, 1f, 0.92f),
                Accent = new Color(0.54f, 0.81f, 1f, 0.95f),
                AmbiencePath = "Assets/Audio/SFX/Ambience/SFX_Ambience_Temple_Loop.wav",
                Crypt = true
            };
            default: return null;
        }
    }

    private static void BuildBackground(Transform root, LevelPalette palette)
    {
        // Five authored panels cover the complete x=-60..890 campaign corridor.
        // All sprites have a strongly negative sort order: they never cover gameplay.
        const float panelWidth = 190f;
        const float panelHeight = 94f;
        for (int layer = 0; layer < palette.LayerCount; layer++)
        {
            Sprite sprite = RequireSprite(palette.BackgroundFolder + "Layer_" + layer + ".png");
            Transform band = AddGroup(root, "Parallax Depth " + layer).transform;
            for (int panel = 0; panel < 5; panel++)
            {
                float x = 35f + panel * panelWidth;
                GameObject go = SpriteObject(band, "Backdrop " + panel,
                    sprite, new Vector3(x, -20f, 0f), -600 + layer * 3, palette.Tint);
                ScaleToDimensions(go.transform, sprite, panelWidth + 0.15f, panelHeight);
            }
        }
    }

    private static void BuildSilhouettes(Transform root, LevelPalette palette)
    {
        Sprite ruin = RequireSprite(CryptSpriteRoot + "Sprite_Ruins_1.png");
        Sprite pillar = RequireSprite(CryptSpriteRoot + "Sprite_Pillar_1_Color.png");

        for (int i = 0; i < 9; i++)
        {
            float x = 12f + i * 91f;
            Sprite sprite = i % 3 == 0 ? pillar : ruin;
            float height = i % 3 == 0 ? 23f : 14f;
            GameObject go = SpriteObject(root, "Distant Ruin " + i, sprite,
                new Vector3(x, 0f, 0f), -135, palette.Crypt
                    ? new Color(0.33f, 0.42f, 0.64f, 0.45f)
                    : new Color(0.25f, 0.32f, 0.34f, 0.22f));
            SetHeightAndBottom(go.transform, sprite, height, GroundBottom);
        }
    }

    private static void BuildProps(Transform root, string sceneName)
    {
        string[] set = sceneName == "ThreeScene"
            ? new[] { "Rock1", "Bush1", "Tree1", "Rock1", "WoodenSign" }
            : new[] { "Tree1", "Bush1", "Rock1", "Bush1", "WoodenSign" };

        // Deliberately decor-only: no additional collision volumes or triggers.
        // Gameplay/terrain authored in the existing scenes must not be overwritten.
        for (int i = 0; i < 20; i++)
        {
            string assetName = set[i % set.Length];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                GrasslandPrefabRoot + assetName + ".prefab");
            GameObject go = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
            if (go == null) throw new InvalidOperationException("Could not instantiate " + assetName);
            Undo.RegisterCreatedObjectUndo(go, "Place " + assetName);
            go.name = "Scenery_" + i + "_" + assetName;

            // Props may contain example-package gameplay components. Strip their
            // physical influence, but keep prefab links and sprite animation.
            foreach (Collider2D collider in go.GetComponentsInChildren<Collider2D>(true))
                collider.enabled = false;
            foreach (Collider collider in go.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (Rigidbody2D rb in go.GetComponentsInChildren<Rigidbody2D>(true))
                rb.simulated = false;

            float x = 16f + i * 38.5f;
            go.transform.position = new Vector3(x, GroundBottom, 0f);
            float height = assetName.StartsWith("Tree") ? 12f :
                assetName.StartsWith("Bush") ? 3.2f :
                assetName.StartsWith("Rock") ? 3.8f : 4.5f;
            NormalizePrefabHeight(go, height, GroundBottom);

            foreach (SpriteRenderer renderer in go.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingOrder = -25;
                renderer.color = sceneName == "ThreeScene"
                    ? new Color(0.74f, 0.85f, 1f, 0.76f) : new Color(1f, 1f, 1f, 0.72f);
            }
        }
    }

    private static void BuildLandmarks(
        Transform root, Transform design, string sceneName, LevelPalette palette)
    {
        Sprite altar = RequireSprite(AltarSpriteRoot + "Sprite_Altar_1.png");
        Sprite pillar = RequireSprite(CryptSpriteRoot + "Sprite_Pillar_1_Color.png");
        Sprite wall = RequireSprite(CryptSpriteRoot + "Sprite_Wall_1_Color.png");

        foreach (Transform item in design.GetComponentsInChildren<Transform>(true))
        {
            bool shrine = item.name.StartsWith("Shrine_");
            bool memory = item.name.StartsWith("Memory_");
            bool spiritGate = item.name.StartsWith("SpiritGate_");
            bool dashGate = item.name.StartsWith("DashBarrier_");
            bool objectiveGate = item.name.EndsWith("ObjectiveGate");
            bool ascentWall = item.name == "LeftWall" || item.name == "RightWall";
            if (!shrine && !memory && !spiritGate && !dashGate &&
                !objectiveGate && !ascentWall) continue;

            Sprite sprite = shrine || memory ? altar : ascentWall ? wall : pillar;
            float height = shrine ? 4.0f : memory ? 1.5f :
                ascentWall ? 40f : 12f;
            float width = ascentWall ? 1.15f : 0f;

            // Game-object ownership is deliberate: pickups destroy their art,
            // barriers control their own visualRoot, and checkpoints stay visible.
            Transform parent = item;
            if (spiritGate || dashGate || objectiveGate)
            {
                Transform visual = item.Find(spiritGate ? "SpiritBarrierVisual" :
                    dashGate ? "DashBarrierVisual" : "ObjectiveBarrierVisual");
                if (visual != null) parent = visual;
            }
            GameObject go = SpriteObject(parent, "ProductionVisual", sprite,
                item.position, shrine ? 24 : memory ? 35 : -5, palette.Accent);
            go.transform.localPosition = Vector3.zero;
            if (width > 0f) ScaleToDimensions(go.transform, sprite, width, height);
            else ScaleToHeight(go.transform, sprite, height);

            if (shrine)
            {
                go.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            }
        }

        // Scene-specific hero compositions only; do not intersect physical geometry.
        if (sceneName == "ThreeScene")
        {
            AddMonument(root, pillar, new Vector2(120f, -4f), 13f, palette.Accent);
            AddMonument(root, pillar, new Vector2(580f, -4f), 13f, palette.Accent);
            AddMonument(root, altar, new Vector2(698f, -35f), 8f, palette.Accent);
        }
        else if (sceneName == "TwoScene")
        {
            AddMonument(root, pillar, new Vector2(668f, -36f), 11f, palette.Accent);
            AddMonument(root, pillar, new Vector2(777f, -36f), 11f, palette.Accent);
        }
        else
        {
            AddMonument(root, altar, new Vector2(700f, -37f), 7f, palette.Accent);
        }
    }

    private static void AddMonument(Transform root, Sprite sprite,
        Vector2 position, float height, Color tint)
    {
        GameObject go = SpriteObject(root, "Progression Monument",
            sprite, new Vector3(position.x, position.y, 0f), -8, tint);
        ScaleToHeight(go.transform, sprite, height);
    }

    private static void BuildAmbience(Transform root, LevelPalette palette)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(palette.AmbiencePath);
        if (clip == null)
        {
            Debug.LogWarning("[Ellen Art] Missing ambience (check git lfs): " + palette.AmbiencePath);
            return;
        }
        GameObject go = AddGroup(root, "05 | World Ambience");
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = true;
        source.spatialBlend = 0f;
        source.volume = 0.15f;
    }

    private static GameObject AddGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Sprite RequireSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            throw new InvalidOperationException("Sprite missing or not imported: " + path);
        return sprite;
    }

    private static GameObject SpriteObject(
        Transform parent, string name, Sprite sprite, Vector3 worldPosition,
        int sortingOrder, Color color)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent, false);
        go.transform.position = worldPosition;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        sr.color = color;
        return go;
    }

    private static void ScaleToHeight(Transform item, Sprite sprite, float height)
    {
        float factor = height / Mathf.Max(0.01f, sprite.bounds.size.y);
        item.localScale = new Vector3(factor, factor, 1f);
    }

    private static void ScaleToDimensions(Transform item, Sprite sprite, float width, float height)
    {
        item.localScale = new Vector3(width / Mathf.Max(0.01f, sprite.bounds.size.x),
            height / Mathf.Max(0.01f, sprite.bounds.size.y), 1f);
    }

    private static void SetHeightAndBottom(Transform item, Sprite sprite, float height, float bottom)
    {
        ScaleToHeight(item, sprite, height);
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        item.position += Vector3.up * (bottom - sr.bounds.min.y);
    }

    private static void NormalizePrefabHeight(GameObject prefab, float height, float bottom)
    {
        SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        float ratio = height / Mathf.Max(0.01f, bounds.size.y);
        prefab.transform.localScale *= Mathf.Clamp(ratio, 0.05f, 15f);

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        prefab.transform.position += Vector3.up * (bottom - bounds.min.y);
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in all)
                if (child.name == name) return child.gameObject;
        }
        return null;
    }
}
#endif
