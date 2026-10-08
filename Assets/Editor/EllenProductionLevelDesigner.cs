#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EllenProductionLevelDesigner
{
    private const string RootName = "[LevelDesign]";

    [MenuItem("Ellen/Production/Build Current Level Design")]
    public static void BuildCurrentLevelDesign()
    {
        ApplyToActiveScene();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Ellen/Production/Rebuild Current Level Design")]
    public static void RebuildCurrentLevelDesign()
    {
        GameObject existing = FindInScene(RootName);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject bossHud = FindInScene("BossHUD");
        if (bossHud != null) Object.DestroyImmediate(bossHud);

        ApplyToActiveScene();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    public static void ApplyToActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "OneScene" && scene.name != "TwoScene" && scene.name != "ThreeScene") return;

        if (FindInScene(RootName) != null)
        {
            Debug.Log("[Ellen Level Design] Production layer already exists in " + scene.name);
            return;
        }

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Create production level design");

        LevelFlowController flow = FindFirstInScene<LevelFlowController>();
        VerticalSliceDirector director = FindFirstInScene<VerticalSliceDirector>();
        SpiritWorldController spirit = FindFirstInScene<SpiritWorldController>();
        PlayerAbilityController abilities = FindFirstInScene<PlayerAbilityController>();

        if (flow == null || director == null || spirit == null || abilities == null)
        {
            Debug.LogError("[Ellen Level Design] Run Ellen/Production/Upgrade Current Scene first.");
            Object.DestroyImmediate(root);
            return;
        }

        if (scene.name == "OneScene")
            BuildSpiritDiscovery(root, flow, director, spirit);
        else if (scene.name == "TwoScene")
            BuildDashMastery(root, flow, director, abilities);
        else
            BuildSpiritAscent(root, flow, director, spirit);

        EditorUtility.SetDirty(root);
        Debug.Log("[Ellen Level Design] " + scene.name + " production layer built.");
    }

    private static void BuildSpiritDiscovery(
        GameObject root,
        LevelFlowController flow,
        VerticalSliceDirector director,
        SpiritWorldController spirit)
    {
        ConfigureFlow(flow, "SpiritDiscovery", 1, 1, 0, false, 115f, 180f, 3);

        CreateBeat(root.transform, "Beat_SpiritDiscovery", new Vector2(18f, -37f), director, VerticalSliceDirector.Beat.SpiritDiscovery);
        CreateBeat(root.transform, "Beat_Shrine", new Vector2(188f, -37f), director, VerticalSliceDirector.Beat.Shrine);
        CreateBeat(root.transform, "Beat_Memory", new Vector2(312f, -37f), director, VerticalSliceDirector.Beat.Memory);
        CreateBeat(root.transform, "Beat_Combat", new Vector2(430f, -37f), director, VerticalSliceDirector.Beat.Combat);
        CreateBeat(root.transform, "Beat_Traversal", new Vector2(558f, -37f), director, VerticalSliceDirector.Beat.Traversal);

        CreateSpiritWell(root.transform, "SpiritWell_Tutorial", new Vector2(70f, -38f), spirit);
        CreateAbilitySeal(root.transform, "AbilitySeal_SpiritTutorial",
            new Vector2(92f, -36.5f), new Vector2(1.4f, 11f),
            AbilitySeal.RequiredAction.SpiritWorld, spirit);
        CreateSpiritGate(root.transform, "SpiritGate_Intro", new Vector2(112f, -36.5f), new Vector2(1.5f, 12f), spirit);
        CreateSpiritGate(root.transform, "SpiritGate_Final", new Vector2(646f, -36.5f), new Vector2(1.5f, 12f), spirit);

        CreateMemory(root.transform, "Memory_01_HighPath", new Vector2(95f, -13.2f));
        CreateMemory(root.transform, "Memory_02_Midpoint", new Vector2(332f, -38.0f));
        CreateMemory(root.transform, "Memory_03_FinalApproach", new Vector2(585f, -38.0f));

        CreateShrine(root.transform, "Shrine_01", new Vector2(210f, -39.0f));
        CreateShrine(root.transform, "Shrine_02", new Vector2(505f, -39.0f));

        CreateSecret(root.transform, "Secret_UpperRuins", new Vector2(196f, -20f), new Vector2(8f, 6f));

        CreateObjectiveBarrier(
            root.transform,
            "MemoryObjectiveGate",
            new Vector2(704f, -36.5f),
            new Vector2(1.6f, 12f),
            flow);

        CreateCompletionTrigger(root.transform, new Vector2(718f, -38f), flow);

        CreateLandmarkParticles(root.transform, "SpiritLandmark_Intro", new Vector2(35f, -34f), new Color(0.35f, 0.9f, 1f, 0.75f));
        CreateLandmarkParticles(root.transform, "SpiritLandmark_Final", new Vector2(680f, -34f), new Color(0.65f, 0.55f, 1f, 0.75f));
    }

    private static void BuildDashMastery(
        GameObject root,
        LevelFlowController flow,
        VerticalSliceDirector director,
        PlayerAbilityController abilities)
    {
        ConfigureFlow(flow, "DashMastery", 2, 0, 0, true, 130f, 205f, 3);

        CreateBeat(root.transform, "Beat_Ability", new Vector2(-4f, -37f), director, VerticalSliceDirector.Beat.Ability);
        CreateBeat(root.transform, "Beat_Traversal", new Vector2(102f, -37f), director, VerticalSliceDirector.Beat.Traversal);
        CreateBeat(root.transform, "Beat_Combat", new Vector2(352f, -37f), director, VerticalSliceDirector.Beat.Combat);
        CreateBeat(root.transform, "Beat_Boss", new Vector2(668f, -37f), director, VerticalSliceDirector.Beat.Boss);

        CreateAbilitySeal(root.transform, "AbilitySeal_DashTutorial",
            new Vector2(135f, -36.5f), new Vector2(1.4f, 11f),
            AbilitySeal.RequiredAction.Dash, null);
        CreateDashBarrier(root.transform, "DashBarrier_01", new Vector2(160f, -36.5f), new Vector2(1.6f, 12f));
        CreateDashBarrier(root.transform, "DashBarrier_02", new Vector2(520f, -36.5f), new Vector2(1.6f, 12f));

        CreateMemory(root.transform, "Memory_01_AfterDash", new Vector2(112f, -29.5f));
        CreateMemory(root.transform, "Memory_02_CombatRoute", new Vector2(402f, -38.0f));
        CreateMemory(root.transform, "Memory_03_BossApproach", new Vector2(640f, -38.0f));

        CreateShrine(root.transform, "Shrine_01", new Vector2(300f, -39.0f));
        CreateShrine(root.transform, "Shrine_02", new Vector2(610f, -39.0f));
        CreateSecret(root.transform, "Secret_DashReturn", new Vector2(438f, -35f), new Vector2(9f, 7f));

        CreateLandmarkParticles(root.transform, "DashLandmark_01", new Vector2(150f, -33f), new Color(0.9f, 0.65f, 0.25f, 0.8f));
        CreateLandmarkParticles(root.transform, "DashLandmark_Boss", new Vector2(690f, -32f), new Color(0.9f, 0.25f, 0.35f, 0.8f));

        CreateGuardianArena(root.transform, flow, director);
        CreateCompletionTrigger(root.transform, new Vector2(786f, -38f), flow);
    }

    private static void BuildSpiritAscent(
        GameObject root,
        LevelFlowController flow,
        VerticalSliceDirector director,
        SpiritWorldController spirit)
    {
        ConfigureFlow(flow, "SpiritAscent", 3, 2, 0, false, 125f, 195f, 3);

        CreateBeat(root.transform, "Beat_WallJump", new Vector2(18f, -37f), director, VerticalSliceDirector.Beat.Ability);
        CreateBeat(root.transform, "Beat_Ascent", new Vector2(118f, -37f), director, VerticalSliceDirector.Beat.Traversal);
        CreateBeat(root.transform, "Beat_SpiritChain", new Vector2(300f, -37f), director, VerticalSliceDirector.Beat.SpiritDiscovery);
        CreateBeat(root.transform, "Beat_DashChain", new Vector2(438f, -37f), director, VerticalSliceDirector.Beat.Combat);
        CreateBeat(root.transform, "Beat_FinalAscent", new Vector2(558f, -37f), director, VerticalSliceDirector.Beat.Traversal);

        CreateShrine(root.transform, "Shrine_01", new Vector2(72f, -39f));
        CreateWallJumpShaft(root.transform, "WallJumpShaft_01", 126f, -24f, 12f, 40f);
        CreateMemory(root.transform, "Memory_01_FirstAscent", new Vector2(126f, -6.5f));

        CreateSecret(root.transform, "Secret_SpiritBalcony", new Vector2(248f, -19f), new Vector2(9f, 7f));
        CreateSpiritWell(root.transform, "SpiritWell_Chain", new Vector2(280f, -38f), spirit);
        CreateAbilitySeal(root.transform, "AbilitySeal_SpiritChain",
            new Vector2(300f, -36.5f), new Vector2(1.4f, 11f),
            AbilitySeal.RequiredAction.SpiritWorld, spirit);
        CreateSpiritGate(root.transform, "SpiritGate_Chain", new Vector2(326f, -36.5f), new Vector2(1.6f, 12f), spirit);
        CreateMemory(root.transform, "Memory_02_SpiritChain", new Vector2(365f, -25f));

        CreateAbilitySeal(root.transform, "AbilitySeal_DashChain",
            new Vector2(435f, -36.5f), new Vector2(1.4f, 11f),
            AbilitySeal.RequiredAction.Dash, spirit);
        CreateDashBarrier(root.transform, "DashBarrier_Chain", new Vector2(458f, -36.5f), new Vector2(1.6f, 12f));
        CreateShrine(root.transform, "Shrine_02", new Vector2(505f, -39f));

        CreateSpiritWell(root.transform, "SpiritWell_FinalAscent", new Vector2(554f, -38f), spirit);
        CreateWallJumpShaft(root.transform, "WallJumpShaft_02", 585f, -24f, 12f, 40f);
        CreateMemory(root.transform, "Memory_03_FinalAscent", new Vector2(585f, -6.5f));

        CreateObjectiveBarrier(
            root.transform,
            "MasteryObjectiveGate",
            new Vector2(690f, -36.5f),
            new Vector2(1.6f, 12f),
            flow);

        CreateCompletionTrigger(root.transform, new Vector2(716f, -38f), flow);

        CreateLandmarkParticles(root.transform, "AscentLandmark_01", new Vector2(126f, -8f), new Color(0.4f, 0.95f, 0.75f, 0.8f));
        CreateLandmarkParticles(root.transform, "AscentLandmark_02", new Vector2(585f, -8f), new Color(0.55f, 0.65f, 1f, 0.8f));
    }

    private static void ConfigureFlow(
        LevelFlowController flow,
        string levelId,
        int levelNumber,
        int requiredMemories,
        int requiredSecrets,
        bool requireBoss,
        float sTime,
        float aTime,
        int totalMemories)
    {
        SerializedObject so = new SerializedObject(flow);
        so.FindProperty("levelId").stringValue = levelId;
        so.FindProperty("levelNumber").intValue = levelNumber;
        so.FindProperty("requiredMemories").intValue = requiredMemories;
        so.FindProperty("requiredSecrets").intValue = requiredSecrets;
        so.FindProperty("requireBossDefeat").boolValue = requireBoss;
        so.FindProperty("sRankTime").floatValue = sTime;
        so.FindProperty("aRankTime").floatValue = aTime;
        so.FindProperty("totalMemories").intValue = totalMemories;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(flow);
    }

    private static void CreateBeat(
        Transform parent,
        string name,
        Vector2 position,
        VerticalSliceDirector director,
        VerticalSliceDirector.Beat beat)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D zone = go.AddComponent<BoxCollider2D>();
        zone.isTrigger = true;
        zone.size = new Vector2(4f, 18f);

        LevelBeatTrigger trigger = go.AddComponent<LevelBeatTrigger>();
        SerializedObject so = new SerializedObject(trigger);
        so.FindProperty("director").objectReferenceValue = director;
        so.FindProperty("beat").enumValueIndex = (int)beat;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateMemory(Transform parent, string name, Vector2 position)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        CircleCollider2D trigger = go.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.15f;

        go.AddComponent<MemoryFragment>();
        CreateLoopingParticles(go.transform, "Aura", ColorForMemory(), 0.12f, 22f, new Vector3(1.3f, 1.3f, 0.3f));
    }

    private static void CreateShrine(Transform parent, string name, Vector2 position)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2.6f, 4.5f);

        GameObject respawn = new GameObject("RespawnPoint");
        respawn.transform.SetParent(go.transform, false);
        respawn.transform.localPosition = Vector3.up * 0.5f;

        ParticleSystem activation = CreateBurstParticles(
            go.transform,
            "ActivationFX",
            new Color(0.45f, 1f, 0.85f, 0.9f),
            0.18f,
            34);

        SpiritShrine shrine = go.AddComponent<SpiritShrine>();
        SerializedObject so = new SerializedObject(shrine);
        so.FindProperty("activationEffect").objectReferenceValue = activation;
        so.FindProperty("respawnPoint").objectReferenceValue = respawn.transform;
        so.ApplyModifiedPropertiesWithoutUndo();

        CreateLoopingParticles(go.transform, "IdleAura", new Color(0.25f, 0.8f, 0.7f, 0.55f), 0.08f, 8f, new Vector3(1.4f, 3f, 0.3f));
    }

    private static void CreateSecret(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = size;

        GameObject reveal = new GameObject("RevealFX");
        reveal.transform.SetParent(go.transform, false);
        CreateLoopingParticles(reveal.transform, "Particles", new Color(0.85f, 0.65f, 1f, 0.75f), 0.1f, 12f, new Vector3(size.x * 0.4f, size.y * 0.4f, 0.3f));
        reveal.SetActive(false);

        SecretArea secret = go.AddComponent<SecretArea>();
        SerializedObject so = new SerializedObject(secret);
        so.FindProperty("revealEffect").objectReferenceValue = reveal;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateSpiritWell(
        Transform parent, string name, Vector2 position, SpiritWorldController spirit)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D sensor = go.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        sensor.size = new Vector2(4.8f, 5.8f);

        ParticleSystem recharge = CreateBurstParticles(
            go.transform, "RechargeBurst", new Color(0.3f, 1f, 0.85f, 0.95f), 0.2f, 36);
        CreateLoopingParticles(go.transform, "WellAura",
            new Color(0.25f, 0.9f, 0.8f, 0.8f), 0.13f, 15f,
            new Vector3(2.4f, 4.2f, 0.2f));

        SpiritWell well = go.AddComponent<SpiritWell>();
        SerializedObject so = new SerializedObject(well);
        so.FindProperty("spiritWorld").objectReferenceValue = spirit;
        so.FindProperty("rechargeEffect").objectReferenceValue = recharge;
        so.FindProperty("energyGranted").floatValue = 3f;
        so.FindProperty("cooldown").floatValue = 8f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateAbilitySeal(
        Transform parent, string name, Vector2 position, Vector2 size,
        AbilitySeal.RequiredAction required, SpiritWorldController spirit)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D blocker = go.AddComponent<BoxCollider2D>();
        blocker.size = size;
        blocker.isTrigger = false;

        BoxCollider2D sensor = go.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        sensor.size = new Vector2(size.x + 4f, size.y + 1f);

        GameObject visual = new GameObject("AbilitySealVisual");
        visual.transform.SetParent(go.transform, false);
        Color color = required == AbilitySeal.RequiredAction.SpiritWorld
            ? new Color(0.3f, 0.82f, 1f, 0.85f)
            : new Color(1f, 0.7f, 0.3f, 0.95f);
        CreateLoopingParticles(visual.transform, "SealAura", color,
            0.16f, 22f, new Vector3(size.x, size.y, 0.25f));

        ParticleSystem burst = CreateBurstParticles(
            go.transform, "UnlockBurst", color, 0.23f, 48);
        AbilitySeal seal = go.AddComponent<AbilitySeal>();
        SerializedObject so = new SerializedObject(seal);
        so.FindProperty("requiredAction").enumValueIndex = (int)required;
        so.FindProperty("spiritWorld").objectReferenceValue = spirit;
        so.FindProperty("blockingCollider").objectReferenceValue = blocker;
        so.FindProperty("proximityTrigger").objectReferenceValue = sensor;
        so.FindProperty("lockedVisual").objectReferenceValue = visual;
        so.FindProperty("activationEffect").objectReferenceValue = burst;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateWallJumpShaft(
        Transform parent,
        string name,
        float centerX,
        float centerY,
        float width,
        float wallHeight)
    {
        GameObject root = CreateWorldObject(name, parent, new Vector2(centerX, centerY));

        GameObject left = CreateWorldObject("LeftWall", root.transform, new Vector2(centerX - width * 0.5f, centerY));
        BoxCollider2D leftCollider = left.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(1.2f, wallHeight);

        GameObject right = CreateWorldObject("RightWall", root.transform, new Vector2(centerX + width * 0.5f, centerY));
        BoxCollider2D rightCollider = right.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(1.2f, wallHeight);

        CreateLoopingParticles(
            left.transform,
            "WallAura",
            new Color(0.35f, 0.95f, 0.75f, 0.65f),
            0.09f,
            14f,
            new Vector3(0.8f, wallHeight, 0.3f));

        CreateLoopingParticles(
            right.transform,
            "WallAura",
            new Color(0.35f, 0.95f, 0.75f, 0.65f),
            0.09f,
            14f,
            new Vector3(0.8f, wallHeight, 0.3f));
    }

    private static void CreateSpiritGate(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        SpiritWorldController spirit)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D blocker = go.AddComponent<BoxCollider2D>();
        blocker.isTrigger = false;
        blocker.size = size;

        GameObject visual = new GameObject("SpiritBarrierVisual");
        visual.transform.SetParent(go.transform, false);
        CreateLoopingParticles(visual.transform, "Particles", new Color(0.25f, 0.85f, 1f, 0.85f), 0.11f, 30f, new Vector3(size.x, size.y, 0.3f));

        SpiritGate gate = go.AddComponent<SpiritGate>();
        SerializedObject so = new SerializedObject(gate);
        so.FindProperty("spiritWorld").objectReferenceValue = spirit;
        so.FindProperty("blockingCollider").objectReferenceValue = blocker;
        so.FindProperty("visualRoot").objectReferenceValue = visual;
        so.FindProperty("passableInSpiritWorld").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateDashBarrier(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = CreateWorldObject(name, parent, position);

        BoxCollider2D blocker = go.AddComponent<BoxCollider2D>();
        blocker.isTrigger = false;
        blocker.size = size;

        BoxCollider2D sensor = go.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        sensor.size = new Vector2(size.x + 2.5f, size.y);

        GameObject visual = new GameObject("DashBarrierVisual");
        visual.transform.SetParent(go.transform, false);
        CreateLoopingParticles(visual.transform, "Particles", new Color(1f, 0.55f, 0.2f, 0.9f), 0.12f, 32f, new Vector3(size.x, size.y, 0.3f));

        ParticleSystem breakFx = CreateBurstParticles(
            go.transform,
            "BreakFX",
            new Color(1f, 0.7f, 0.3f, 1f),
            0.2f,
            48);

        DashBreakableBarrier barrier = go.AddComponent<DashBreakableBarrier>();
        SerializedObject so = new SerializedObject(barrier);
        so.FindProperty("blockingCollider").objectReferenceValue = blocker;
        so.FindProperty("triggerCollider").objectReferenceValue = sensor;
        so.FindProperty("visualRoot").objectReferenceValue = visual;
        so.FindProperty("breakEffect").objectReferenceValue = breakFx;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateObjectiveBarrier(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        LevelFlowController flow)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D blocker = go.AddComponent<BoxCollider2D>();
        blocker.size = size;

        GameObject visual = new GameObject("ObjectiveBarrierVisual");
        visual.transform.SetParent(go.transform, false);
        CreateLoopingParticles(visual.transform, "Particles", new Color(0.75f, 0.45f, 1f, 0.9f), 0.11f, 28f, new Vector3(size.x, size.y, 0.3f));

        ObjectiveBarrier barrier = go.AddComponent<ObjectiveBarrier>();
        SerializedObject so = new SerializedObject(barrier);
        so.FindProperty("flow").objectReferenceValue = flow;
        so.FindProperty("blocker").objectReferenceValue = blocker;
        so.FindProperty("visualRoot").objectReferenceValue = visual;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCompletionTrigger(Transform parent, Vector2 position, LevelFlowController flow)
    {
        GameObject go = CreateWorldObject("LevelResultTrigger", parent, position);
        BoxCollider2D zone = go.AddComponent<BoxCollider2D>();
        zone.isTrigger = true;
        zone.size = new Vector2(4f, 14f);

        LevelCompletionTrigger trigger = go.AddComponent<LevelCompletionTrigger>();
        SerializedObject so = new SerializedObject(trigger);
        so.FindProperty("flow").objectReferenceValue = flow;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateGuardianArena(
        Transform parent,
        LevelFlowController flow,
        VerticalSliceDirector director)
    {
        GameObject source = FindInScene("Snail2");
        if (source == null) source = FindInScene("Snail");

        // Some scene variants do not include Snail2. Do not silently create
        // an unbeatable boss level: fall back to the committed enemy prefab.
        if (source == null)
            source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Snail.prefab");
        if (source == null)
            throw new System.InvalidOperationException(
                "Level 2 Guardian needs Snail/Snail2 or Assets/Prefabs/Snail.prefab.");

        GameObject boss = Object.Instantiate(source, parent);
        boss.name = "SpiritGuardian";
        boss.transform.position = new Vector3(730f, -39f, 0f);
        boss.transform.localScale *= 1.55f;
        boss.SetActive(true);

        try { boss.tag = "Enemy"; } catch { }

        foreach (EnemyMovement movement in boss.GetComponentsInChildren<EnemyMovement>(true))
            movement.enabled = false;

        Rigidbody2D body = boss.GetComponent<Rigidbody2D>();
        if (body == null) body = boss.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.simulated = true;
        body.freezeRotation = true;
        body.gravityScale = Mathf.Max(1f, body.gravityScale);

        EnemyHealth health = boss.GetComponent<EnemyHealth>();
        if (health == null) health = boss.AddComponent<EnemyHealth>();

        SerializedObject healthSo = new SerializedObject(health);
        healthSo.FindProperty("maxHealth").intValue = 9;
        healthSo.ApplyModifiedPropertiesWithoutUndo();

        BossController bossController = boss.GetComponent<BossController>();
        if (bossController == null) bossController = boss.AddComponent<BossController>();

        SerializedObject bossSo = new SerializedObject(bossController);
        bossSo.FindProperty("levelFlow").objectReferenceValue = flow;
        bossSo.ApplyModifiedPropertiesWithoutUndo();

        BossAttackPattern pattern = boss.GetComponent<BossAttackPattern>();
        if (pattern == null) pattern = boss.AddComponent<BossAttackPattern>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SerializedObject patternSo = new SerializedObject(pattern);
        patternSo.FindProperty("boss").objectReferenceValue = bossController;
        patternSo.FindProperty("player").objectReferenceValue = player != null ? player.transform : null;
        patternSo.FindProperty("body").objectReferenceValue = body;
        patternSo.FindProperty("phaseOneSpeed").floatValue = 2.4f;
        patternSo.FindProperty("phaseTwoSpeed").floatValue = 3.8f;
        patternSo.FindProperty("phaseThreeSpeed").floatValue = 5.2f;
        patternSo.FindProperty("attackInterval").floatValue = 1.25f;
        patternSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject entranceBarrier = CreateArenaBarrier(parent, "BossEntranceBarrier", new Vector2(665f, -36.5f));
        GameObject exitBarrier = CreateArenaBarrier(parent, "BossExitBarrier", new Vector2(777f, -36.5f));
        GameObject bossHud = CreateBossHud(health, bossController);

        GameObject arena = CreateWorldObject("BossArenaTrigger", parent, new Vector2(680f, -37f));
        BoxCollider2D arenaZone = arena.AddComponent<BoxCollider2D>();
        arenaZone.isTrigger = true;
        arenaZone.size = new Vector2(5f, 18f);

        BossArenaController arenaController = arena.AddComponent<BossArenaController>();
        SerializedObject arenaSo = new SerializedObject(arenaController);
        arenaSo.FindProperty("boss").objectReferenceValue = bossController;
        arenaSo.FindProperty("entranceBarrier").objectReferenceValue = entranceBarrier;
        arenaSo.FindProperty("exitBarrier").objectReferenceValue = exitBarrier;
        arenaSo.FindProperty("bossHud").objectReferenceValue = bossHud;
        arenaSo.FindProperty("bossRoot").objectReferenceValue = boss;
        arenaSo.FindProperty("director").objectReferenceValue = director;
        arenaSo.FindProperty("activateBossOnEnter").boolValue = true;
        arenaSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateArenaBarrier(Transform parent, string name, Vector2 position)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        BoxCollider2D blocker = go.AddComponent<BoxCollider2D>();
        blocker.size = new Vector2(1.6f, 12f);

        CreateLoopingParticles(go.transform, "Particles", new Color(0.95f, 0.2f, 0.3f, 0.9f), 0.12f, 34f, new Vector3(1.5f, 12f, 0.3f));
        return go;
    }

    private static GameObject CreateBossHud(EnemyHealth health, BossController boss)
    {
        GameObject existing = FindInScene("BossHUD");
        if (existing != null) return existing;

        Canvas canvas = FindFirstInScene<Canvas>();
        if (canvas == null) return null;

        GameObject productionHud = FindInScene("ProductionHUD");
        Transform parent = productionHud != null ? productionHud.transform : canvas.transform;

        GameObject root = CreateUiImage("BossHUD", parent, new Color(0.02f, 0.025f, 0.04f, 0.92f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -28f);
        rootRect.sizeDelta = new Vector2(620f, 88f);

        GameObject fillBack = CreateUiImage("BossHealthBack", rootRect, new Color(0f, 0f, 0f, 0.5f));
        RectTransform backRect = fillBack.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0f);
        backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.anchoredPosition = new Vector2(0f, 16f);
        backRect.sizeDelta = new Vector2(-44f, 20f);

        GameObject fillObject = CreateUiImage("BossHealthFill", backRect, new Color(0.9f, 0.25f, 0.28f, 1f));
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

        GameObject phaseObject = CreateUiText("BossPhase", rootRect, "PHASE 1", 22f);
        RectTransform phaseRect = phaseObject.GetComponent<RectTransform>();
        phaseRect.anchorMin = new Vector2(0f, 1f);
        phaseRect.anchorMax = new Vector2(1f, 1f);
        phaseRect.pivot = new Vector2(0.5f, 1f);
        phaseRect.anchoredPosition = new Vector2(0f, -10f);
        phaseRect.sizeDelta = new Vector2(-40f, 30f);

        BossHealthPresenter presenter = root.AddComponent<BossHealthPresenter>();
        SerializedObject so = new SerializedObject(presenter);
        so.FindProperty("bossHealth").objectReferenceValue = health;
        so.FindProperty("boss").objectReferenceValue = boss;
        so.FindProperty("root").objectReferenceValue = root;
        so.FindProperty("fill").objectReferenceValue = fill;
        so.FindProperty("phaseText").objectReferenceValue = phaseObject.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        return root;
    }

    private static void CreateLandmarkParticles(Transform parent, string name, Vector2 position, Color color)
    {
        GameObject go = CreateWorldObject(name, parent, position);
        CreateLoopingParticles(go.transform, "Particles", color, 0.08f, 10f, new Vector3(4f, 7f, 0.3f));
    }

    private static ParticleSystem CreateLoopingParticles(
        Transform parent,
        string name,
        Color color,
        float size,
        float rate,
        Vector3 shapeScale)
    {
        GameObject go = new GameObject(name, typeof(ParticleSystem));
        go.transform.SetParent(parent, false);

        ParticleSystem system = go.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.playOnAwake = true;
        main.duration = 2f;
        main.startLifetime = 1.8f;
        main.startSpeed = 0.25f;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 180;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = shapeScale;

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 60;

        return system;
    }

    private static ParticleSystem CreateBurstParticles(
        Transform parent,
        string name,
        Color color,
        float size,
        short count)
    {
        GameObject go = new GameObject(name, typeof(ParticleSystem));
        go.transform.SetParent(parent, false);

        ParticleSystem system = go.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = 0.8f;
        main.startSpeed = 2.5f;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 70;

        return system;
    }

    private static GameObject CreateWorldObject(string name, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = new Vector3(position.x, position.y, 0f);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static GameObject CreateUiImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static GameObject CreateUiText(string name, Transform parent, string textValue, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static Color ColorForMemory()
    {
        return new Color(0.7f, 0.85f, 1f, 0.9f);
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
}
#endif
