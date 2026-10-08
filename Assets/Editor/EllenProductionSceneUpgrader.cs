#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EllenProductionSceneUpgrader
{
    private const string StartingScenePath = "Assets/Scenes/StartingScene.unity";
    private const string OneScenePath = "Assets/Scenes/OneScene.unity";
    private const string TwoScenePath = "Assets/Scenes/TwoScene.unity";

    [MenuItem("Ellen/Production/Upgrade All Scenes")]
    public static void UpgradeAllScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string previousScene = SceneManager.GetActiveScene().path;

        UpgradeMainMenu(StartingScenePath);
        UpgradeGameplayScene(OneScenePath);
        UpgradeGameplayScene(TwoScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(previousScene))
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);

        Debug.Log("[Ellen Production] StartingScene, OneScene and TwoScene upgraded. Review changes and run Play Mode preflight before committing scene files.");
    }

    [MenuItem("Ellen/Production/Validate Migrated Scenes")]
    public static void ValidateMigratedScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string previousScene = SceneManager.GetActiveScene().path;
        int errors = 0;

        errors += ValidateMenuScene(StartingScenePath);
        errors += ValidateGameplayScene(OneScenePath);
        errors += ValidateGameplayScene(TwoScenePath);

        if (!string.IsNullOrEmpty(previousScene))
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);

        if (errors == 0)
            Debug.Log("[Ellen Production] Scene migration validation passed.");
        else
            Debug.LogError("[Ellen Production] Scene migration validation found " + errors + " issue(s).");
    }

    private static int ValidateMenuScene(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int errors = 0;

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing Canvas.");
            return 1;
        }

        if (canvas.GetComponent<MainMenuPresentation>() == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing MainMenuPresentation.");
            errors++;
        }

        if (UnityEngine.Object.FindFirstObjectByType<StartScene>() == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing StartScene controller.");
            errors++;
        }

        if (FindSceneObjectByName(scene, "ReplayLevelOne") == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing ReplayLevelOne button.");
            errors++;
        }

        errors += ValidateCanvasScaler(canvas, scene.name, new Vector2(800f, 500f));
        return errors;
    }

    private static int ValidateGameplayScene(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int errors = 0;
        GameObject player = FindPlayer();

        if (player == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing Player.");
            return 1;
        }

        errors += RequireComponent<PlayerMovement>(player, scene.name);
        errors += RequireComponent<PlayerHealth>(player, scene.name);
        errors += RequireComponent<PlayerAdvancedMovement>(player, scene.name);
        errors += RequireComponent<PlayerAbilityController>(player, scene.name);
        errors += RequireComponent<PlayerRespawnController>(player, scene.name);
        errors += RequireComponent<PlayerDamagePresenter>(player, scene.name);
        errors += RequireComponent<PlayerMovementFeedback>(player, scene.name);

        GameObject root = GameObject.Find("[Production]");
        if (root == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing [Production] root.");
            errors++;
        }
        else
        {
            errors += RequireComponent<GameSession>(root, scene.name);
            errors += RequireComponent<SpiritWorldController>(root, scene.name);
            errors += RequireComponent<LevelFlowController>(root, scene.name);
            errors += RequireComponent<GameplayBootstrap>(root, scene.name);
        }

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] " + scene.name + " missing Canvas.");
            errors++;
        }
        else
        {
            errors += ValidateCanvasScaler(canvas, scene.name, new Vector2(1920f, 1080f));
            Transform hud = canvas.transform.Find("ProductionHUD");
            if (hud == null)
            {
                Debug.LogError("[Ellen Production] " + scene.name + " missing ProductionHUD.");
                errors++;
            }
            else
            {
                if (hud.GetComponent<SafeAreaFitter>() == null)
                {
                    Debug.LogError("[Ellen Production] " + scene.name + " ProductionHUD missing SafeAreaFitter.");
                    errors++;
                }
                if (hud.Find("SpiritButton") == null)
                {
                    Debug.LogError("[Ellen Production] " + scene.name + " missing SpiritButton.");
                    errors++;
                }
            }
        }

        foreach (string panelName in new[] { "WinPanel", "LostPanel", "StopPanel" })
        {
            GameObject panel = FindSceneObjectByName(scene, panelName);
            if (panel != null && panel.GetComponent<UIPanelTransition>() == null)
            {
                Debug.LogError("[Ellen Production] " + scene.name + " " + panelName + " missing UIPanelTransition.");
                errors++;
            }
        }

        return errors;
    }

    private static int RequireComponent<T>(GameObject target, string sceneName) where T : Component
    {
        if (target.GetComponent<T>() != null) return 0;
        Debug.LogError("[Ellen Production] " + sceneName + " missing " + typeof(T).Name + " on " + target.name + ".");
        return 1;
    }

    private static int ValidateCanvasScaler(Canvas canvas, string sceneName, Vector2 expectedReferenceResolution)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogError("[Ellen Production] " + sceneName + " missing CanvasScaler.");
            return 1;
        }

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
            || scaler.referenceResolution != expectedReferenceResolution
            || Mathf.Abs(scaler.matchWidthOrHeight - 0.5f) > 0.001f)
        {
            Debug.LogError("[Ellen Production] " + sceneName + " CanvasScaler is not production configured.");
            return 1;
        }

        return 0;
    }

    private static void UpgradeMainMenu(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] StartingScene has no Canvas.");
            return;
        }

        ConfigureCanvas(canvas, new Vector2(800f, 500f));

        CanvasGroup group = Ensure<CanvasGroup>(canvas.gameObject);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        MainMenuPresentation presentation = Ensure<MainMenuPresentation>(canvas.gameObject);
        RectTransform title = FindRectTransform(canvas.transform, "StoryOfEllen");
        if (title == null) title = FindRectTransform(canvas.transform, "StoryOfEllen (1)");
        SetObjectReference(presentation, "title", title);

        StartScene controller = UnityEngine.Object.FindFirstObjectByType<StartScene>();
        GameObject playObject = FindSceneObjectByName(scene, "Play");
        if (controller != null && playObject != null)
        {
            TextMeshProUGUI playLabel = playObject.GetComponentInChildren<TextMeshProUGUI>(true);
            SetObjectReference(controller, "playLabel", playLabel);

            GameObject replayObject = FindSceneObjectByName(scene, "ReplayLevelOne");
            if (replayObject == null)
            {
                replayObject = UnityEngine.Object.Instantiate(playObject, playObject.transform.parent);
                Undo.RegisterCreatedObjectUndo(replayObject, "Create Replay Level 1 button");
                replayObject.name = "ReplayLevelOne";

                RectTransform replayRect = replayObject.GetComponent<RectTransform>();
                RectTransform playRect = playObject.GetComponent<RectTransform>();
                if (replayRect != null && playRect != null)
                {
                    replayRect.anchoredPosition = new Vector2(playRect.anchoredPosition.x, 79f);
                    replayRect.sizeDelta = new Vector2(-520f, playRect.sizeDelta.y);
                }

                Button replayButton = replayObject.GetComponent<Button>();
                if (replayButton != null)
                {
                    replayButton.onClick = new Button.ButtonClickedEvent();
                    UnityEventTools.AddPersistentListener(replayButton.onClick, controller.StartFromBeginning);
                }

                TextMeshProUGUI replayLabel = replayObject.GetComponentInChildren<TextMeshProUGUI>(true);
                if (replayLabel != null) replayLabel.text = "REPLAY LEVEL 1";
            }

            SetObjectReference(controller, "replayLevelOneButton", replayObject);
            replayObject.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void UpgradeGameplayScene(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        GameObject player = FindPlayer();
        if (player == null)
        {
            Debug.LogError("[Ellen Production] Player not found in " + scene.name);
            return;
        }

        PlayerHealth health = Ensure<PlayerHealth>(player);
        PlayerAdvancedMovement advancedMovement = Ensure<PlayerAdvancedMovement>(player);
        PlayerAbilityController abilities = Ensure<PlayerAbilityController>(player);
        PlayerRespawnController respawn = Ensure<PlayerRespawnController>(player);
        PlayerDamagePresenter damagePresenter = Ensure<PlayerDamagePresenter>(player);
        PlayerMovementFeedback movementFeedback = Ensure<PlayerMovementFeedback>(player);

        Transform wallCheck = player.transform.Find("WallCheck");
        if (wallCheck == null)
        {
            GameObject wallCheckObject = new GameObject("WallCheck");
            wallCheck = wallCheckObject.transform;
            wallCheck.SetParent(player.transform, false);
            wallCheck.localPosition = new Vector3(0.45f, 0.1f, 0f);
        }
        SetObjectReference(advancedMovement, "wallCheck", wallCheck);

        GameObject ground = FindByNameOrTag("Ground", "Grounds");
        if (ground != null)
            SetInt(advancedMovement, "wallLayer", 1 << ground.layer);

        ScenesManager legacyScenes = UnityEngine.Object.FindFirstObjectByType<ScenesManager>();
        if (legacyScenes != null && legacyScenes.respawnPoint != null)
            SetObjectReference(respawn, "fallbackRespawnPoint", legacyScenes.respawnPoint);

        GameObject root = GameObject.Find("[Production]");
        if (root == null) root = new GameObject("[Production]");

        GameSession session = Ensure<GameSession>(root);
        SpiritWorldController spirit = Ensure<SpiritWorldController>(root);
        LevelFlowController flow = Ensure<LevelFlowController>(root);
        SetString(flow, "levelId", scene.name);
        SetInt(flow, "levelNumber", scene.name == "OneScene" ? 1 : 2);
        int memoryCount = UnityEngine.Object.FindObjectsByType<MemoryFragment>(FindObjectsSortMode.None).Length;
        SetInt(flow, "totalMemories", memoryCount);
        SetInt(flow, "requiredMemories", 0);
        SetInt(flow, "requiredSecrets", 0);
        SetFloat(flow, "sRankTime", scene.name == "OneScene" ? 120f : 150f);
        SetFloat(flow, "aRankTime", scene.name == "OneScene" ? 180f : 220f);

        Ensure<VerticalSliceDirector>(root);
        Ensure<GameplayBootstrap>(root);

        LevelResultRecorder recorder = Ensure<LevelResultRecorder>(root);
        SetObjectReference(recorder, "flow", flow);

        GameObject presentationRoot = FindOrCreateChild(root.transform, "Presentation");
        CameraJuice cameraJuice = Ensure<CameraJuice>(presentationRoot);
        HitStop hitStop = Ensure<HitStop>(presentationRoot);

        Camera mainCamera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (mainCamera != null)
            SetObjectReference(cameraJuice, "cameraTransform", mainCamera.transform);

        SetObjectReference(damagePresenter, "health", health);
        SetObjectReference(damagePresenter, "cameraJuice", cameraJuice);
        SetObjectReference(damagePresenter, "hitStop", hitStop);
        SetObjectReference(movementFeedback, "movement", player.GetComponent<PlayerMovement>());
        SetObjectReference(movementFeedback, "advancedMovement", advancedMovement);
        SetObjectReference(movementFeedback, "cameraJuice", cameraJuice);

        SpiritWorldPresentation spiritPresentation = Ensure<SpiritWorldPresentation>(presentationRoot);
        SetObjectReference(spiritPresentation, "spiritWorld", spirit);
        SetObjectReference(spiritPresentation, "cameraJuice", cameraJuice);
        SetSpriteRendererArray(spiritPresentation, "tintedSprites",
            UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .Where(x => x.name.IndexOf("Background", StringComparison.OrdinalIgnoreCase) >= 0
                         || x.name.IndexOf("Light", StringComparison.OrdinalIgnoreCase) >= 0)
                .Distinct()
                .ToArray());

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            ConfigureCanvas(canvas, new Vector2(1920f, 1080f));
            BuildProductionHud(canvas, flow, spirit, abilities, damagePresenter, scene);
        }

        ConfigurePanelTransition(scene, "WinPanel");
        ConfigurePanelTransition(scene, "LostPanel");
        ConfigurePanelTransition(scene, "StopPanel");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildProductionHud(Canvas canvas, LevelFlowController flow, SpiritWorldController spirit, PlayerAbilityController abilities, PlayerDamagePresenter damagePresenter, Scene scene)
    {
        Transform existing = canvas.transform.Find("ProductionHUD");
        if (existing != null) return;

        GameObject hud = CreateUiObject("ProductionHUD", canvas.transform);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        Ensure<SafeAreaFitter>(hud);

        GameObject objectivesPanel = CreatePanel("Objectives", hud.transform, new Vector2(22f, -22f), new Vector2(330f, 122f), false);
        TextMeshProUGUI memories = CreateText("Memories", objectivesPanel.transform, new Vector2(14f, -14f), "Memories 0/0");
        TextMeshProUGUI secrets = CreateText("Secrets", objectivesPanel.transform, new Vector2(14f, -48f), "Secrets 0/0");
        TextMeshProUGUI boss = CreateText("Boss", objectivesPanel.transform, new Vector2(14f, -82f), "Defeat the Guardian");

        ObjectiveTrackerPresenter tracker = Ensure<ObjectiveTrackerPresenter>(hud);
        SetObjectReference(tracker, "flow", flow);
        SetObjectReference(tracker, "root", objectivesPanel);
        SetObjectReference(tracker, "memoriesText", memories);
        SetObjectReference(tracker, "secretsText", secrets);
        SetObjectReference(tracker, "bossText", boss);

        GameObject spiritPanel = CreatePanel("Spirit", hud.transform, new Vector2(-22f, -22f), new Vector2(280f, 82f), true);
        TextMeshProUGUI state = CreateText("State", spiritPanel.transform, new Vector2(14f, -12f), "MATERIAL");
        state.fontSize = 20f;

        GameObject energyBackground = CreateUiObject("EnergyBackground", spiritPanel.transform);
        RectTransform energyBgRect = energyBackground.GetComponent<RectTransform>();
        energyBgRect.anchorMin = new Vector2(0f, 1f);
        energyBgRect.anchorMax = new Vector2(0f, 1f);
        energyBgRect.pivot = new Vector2(0f, 1f);
        energyBgRect.anchoredPosition = new Vector2(14f, -48f);
        energyBgRect.sizeDelta = new Vector2(250f, 16f);
        Image energyBg = energyBackground.AddComponent<Image>();
        energyBg.sprite = BuiltinUiSprite();
        energyBg.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject energyFillObject = CreateUiObject("EnergyFill", energyBackground.transform);
        RectTransform energyFillRect = energyFillObject.GetComponent<RectTransform>();
        energyFillRect.anchorMin = Vector2.zero;
        energyFillRect.anchorMax = Vector2.one;
        energyFillRect.offsetMin = new Vector2(2f, 2f);
        energyFillRect.offsetMax = new Vector2(-2f, -2f);
        Image energyFill = energyFillObject.AddComponent<Image>();
        energyFill.sprite = BuiltinUiSprite();
        energyFill.type = Image.Type.Filled;
        energyFill.fillMethod = Image.FillMethod.Horizontal;
        energyFill.fillAmount = 1f;
        energyFill.color = new Color(0.35f, 0.82f, 1f, 1f);

        GameObject indicator = CreateUiObject("SpiritActive", spiritPanel.transform);
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(1f, 1f);
        indicatorRect.anchorMax = new Vector2(1f, 1f);
        indicatorRect.pivot = new Vector2(1f, 1f);
        indicatorRect.anchoredPosition = new Vector2(-14f, -14f);
        indicatorRect.sizeDelta = new Vector2(14f, 14f);
        Image indicatorImage = indicator.AddComponent<Image>();
        indicatorImage.sprite = BuiltinUiSprite();
        indicatorImage.color = new Color(0.45f, 0.9f, 1f, 1f);

        SpiritHudPresenter spiritHud = Ensure<SpiritHudPresenter>(spiritPanel);
        SetObjectReference(spiritHud, "spiritWorld", spirit);
        SetObjectReference(spiritHud, "energyFill", energyFill);
        SetObjectReference(spiritHud, "activeIndicator", indicator);
        SetObjectReference(spiritHud, "stateText", state);

        GameObject spiritButtonObject = CreatePanel("SpiritButton", hud.transform, new Vector2(-70f, 92f), new Vector2(150f, 54f), true, false);
        Button spiritButton = spiritButtonObject.AddComponent<Button>();
        spiritButton.targetGraphic = spiritButtonObject.GetComponent<Image>();
        spiritButton.navigation = new Navigation { mode = Navigation.Mode.None };
        TextMeshProUGUI buttonText = CreateCenteredText("Label", spiritButtonObject.transform, "SPIRIT");
        buttonText.fontSize = 19f;
        UnityEventTools.AddPersistentListener(spiritButton.onClick, abilities.TryToggleSpirit);

        GameObject tutorialPanel = CreateUiObject("SpiritTutorial", hud.transform);
        RectTransform tutorialRect = tutorialPanel.GetComponent<RectTransform>();
        tutorialRect.anchorMin = new Vector2(0.5f, 1f);
        tutorialRect.anchorMax = new Vector2(0.5f, 1f);
        tutorialRect.pivot = new Vector2(0.5f, 1f);
        tutorialRect.anchoredPosition = new Vector2(0f, -120f);
        tutorialRect.sizeDelta = new Vector2(560f, 72f);
        Image tutorialBackground = tutorialPanel.AddComponent<Image>();
        tutorialBackground.sprite = BuiltinUiSprite();
        tutorialBackground.color = new Color(0.03f, 0.045f, 0.07f, 0.92f);
        CanvasGroup tutorialGroup = tutorialPanel.AddComponent<CanvasGroup>();
        tutorialGroup.alpha = 0f;
        tutorialGroup.blocksRaycasts = false;
        tutorialGroup.interactable = false;
        TextMeshProUGUI tutorialText = CreateCenteredText("Message", tutorialPanel.transform, "Spirit World reveals hidden paths.");
        tutorialText.fontSize = 17f;

        SpiritTutorialPresenter tutorialPresenter = Ensure<SpiritTutorialPresenter>(hud);
        SetObjectReference(tutorialPresenter, "spiritWorld", spirit);
        SetObjectReference(tutorialPresenter, "panel", tutorialGroup);
        SetObjectReference(tutorialPresenter, "message", tutorialText);

        GameObject deathOverlayObject = CreateUiObject("DeathOverlay", hud.transform);
        RectTransform deathRect = deathOverlayObject.GetComponent<RectTransform>();
        deathRect.anchorMin = Vector2.zero;
        deathRect.anchorMax = Vector2.one;
        deathRect.offsetMin = Vector2.zero;
        deathRect.offsetMax = Vector2.zero;
        Image deathImage = deathOverlayObject.AddComponent<Image>();
        deathImage.sprite = BuiltinUiSprite();
        deathImage.color = new Color(0.25f, 0.015f, 0.02f, 0.5f);
        deathImage.raycastTarget = false;
        CanvasGroup deathGroup = deathOverlayObject.AddComponent<CanvasGroup>();
        deathGroup.alpha = 0f;
        deathGroup.blocksRaycasts = false;
        deathGroup.interactable = false;
        SetObjectReference(damagePresenter, "deathOverlay", deathGroup);

        deathOverlayObject.transform.SetAsFirstSibling();

        GameObject winPanel = FindSceneObjectByName(scene, "WinPanel");
        if (winPanel != null)
            BuildResultSummary(hud, winPanel, flow);
    }

    private static void BuildResultSummary(GameObject hud, GameObject winPanel, LevelFlowController flow)
    {
        Transform existing = winPanel.transform.Find("ProductionResults");
        GameObject summary = existing != null ? existing.gameObject : CreateUiObject("ProductionResults", winPanel.transform);

        RectTransform rect = summary.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(380f, 250f);

        Image background = summary.GetComponent<Image>();
        if (background == null) background = summary.AddComponent<Image>();
        background.sprite = BuiltinUiSprite();
        background.color = new Color(0.03f, 0.045f, 0.07f, 0.9f);

        TextMeshProUGUI title = FindOrCreateResultText(summary.transform, "Title", new Vector2(18f, -18f), "LEVEL COMPLETE", 24f);
        TextMeshProUGUI rank = FindOrCreateResultText(summary.transform, "Rank", new Vector2(18f, -58f), "A", 42f);
        TextMeshProUGUI time = FindOrCreateResultText(summary.transform, "Time", new Vector2(18f, -118f), "Time", 18f);
        TextMeshProUGUI deaths = FindOrCreateResultText(summary.transform, "Deaths", new Vector2(18f, -150f), "Deaths", 18f);
        TextMeshProUGUI memories = FindOrCreateResultText(summary.transform, "Memories", new Vector2(18f, -182f), "Memories", 18f);
        TextMeshProUGUI secrets = FindOrCreateResultText(summary.transform, "Secrets", new Vector2(18f, -214f), "Secrets", 18f);

        title.alignment = TextAlignmentOptions.Left;

        LevelResultPresenter presenter = Ensure<LevelResultPresenter>(hud);
        SetObjectReference(presenter, "flow", flow);
        SetObjectReference(presenter, "panel", winPanel);
        SetObjectReference(presenter, "rankText", rank);
        SetObjectReference(presenter, "timeText", time);
        SetObjectReference(presenter, "deathsText", deaths);
        SetObjectReference(presenter, "memoriesText", memories);
        SetObjectReference(presenter, "secretsText", secrets);
    }

    private static TextMeshProUGUI FindOrCreateResultText(Transform parent, string name, Vector2 position, string text, float fontSize)
    {
        Transform existing = parent.Find(name);
        TextMeshProUGUI label;

        if (existing != null)
            label = existing.GetComponent<TextMeshProUGUI>();
        else
            label = CreateText(name, parent, position, text);

        if (label == null) return null;
        label.text = text;
        label.fontSize = fontSize;
        return label;
    }

    private static void ConfigurePanelTransition(Scene scene, string panelName)
    {
        GameObject panel = FindSceneObjectByName(scene, panelName);
        if (panel == null) return;

        CanvasGroup group = Ensure<CanvasGroup>(panel);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        UIPanelTransition transition = Ensure<UIPanelTransition>(panel);
        SetObjectReference(transition, "panel", panel.transform as RectTransform);
        SetObjectReference(transition, "group", group);
    }

    private static GameObject FindPlayer()
    {
        try
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) return tagged;
        }
        catch { }

        return GameObject.Find("Player");
    }

    private static GameObject FindSceneObjectByName(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child.gameObject;
        }
        return null;
    }

    private static GameObject FindByNameOrTag(string name, string tag)
    {
        GameObject byName = GameObject.Find(name);
        if (byName != null) return byName;
        try { return GameObject.FindGameObjectWithTag(tag); }
        catch { return null; }
    }

    private static void ConfigureCanvas(Canvas canvas, Vector2 referenceResolution)
    {
        CanvasScaler scaler = Ensure<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
        EditorUtility.SetDirty(scaler);
    }

    private static T Ensure<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create " + name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 position, Vector2 size, bool rightAnchored, bool topAnchored = true)
    {
        GameObject panel = CreateUiObject(name, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();

        Vector2 anchor = rightAnchored ? new Vector2(1f, topAnchored ? 1f : 0f) : new Vector2(0f, topAnchored ? 1f : 0f);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.sprite = BuiltinUiSprite();
        image.color = new Color(0.04f, 0.055f, 0.08f, 0.78f);
        return panel;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, string text)
    {
        GameObject go = CreateUiObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(-28f, 28f);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = 18f;
        label.alignment = TextAlignmentOptions.Left;
        label.raycastTarget = false;
        return label;
    }

    private static TextMeshProUGUI CreateCenteredText(string name, Transform parent, string text)
    {
        GameObject go = CreateUiObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = 18f;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static Sprite BuiltinUiSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static RectTransform FindRectTransform(Transform root, string targetName)
    {
        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            if (rect.name == targetName) return rect;
        return null;
    }

    private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetInt(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.stringValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetSpriteRendererArray(UnityEngine.Object target, string propertyName, SpriteRenderer[] values)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }
}
#endif
