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

if errors:
    print("Ellen content asset preflight FAILED:")
    for error in errors:
        print(" -", error)
    sys.exit(1)

print(f"Ellen content asset preflight PASSED: {len(ASSETS)} critical assets checked.")
print("Unity compilation and interactive game testing are still required.")
