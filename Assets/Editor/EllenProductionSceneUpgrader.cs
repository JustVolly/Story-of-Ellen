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

    private static void UpgradeMainMenu(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Ellen Production] StartingScene has no Canvas.");
            return;
        }

        CanvasGroup group = Ensure<CanvasGroup>(canvas.gameObject);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        MainMenuPresentation presentation = Ensure<MainMenuPresentation>(canvas.gameObject);
        RectTransform title = FindRectTransform(canvas.transform, "StoryOfEllen");
        if (title == null) title = FindRectTransform(canvas.transform, "StoryOfEllen (1)");
        SetObjectReference(presentation, "title", title);

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
            BuildProductionHud(canvas, flow, spirit, abilities);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildProductionHud(Canvas canvas, LevelFlowController flow, SpiritWorldController spirit, PlayerAbilityController abilities)
    {
        Transform existing = canvas.transform.Find("ProductionHUD");
        if (existing != null) return;

        GameObject hud = CreateUiObject("ProductionHUD", canvas.transform);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        GameObject objectivesPanel = CreatePanel("Objectives", hud.transform, new Vector2(22f, -22f), new Vector2(330f, 122f), false);
        TextMeshProUGUI memories = CreateText("Memories", objectivesPanel.transform, new Vector2(14f, -14f), "Memories 0/0");
        TextMeshProUGUI secrets = CreateText("Secrets", objectivesPanel.transform, new Vector2(14f, -48f), "Secrets 0/0");
        TextMeshProUGUI boss = CreateText("Boss", objectivesPanel.transform, new Vector2(14f, -82f), "Defeat the Guardian");

        ObjectiveTrackerPresenter tracker = Ensure<ObjectiveTrackerPresenter>(objectivesPanel);
        SetObjectReference(tracker, "flow", flow);
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

        GameObject spiritButtonObject = CreatePanel("SpiritButton", hud.transform, new Vector2(-28f, 28f), new Vector2(150f, 58f), true, false);
        Button spiritButton = spiritButtonObject.AddComponent<Button>();
        spiritButton.targetGraphic = spiritButtonObject.GetComponent<Image>();
        spiritButton.navigation = new Navigation { mode = Navigation.Mode.None };
        TextMeshProUGUI buttonText = CreateCenteredText("Label", spiritButtonObject.transform, "SPIRIT");
        buttonText.fontSize = 19f;
        UnityEventTools.AddPersistentListener(spiritButton.onClick, abilities.TryToggleSpirit);
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

    private static GameObject FindByNameOrTag(string name, string tag)
    {
        GameObject byName = GameObject.Find(name);
        if (byName != null) return byName;
        try { return GameObject.FindGameObjectWithTag(tag); }
        catch { return null; }
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
