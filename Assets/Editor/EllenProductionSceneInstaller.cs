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
        GameObject settingsButton = FindInScene("Settings");

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

            newJourney.SetActive(false);
        }

        if (settingsButton == null && quit != null)
        {
            settingsButton = Object.Instantiate(quit, quit.transform.parent);
            settingsButton.name = "Settings";
            Undo.RegisterCreatedObjectUndo(settingsButton, "Create Settings button");

            RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
            RectTransform quitRect = quit.GetComponent<RectTransform>();

            if (settingsRect != null && quitRect != null)
            {
                settingsRect.anchoredPosition = quitRect.anchoredPosition;
                quitRect.anchoredPosition += Vector2.down * 42f;
            }

            TextMeshProUGUI label = settingsButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = "SETTINGS";
        }

        MainMenuSettingsController settingsController = EnsureSettingsPanel(canvas, startScene, settingsButton);

        RectTransform[] titleParts = CollectRects("StoryOfEllen", "StoryOfEllen (1)");
        RectTransform[] menuButtons = CollectRects("Play", "NewJourney", "Settings", "Quit");
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
        StyleButton(settingsButton);
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

    private static MainMenuSettingsController EnsureSettingsPanel(Canvas canvas, StartScene startScene, GameObject settingsButton)
    {
        MainMenuSettingsController controller = EnsureComponent<MainMenuSettingsController>(canvas.gameObject);

        GameObject panel = FindInScene("SettingsPanel");
        Slider musicSlider;
        Slider sfxSlider;

        if (panel == null)
        {
            panel = CreateImage("SettingsPanel", canvas.transform, new Color(0.02f, 0.03f, 0.05f, 0.96f));
            Undo.RegisterCreatedObjectUndo(panel, "Create Settings panel");

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(680f, 420f);

            CanvasGroup group = EnsureComponent<CanvasGroup>(panel);
            group.alpha = 1f;
            UIPanelTransition transition = EnsureComponent<UIPanelTransition>(panel);

            GameObject title = CreateText("SettingsTitle", panelRect, "SETTINGS", 34f);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -34f);
            titleRect.sizeDelta = new Vector2(420f, 52f);

            GameObject musicLabel = CreateText("MusicLabel", panelRect, "MUSIC", 22f);
            RectTransform musicLabelRect = musicLabel.GetComponent<RectTransform>();
            musicLabelRect.anchorMin = musicLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            musicLabelRect.anchoredPosition = new Vector2(-220f, 55f);
            musicLabelRect.sizeDelta = new Vector2(160f, 40f);

            musicSlider = CreateSlider("MusicSlider", panelRect);
            RectTransform musicRect = musicSlider.GetComponent<RectTransform>();
            musicRect.anchorMin = musicRect.anchorMax = new Vector2(0.5f, 0.5f);
            musicRect.anchoredPosition = new Vector2(85f, 55f);
            musicRect.sizeDelta = new Vector2(340f, 28f);

            GameObject sfxLabel = CreateText("SfxLabel", panelRect, "SFX", 22f);
            RectTransform sfxLabelRect = sfxLabel.GetComponent<RectTransform>();
            sfxLabelRect.anchorMin = sfxLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            sfxLabelRect.anchoredPosition = new Vector2(-220f, -25f);
            sfxLabelRect.sizeDelta = new Vector2(160f, 40f);

            sfxSlider = CreateSlider("SfxSlider", panelRect);
            RectTransform sfxRect = sfxSlider.GetComponent<RectTransform>();
            sfxRect.anchorMin = sfxRect.anchorMax = new Vector2(0.5f, 0.5f);
            sfxRect.anchoredPosition = new Vector2(85f, -25f);
            sfxRect.sizeDelta = new Vector2(340f, 28f);

            GameObject back = CreateButton("SettingsBack", panelRect, "BACK");
            RectTransform backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 36f);
            backRect.sizeDelta = new Vector2(240f, 58f);

            SerializedObject controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("panel").objectReferenceValue = panel;
            controllerSo.FindProperty("panelTransition").objectReferenceValue = transition;
            controllerSo.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            controllerSo.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            controllerSo.FindProperty("startScene").objectReferenceValue = startScene;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            Button backButton = back.GetComponent<Button>();
            backButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(backButton.onClick, controller.Close);

            musicSlider.onValueChanged = new Slider.SliderEvent();
            sfxSlider.onValueChanged = new Slider.SliderEvent();
            UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, controller.SetMusicVolume);
            UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, controller.SetSfxVolume);

            panel.SetActive(false);
        }
        else
        {
            musicSlider = FindInScene("MusicSlider")?.GetComponent<Slider>();
            sfxSlider = FindInScene("SfxSlider")?.GetComponent<Slider>();
        }

        if (settingsButton != null)
        {
            Button button = settingsButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(button.onClick, controller.Open);
            }
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static Slider CreateSlider(string name, Transform parent)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        GameObject background = CreateImage("Background", sliderObject.transform, new Color(1f, 1f, 1f, 0.15f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5f, 0f);
        fillAreaRect.offsetMax = new Vector2(-5f, 0f);

        GameObject fillObject = CreateImage("Fill", fillAreaRect, new Color(0.35f, 0.86f, 0.95f, 1f));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handleObject = CreateImage("Handle", handleAreaRect, Color.white);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(22f, 32f);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleObject.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static GameObject CreateButton(string name, Transform parent, string label)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.12f);

        GameObject labelObject = CreateText("Label", buttonObject.transform, label, 22f);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        StyleButton(buttonObject);
        return buttonObject;
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
        SpiritWorldPresentation spiritPresentation = EnsureComponent<SpiritWorldPresentation>(productionRoot);
        LevelEntryConfigurator levelEntry = EnsureComponent<LevelEntryConfigurator>(productionRoot);

        SerializedObject levelEntrySo = new SerializedObject(levelEntry);
        bool isLevelTwo = SceneManager.GetActiveScene().name == "TwoScene";
        levelEntrySo.FindProperty("levelNumber").intValue = isLevelTwo ? 2 : 1;
        levelEntrySo.FindProperty("grantOnStart").intValue = isLevelTwo
            ? (int)PlayerAbilityController.Ability.Dash
            : (int)PlayerAbilityController.Ability.None;
        levelEntrySo.FindProperty("abilities").objectReferenceValue = abilities;
        levelEntrySo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject bootstrapSo = new SerializedObject(bootstrap);
        bootstrapSo.FindProperty("gameSession").objectReferenceValue = session;
        bootstrapSo.FindProperty("playerHealth").objectReferenceValue = health;
        bootstrapSo.FindProperty("abilities").objectReferenceValue = abilities;
        bootstrapSo.FindProperty("spiritWorld").objectReferenceValue = spirit;
        bootstrapSo.FindProperty("levelFlow").objectReferenceValue = flow;
        bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject spiritPresentationSo = new SerializedObject(spiritPresentation);
        spiritPresentationSo.FindProperty("spiritWorld").objectReferenceValue = spirit;
        SetObjectArray(spiritPresentationSo.FindProperty("tintedSprites"), CollectBackgroundSprites());
        spiritPresentationSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject damageSo = new SerializedObject(damagePresenter);
        damageSo.FindProperty("health").objectReferenceValue = health;
        damageSo.FindProperty("hitStop").objectReferenceValue = hitStop;
        damageSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject feedbackSo = new SerializedObject(feedback);
        feedbackSo.FindProperty("movement").objectReferenceValue = movement;
        feedbackSo.FindProperty("advancedMovement").objectReferenceValue = advanced;
        feedbackSo.ApplyModifiedPropertiesWithoutUndo();

        if (canvas != null)
            EnsureProductionHud(canvas, spirit, flow, abilities);

        AddPanelTransition("LostPanel");
        AddPanelTransition("WinPanel");
        AddPanelTransition("StopPanel");

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(productionRoot);
        EditorUtility.SetDirty(respawn);
        EditorUtility.SetDirty(director);
    }

    private static void EnsureProductionHud(Canvas canvas, SpiritWorldController spirit, LevelFlowController flow, PlayerAbilityController abilities)
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
        Image spiritPanelImage = spiritPanel.GetComponent<Image>();
        spiritPanelImage.raycastTarget = true;
        Button spiritToggle = EnsureComponent<Button>(spiritPanel);
        spiritToggle.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(spiritToggle.onClick, abilities.TryToggleSpirit);
        StyleButton(spiritPanel);

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

        GameObject objectiveHost = new GameObject("ObjectiveHUD", typeof(RectTransform));
        objectiveHost.transform.SetParent(hudRect, false);
        RectTransform objectiveHostRect = objectiveHost.GetComponent<RectTransform>();
        objectiveHostRect.anchorMin = Vector2.zero;
        objectiveHostRect.anchorMax = Vector2.one;
        objectiveHostRect.offsetMin = Vector2.zero;
        objectiveHostRect.offsetMax = Vector2.zero;

        GameObject objectiveVisual = CreateImage("ObjectiveVisual", objectiveHostRect, new Color(0.025f, 0.035f, 0.055f, 0.82f));
        RectTransform objectiveRect = objectiveVisual.GetComponent<RectTransform>();
        objectiveRect.anchorMin = objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot = new Vector2(1f, 1f);
        objectiveRect.anchoredPosition = new Vector2(-28f, -126f);
        objectiveRect.sizeDelta = new Vector2(300f, 132f);

        GameObject memoriesObject = CreateText("MemoriesObjective", objectiveRect, "Memories 0/0", 18f);
        RectTransform memoriesRect = memoriesObject.GetComponent<RectTransform>();
        memoriesRect.anchorMin = memoriesRect.anchorMax = new Vector2(0.5f, 1f);
        memoriesRect.anchoredPosition = new Vector2(0f, -28f);
        memoriesRect.sizeDelta = new Vector2(260f, 28f);

        GameObject secretsObject = CreateText("SecretsObjective", objectiveRect, "Secrets 0/0", 18f);
        RectTransform secretsRect = secretsObject.GetComponent<RectTransform>();
        secretsRect.anchorMin = secretsRect.anchorMax = new Vector2(0.5f, 1f);
        secretsRect.anchoredPosition = new Vector2(0f, -65f);
        secretsRect.sizeDelta = new Vector2(260f, 28f);

        GameObject bossObject = CreateText("BossObjective", objectiveRect, "Defeat the Guardian", 18f);
        RectTransform bossRect = bossObject.GetComponent<RectTransform>();
        bossRect.anchorMin = bossRect.anchorMax = new Vector2(0.5f, 1f);
        bossRect.anchoredPosition = new Vector2(0f, -102f);
        bossRect.sizeDelta = new Vector2(260f, 28f);

        ObjectiveTrackerPresenter objectivePresenter = EnsureComponent<ObjectiveTrackerPresenter>(objectiveHost);
        SerializedObject objectiveSo = new SerializedObject(objectivePresenter);
        objectiveSo.FindProperty("flow").objectReferenceValue = flow;
        objectiveSo.FindProperty("memoriesText").objectReferenceValue = memoriesObject.GetComponent<TextMeshProUGUI>();
        objectiveSo.FindProperty("secretsText").objectReferenceValue = secretsObject.GetComponent<TextMeshProUGUI>();
        objectiveSo.FindProperty("bossText").objectReferenceValue = bossObject.GetComponent<TextMeshProUGUI>();
        objectiveSo.FindProperty("visualRoot").objectReferenceValue = objectiveVisual;
        objectiveSo.ApplyModifiedPropertiesWithoutUndo();

        objectiveVisual.SetActive(false);

        GameObject abilityToast = CreateImage("AbilityToast", hudRect, new Color(0.025f, 0.035f, 0.055f, 0.92f));
        RectTransform toastRect = abilityToast.GetComponent<RectTransform>();
        toastRect.anchorMin = toastRect.anchorMax = new Vector2(0.5f, 1f);
        toastRect.pivot = new Vector2(0.5f, 1f);
        toastRect.anchoredPosition = new Vector2(0f, -30f);
        toastRect.sizeDelta = new Vector2(560f, 108f);

        CanvasGroup toastGroup = EnsureComponent<CanvasGroup>(abilityToast);
        toastGroup.alpha = 0f;
        toastGroup.interactable = false;
        toastGroup.blocksRaycasts = false;

        GameObject abilityTitle = CreateText("AbilityTitle", toastRect, "ABILITY UNLOCKED", 25f);
        RectTransform abilityTitleRect = abilityTitle.GetComponent<RectTransform>();
        abilityTitleRect.anchorMin = abilityTitleRect.anchorMax = new Vector2(0.5f, 1f);
        abilityTitleRect.anchoredPosition = new Vector2(0f, -18f);
        abilityTitleRect.sizeDelta = new Vector2(520f, 34f);

        GameObject abilityDescription = CreateText("AbilityDescription", toastRect, "", 17f);
        RectTransform abilityDescriptionRect = abilityDescription.GetComponent<RectTransform>();
        abilityDescriptionRect.anchorMin = abilityDescriptionRect.anchorMax = new Vector2(0.5f, 0f);
        abilityDescriptionRect.anchoredPosition = new Vector2(0f, 20f);
        abilityDescriptionRect.sizeDelta = new Vector2(520f, 38f);

        AbilityUnlockPresenter abilityPresenter = EnsureComponent<AbilityUnlockPresenter>(abilityToast);
        SerializedObject abilitySo = new SerializedObject(abilityPresenter);
        abilitySo.FindProperty("abilities").objectReferenceValue = abilities;
        abilitySo.FindProperty("panel").objectReferenceValue = toastGroup;
        abilitySo.FindProperty("title").objectReferenceValue = abilityTitle.GetComponent<TextMeshProUGUI>();
        abilitySo.FindProperty("description").objectReferenceValue = abilityDescription.GetComponent<TextMeshProUGUI>();
        abilitySo.ApplyModifiedPropertiesWithoutUndo();
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

    private static SpriteRenderer[] CollectBackgroundSprites()
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null) continue;
                if (renderer.gameObject.name.IndexOf("Background", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                result.Add(renderer);
            }
        }

        return result.ToArray();
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
