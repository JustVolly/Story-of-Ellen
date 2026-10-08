#!/usr/bin/env python3
"""Repository-only preflight for the asset-backed Ellen production campaign.

Does not replace a Unity compilation, scene bake, renderer or Play Mode test.
"""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parent.parent

ASSETS = [
    "Assets/Scenes/StartingScene.unity",
    "Assets/Scenes/OneScene.unity",
    "Assets/Scenes/TwoScene.unity",
    "Assets/Scenes/ThreeScene.unity",
    "Assets/Scenes/ThreeScene.unity.meta",
    "Assets/Prefabs/Snail.prefab",
    "Assets/Environment/Sprites/Crypt/Sprite_Ruins_1.png",
    "Assets/Environment/Sprites/Crypt/Sprite_Pillar_1_Color.png",
    "Assets/Environment/Sprites/Crypt/Sprite_Wall_1_Color.png",
    "Assets/Environment/Sprites/Altar/Altar Packed/Sprite_Altar_1.png",
    "Assets/Audio/SFX/Ambience/SFX_Ambience_Forest_Day_Loop.wav",
    "Assets/Audio/SFX/Ambience/SFX_Ambience_Forest_Night_Loop.wav",
    "Assets/Audio/SFX/Ambience/SFX_Ambience_Temple_Loop.wav",
    "Assets/BigManJD/Platformer Tileset - Pixelart Grasslands/Prefabs/Tree1.prefab",
    "Assets/BigManJD/Platformer Tileset - Pixelart Grasslands/Prefabs/Bush1.prefab",
    "Assets/BigManJD/Platformer Tileset - Pixelart Grasslands/Prefabs/Rock1.prefab",
    "Assets/BigManJD/Platformer Tileset - Pixelart Grasslands/Prefabs/WoodenSign.prefab",
    "Assets/Scripts/Production/Audio/EllenAmbienceVolume.cs",
    "Assets/Scripts/Production/Audio/EllenAmbienceVolume.cs.meta",
    "Assets/Scripts/Production/Player/EllenGameplayInput.cs",
    "Assets/Scripts/Production/Player/EllenGameplayInput.cs.meta",
    "Assets/Scripts/Production/Player/EllenFallRecovery.cs",
    "Assets/Scripts/Production/Player/EllenFallRecovery.cs.meta",
    "Assets/Scripts/Production/Spirit/SpiritWell.cs",
    "Assets/Scripts/Production/Spirit/SpiritWell.cs.meta",
    "Assets/Scripts/Production/World/AbilitySeal.cs",
    "Assets/Scripts/Production/World/AbilitySeal.cs.meta",
    "Assets/Editor/EllenProductionSceneInstaller.cs",
    "Assets/Editor/EllenProductionLevelDesigner.cs",
    "Assets/Editor/EllenLevelArtBuilder.cs",
    "Assets/Editor/EllenLevelArtBuilder.cs.meta",
    "Assets/Editor/EllenCampaignContentBuilder.cs",
    "Assets/Editor/EllenCampaignContentBuilder.cs.meta",
    "ProjectSettings/EditorBuildSettings.asset",
]
for folder, count in [
    ("!_Moutain", 4),
    ("2_Desert", 5),
    ("3_Graveyard", 5),
]:
    for layer in range(count):
        ASSETS.append(f"Assets/Background/{folder}/Layer_{layer}.png")

errors = []
for path in ASSETS:
    if not (ROOT / path).is_file():
        errors.append(f"Missing asset: {path}")

for name in ("EllenLevelArtBuilder", "EllenCampaignContentBuilder"):
    meta = ROOT / "Assets" / "Editor" / (name + ".cs.meta")
    if meta.exists() and not re.search(r"(?m)^guid: [0-9a-f]{32}$", meta.read_text()):
        errors.append(f"Invalid script .meta GUID: {name}")

settings_path = ROOT / "ProjectSettings" / "EditorBuildSettings.asset"
if settings_path.exists():
    settings = settings_path.read_text()
    for scene in ("StartingScene", "OneScene", "TwoScene", "ThreeScene"):
        if f"enabled: 1\n    path: Assets/Scenes/{scene}.unity" not in settings:
            errors.append(f"Scene disabled or missing from Build Settings: {scene}")

designer_path = ROOT / "Assets/Editor/EllenProductionLevelDesigner.cs"
if designer_path.exists():
    source = designer_path.read_text()
    for token in (
        "BuildSpiritDiscovery(", "BuildDashMastery(", "BuildSpiritAscent(",
        "CreateWallJumpShaft(", "CreateGuardianArena(", "CreateMemory(",
        "CreateSpiritGate(", "CreateDashBarrier(", "CreateCompletionTrigger(",
        "Assets/Prefabs/Snail.prefab",
        "CreateSpiritWell(", "CreateAbilitySeal(",
        "AbilitySeal_SpiritTutorial", "AbilitySeal_DashTutorial",
        "AbilitySeal_SpiritChain", "AbilitySeal_DashChain",
    ):
        if token not in source:
            errors.append(f"Level designer missing feature: {token}")

campaign_path = ROOT / "Assets/Editor/EllenCampaignContentBuilder.cs"
if campaign_path.exists():
    source = campaign_path.read_text()
    for token in ("BuildCompleteCampaign()", "ValidateCompleteCampaign()",
                  "EllenLevelArtBuilder.Build(", "EllenProductionSceneInstaller.UpgradeBuildScenes()"):
        if token not in source:
            errors.append(f"Campaign builder missing feature: {token}")

# These checks catch disconnected runtime polish during subsequent scene work.
# They are wiring/source checks, NOT substitutes for Unity Play Mode tests.
required_wiring = {
    "Assets/Editor/EllenProductionSceneInstaller.cs": (
        "EnsureComponent<EllenGameplayInput>(player)",
        "EnsureComponent<EllenFallRecovery>(player)",
    ),
    "Assets/Editor/EllenCampaignContentBuilder.cs": (
        "ExpectComponent<EllenGameplayInput>(scene)",
        "ExpectComponent<EllenFallRecovery>(scene)",
    ),
    "Assets/Scripts/Production/Progression/ProgressionSave.cs": (
        "ResetForNewJourney()", "musicVolume = current.musicVolume",
        "sfxVolume = current.sfxVolume", "data.campaignCompleted = true",
    ),
    "Assets/Scripts/StartScene.cs": ("ProgressionSave.ResetForNewJourney()",),
    "Assets/Scripts/Production/Player/EllenGameplayInput.cs": (
        "TryDash()", "TryWallJump()", "TryToggleSpirit()",
        "AttackStart()", "PauseRequested()", "OnPressUp_W()",
    ),
    "Assets/Scripts/Production/Player/EllenFallRecovery.cs": (
        "deathPlaneY = -85f", "health.Kill()",
    ),
    "Assets/Scripts/Production/Spirit/SpiritGate.cs": (
        "SolidifyWhenPlayerLeaves()", "PlayerInsideOriginalBounds()",
        "blockingCollider.enabled = false",
    ),
    "Assets/Scripts/Production/Boss/BossAttackPattern.cs": (
        "warningTint", "telegraphDuration", "recoveryDuration",
    ),
    "Assets/Scripts/PlayerMovement.cs": ("ResetForRespawn()",),
    "Assets/Scripts/Production/Player/PlayerRespawnController.cs": (
        "ResetForRespawn()",
    ),
    "Assets/Scripts/CanvasControl.cs": (
        "waitingForRespawn", "CurrentTime = Mathf.Max(1, TotalTime)",
    ),
    "Assets/Scripts/Production/Spirit/SpiritWorldController.cs": (
        "float RestoreEnergy(float amount)", "EnergyChanged?.Invoke(NormalizedEnergy)",
    ),
    "Assets/Scripts/Production/Spirit/SpiritWell.cs": (
        "spiritWorld.RestoreEnergy(energyGranted)", "Time.time + cooldown",
        "OnTriggerStay2D", "rechargeEffect.Play()",
    ),
    "Assets/Scripts/Production/World/AbilitySeal.cs": (
        "RequiredAction.SpiritWorld", "IsPlayerDashing(other)",
        "Refresh()", "activationEffect.Play()",
    ),
    "Assets/Scripts/PlayerBulletDamage.cs": (
        "GetComponentInParent<EnemyHealth>()",
        "productionHealth.TakeDamage(1)",
    ),
    "Assets/Editor/EllenLevelArtBuilder.cs": (
        "bool spiritWell = item.name.StartsWith(", "bool abilitySeal = item.name.StartsWith(",
        'so.FindProperty("indicator")',
    ),
    "Assets/Editor/EllenCampaignContentBuilder.cs": (
        "ValidateAbilityPuzzles(scene)", "expectedSeals", "expectedWells",
        "ExpectReference(so,",
    ),
}
for path, tokens in required_wiring.items():
    file = ROOT / path
    if not file.is_file():
        errors.append(f"Missing gameplay script: {path}")
        continue
    source = file.read_text(encoding="utf-8")
    for token in tokens:
        if token not in source:
            errors.append(f"Disconnected gameplay wiring: {path}: {token}")

# Prevent new scripts from accidentally aliasing existing .meta identifiers.
new_metas = [
    ROOT / "Assets/Editor/EllenCampaignContentBuilder.cs.meta",
    ROOT / "Assets/Editor/EllenLevelArtBuilder.cs.meta",
    ROOT / "Assets/Scripts/Production/Audio/EllenAmbienceVolume.cs.meta",
    ROOT / "Assets/Scripts/Production/Player/EllenGameplayInput.cs.meta",
    ROOT / "Assets/Scripts/Production/Player/EllenFallRecovery.cs.meta",
    ROOT / "Assets/Scripts/Production/Spirit/SpiritWell.cs.meta",
    ROOT / "Assets/Scripts/Production/World/AbilitySeal.cs.meta",
]
new_guids = {}
for meta in new_metas:
    if not meta.is_file():
        errors.append(f"Missing Unity GUID metadata: {meta}")
        continue
    match = re.search(r"(?m)^guid: ([0-9a-f]{32})$", meta.read_text())
    if not match:
        errors.append(f"Invalid Unity GUID metadata: {meta}")
    elif match.group(1) in new_guids:
        errors.append(f"Duplicate newly created GUID: {meta} and {new_guids[match.group(1)]}")
    else:
        new_guids[match.group(1)] = meta

if errors:
    print("Ellen content asset preflight FAILED:")
    for error in errors:
        print(" -", error)
    sys.exit(1)

print(f"Ellen content asset preflight PASSED: {len(ASSETS)} critical assets checked.")
print("Unity compilation and interactive game testing are still required.")
