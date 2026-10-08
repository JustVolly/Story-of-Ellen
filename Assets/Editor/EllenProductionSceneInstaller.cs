#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EllenProductionSceneInstaller
{
    private static readonly string[] TargetScenes =
    {
        "Assets/Scenes/StartingScene.unity",
        "Assets/Scenes/OneScene.unity",
        "Assets/Scenes/TwoScene.unity"
    };

    [MenuItem("Ellen/Production/Upgrade Build Scenes")]
    public static void UpgradeBuildScenes()
    {
        string previousScene = SceneManager.GetActiveScene().path;

        foreach (string path in TargetScenes)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("[Ellen Production] Scene not found: " + path);
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            UpgradeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        Debug.Log("[Ellen Production] StartingScene, OneScene and TwoScene production upgrade completed.");
    }

    [MenuItem("Ellen/Production/Upgrade Current Scene")]
    public static void UpgradeCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        UpgradeScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[Ellen Production] Current scene upgraded. Review it, then save.");
    }

    private static void UpgradeScene(Scene scene)
    {
        if (scene.name == "StartingScene")
            UpgradeStartingScene();
        else
            UpgradeGameplayScene();
    }

    private static void UpgradeStartingScene()
    {
        Canvas canvas = FindFirstInScene<Canvas>();
        StartScene startScene = FindFirstInScene<StartScene>();
        LoaderPanel loader = FindFirstInScene<LoaderPanel>();

        if (canvas == null || startScene == null)
        {
            Debug.LogError("[Ellen Production] StartingScene is missing Canvas or StartScene.");
            return;
        }

        ConfigureCanvas(canvas);

        CanvasGroup canvasGroup = EnsureComponent<CanvasGroup>(canvas.gameObject);
        MainMenuPresentation presentation = EnsureComponent<MainMenuPresentation>(canvas.gameObject);

        GameObject play = FindInScene("Play");
        GameObject quit = FindInScene("Quit");
        GameObject newJourney = FindInScene("NewJourney");

        if (newJourney == null && play != null)
        {
            newJourney = Object.Instantiate(play, play.transform.parent);
            newJourney.name = "NewJourney";
            Undo.RegisterCreatedObjectUndo(newJourney, "Create New Journey button");

            RectTransform playRect = play.GetComponent<RectTransform>();
            RectTransform quitRect = quit != null ? quit.GetComponent<RectTransform>() : null;
            RectTransform newRect = newJourney.GetComponent<RectTransform>();

            if (playRect != null && newRect != null)
            {
                newRect.anchoredPosition = quitRect != null
                    ? Vector2.Lerp(playRect.anchoredPosition, quitRect.anchoredPosition, 0.5f)
                    : playRect.anchoredPosition + Vector2.down * 44f;
            }

            TextMeshProUGUI label = newJourney.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = "NEW JOURNEY";

            Button button = newJourney.GetComponent<Button>();
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(button.onClick, startScene.StartFromBeginning);
            }
        }

        RectTransform[] titleParts = CollectRects("StoryOfEllen", "StoryOfEllen (1)");
        RectTransform[] menuButtons = CollectRects("Play", "NewJourney", "Quit");
        Transform[] backgrounds = CollectTransforms("Background2", "Background");

        SerializedObject presentationSo = new SerializedObject(presentation);
        SetObjectArray(presentationSo.FindProperty("titleParts"), titleParts);
        SetObjectArray(presentationSo.FindProperty("menuButtons"), menuButtons);
        SetObjectArray(presentationSo.FindProperty("backgroundLayers"), backgrounds);
        presentationSo.ApplyModifiedPropertiesWithoutUndo();

        TextMeshProUGUI playLabel = play != null ? play.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        SerializedObject startSo = new SerializedObject(startScene);
        startSo.FindProperty("playLabel").objectReferenceValue = playLabel;
        startSo.FindProperty("replayLevelOneButton").objectReferenceValue = newJourney;
        startSo.ApplyModifiedPropertiesWithoutUndo();

        StyleButton(play);
        StyleButton(newJourney);
        StyleButton(quit);

        if (loader != null && loader.loadingScreen != null)
        {
            CanvasGroup loadingGroup = EnsureComponent<CanvasGroup>(loader.loadingScreen);
            loadingGroup.interactable = true;
            EnsureComponent<UIPanelTransition>(loader.loadingScreen);
        }

        EditorUtility.SetDirty(canvasGroup);
        EditorUtility.SetDirty(presentation);
        EditorUtility.SetDirty(startScene);
    }

    private static void UpgradeGameplayScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Canvas canvas = FindFirstInScene<Canvas>();

        if (player == null)
        {
            Debug.LogError("[Ellen Production] Gameplay scene is missing Player tag.");
            return;
        }

        if (canvas != null) ConfigureCanvas(canvas);

        PlayerMovement movement = EnsureComponent<PlayerMovement>(player);
        PlayerHealth health = EnsureComponent<PlayerHealth>(player);
        PlayerAdvancedMovement advanced = EnsureComponent<PlayerAdvancedMovement>(player);
        PlayerAbilityController abilities = EnsureComponent<PlayerAbilityController>(player);
        PlayerRespawnController respawn = EnsureComponent<PlayerRespawnController>(player);
        PlayerMovementFeedback feedback = EnsureComponent<PlayerMovementFeedback>(player);
        PlayerDamagePresenter damagePresenter = EnsureComponent<PlayerDamagePresenter>(player);

        Transform wallCheck = player.transform.Find("[WallCheck]");
        if (wallCheck == null)
        {
            GameObject wallCheckObject = new GameObject("[WallCheck]");
            Undo.RegisterCreatedObjectUndo(wallCheckObject, "Create wall check");
            wallCheckObject.transform.SetParent(player.transform, false);
            wallCheckObject.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            wallCheck = wallCheckObject.transform;
        }

        SerializedObject advancedSo = new SerializedObject(advanced);
        advancedSo.FindProperty("wallCheck").objectReferenceValue = wallCheck;
        advancedSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject productionRoot = FindInScene("[Production]");
        if (productionRoot == null)
        {
            productionRoot = new GameObject("[Production]");
            Undo.RegisterCreatedObjectUndo(productionRoot, "Create production root");
        }

        GameSession session = EnsureComponent<GameSession>(productionRoot);
        SpiritWorldController spirit = EnsureComponent<SpiritWorldController>(productionRoot);
        LevelFlowController flow = EnsureComponent<LevelFlowController>(productionRoot);
        VerticalSliceDirector director = EnsureComponent<VerticalSliceDirector>(productionRoot);
        GameplayBootstrap bootstrap = EnsureComponent<GameplayBootstrap>(productionRoot);
        HitStop hitStop = EnsureComponent<HitStop>(productionRoot);

        SerializedObject bootstrapSo = new SerializedObject(bootstrap);
        bootstrapSo.FindProperty("gameSession").objectReferenceValue = session;
        bootstrapSo.FindProperty("playerHealth").objectReferenceValue = health;
        bootstrapSo.FindProperty("abilities").objectReferenceValue = abilities;
        bootstrapSo.FindProperty("spiritWorld").objectReferenceValue = spirit;
        bootstrapSo.FindProperty("levelFlow").objectReferenceValue = flow;
        bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject damageSo = new SerializedObject(damagePresenter);
        damageSo.FindProperty("health").objectReferenceValue = health;
        damageSo.FindProperty("hitStop").objectReferenceValue = hitStop;
        damageSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject feedbackSo = new SerializedObject(feedback);
        feedbackSo.FindProperty("movement").objectReferenceValue = movement;
        feedbackSo.FindProperty("advancedMovement").objectReferenceValue = advanced;
        feedbackSo.ApplyModifiedPropertiesWithoutUndo();

        if (canvas != null)
            EnsureProductionHud(canvas, spirit);

        AddPanelTransition("LostPanel");
        AddPanelTransition("WinPanel");
        AddPanelTransition("StopPanel");

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(productionRoot);
        EditorUtility.SetDirty(respawn);
        EditorUtility.SetDirty(director);
    }

    private static void EnsureProductionHud(Canvas canvas, SpiritWorldController spirit)
    {
        GameObject existing = FindInScene("ProductionHUD");
        if (existing != null) return;

        GameObject hud = new GameObject("ProductionHUD", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(hud, "Create Production HUD");
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.SetParent(canvas.transform, false);
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        hudRect.SetAsLastSibling();

        GameObject spiritPanel = CreateImage("SpiritHUD", hudRect, new Color(0.025f, 0.035f, 0.055f, 0.86f));
        RectTransform panelRect = spiritPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-28f, -28f);
        panelRect.sizeDelta = new Vector2(300f, 88f);

        GameObject stateObject = CreateText("SpiritState", panelRect, "MATERIAL", 22f);
        RectTransform stateRect = stateObject.GetComponent<RectTransform>();
        stateRect.anchorMin = new Vector2(0f, 1f);
        stateRect.anchorMax = new Vector2(1f, 1f);
        stateRect.pivot = new Vector2(0.5f, 1f);
        stateRect.anchoredPosition = new Vector2(0f, -12f);
        stateRect.sizeDelta = new Vector2(-28f, 28f);

        GameObject barBack = CreateImage("EnergyBack", panelRect, new Color(0f, 0f, 0f, 0.42f));
        RectTransform backRect = barBack.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0f);
        backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.anchoredPosition = new Vector2(0f, 14f);
        backRect.sizeDelta = new Vector2(-34f, 18f);

        GameObject fillObject = CreateImage("EnergyFill", backRect, new Color(0.35f, 0.86f, 0.95f, 1f));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        Image fill = fillObject.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;

        GameObject indicator = CreateImage("SpiritActive", panelRect, new Color(0.65f, 0.9f, 1f, 1f));
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
        indicatorRect.anchorMin = indicatorRect.anchorMax = new Vector2(1f, 1f);
        indicatorRect.pivot = new Vector2(1f, 1f);
        indicatorRect.anchoredPosition = new Vector2(-10f, -10f);
        indicatorRect.sizeDelta = new Vector2(10f, 10f);

        SpiritHudPresenter presenter = EnsureComponent<SpiritHudPresenter>(spiritPanel);
        SerializedObject presenterSo = new SerializedObject(presenter);
        presenterSo.FindProperty("spiritWorld").objectReferenceValue = spirit;
        presenterSo.FindProperty("energyFill").objectReferenceValue = fill;
        presenterSo.FindProperty("activeIndicator").objectReferenceValue = indicator;
        presenterSo.FindProperty("stateText").objectReferenceValue = stateObject.GetComponent<TextMeshProUGUI>();
        presenterSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddPanelTransition(string panelName)
    {
        GameObject panel = FindInScene(panelName);
        if (panel == null) return;

        EnsureComponent<CanvasGroup>(panel);
        EnsureComponent<UIPanelTransition>(panel);
    }

    private static void ConfigureCanvas(Canvas canvas)
    {
        CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void StyleButton(GameObject target)
    {
        if (target == null) return;
        Button button = target.GetComponent<Button>();
        if (button == null) return;

        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.82f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.62f, 0.82f, 0.9f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private static GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return go;
    }

    private static GameObject CreateText(string name, Transform parent, string value, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return go;
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(go);
    }

    private static T FindFirstInScene<T>() where T : Component
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(true);
            if (component != null) return component;
        }
        return null;
    }

    private static GameObject FindInScene(string name)
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

    private static RectTransform[] CollectRects(params string[] names)
    {
        List<RectTransform> result = new List<RectTransform>();
        foreach (string name in names)
        {
            GameObject go = FindInScene(name);
            if (go != null && go.TryGetComponent(out RectTransform rect))
                result.Add(rect);
        }
        return result.ToArray();
    }

    private static Transform[] CollectTransforms(params string[] names)
    {
        List<Transform> result = new List<Transform>();
        foreach (string name in names)
        {
            GameObject go = FindInScene(name);
            if (go != null) result.Add(go.transform);
        }
        return result.ToArray();
    }

    private static void SetObjectArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        property.arraySize = values != null ? values.Length : 0;
        if (values == null) return;

        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
#endif
