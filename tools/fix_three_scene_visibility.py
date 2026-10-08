#!/usr/bin/env python3
"""Align authored Level 3 sprite geometry with its physical hitbox.

Simple SpriteRenderer mode ignores m_Size. Switch generated crypt geometry
to sliced mode and use the BoxCollider2D's LOCAL dimensions, preserving
the existing transform scale. Changes only [AstralCryptAuthored] descendants.
"""
from pathlib import Path
import re

SCENE = Path(__file__).resolve().parents[1] / "Assets/Scenes/ThreeScene.unity"
START = "--- !u!1 &8104000000000000000\n"
END = "--- !u!1660057539 &9223372036854775807\nSceneRoots:"


def main() -> None:
    original = SCENE.read_text(encoding="utf-8")
    if original.count(START) != 1 or original.count(END) != 1:
        raise RuntimeError("Expected authored Unity scene layout not found.")
    a, b = original.index(START), original.index(END)
    region = original[a:b]
    group_heads = list(re.finditer(r"^--- !u!1 &(\d+)\n", region, re.M))
    rebuilt = []
    count = 0

    for i, match in enumerate(group_heads):
        end = (group_heads[i + 1].start()
               if i + 1 < len(group_heads) else len(region))
        group = region[match.start():end]
        name_match = re.search(r"^  m_Name: (.+)$", group, re.M)
        name = name_match.group(1) if name_match else ""
        if not (name.startswith("Astral_Obstacle_")
                or name.startswith("Astral_Platform_")
                or name.startswith("Astral_Hazard_")
                or name.startswith("Astral_MovingPlatform_")
                or name.startswith("Astral_Sentinel_")
                or name.startswith("Astral_Expansion_")):
            rebuilt.append(group)
            continue

        collider = re.search(
            r"^--- !u!61 &\d+\nBoxCollider2D:[\s\S]*?"
            r"(?=^--- !u!|\Z)", group, re.M)
        renderer = re.search(
            r"^--- !u!212 &\d+\nSpriteRenderer:[\s\S]*?"
            r"(?=^--- !u!|\Z)", group, re.M)
        if collider is None or renderer is None:
            raise RuntimeError("Missing renderer or collision for " + name)
        collider_size = re.search(
            r"^  m_Size: (\{x: [^}]+\})$", collider.group(), re.M)
        if collider_size is None:
            raise RuntimeError("Collider dimensions missing for " + name)

        replacement = renderer.group()
        replacement, one = re.subn(
            r"^  m_DrawMode: [012]$", "  m_DrawMode: 1",
            replacement, count=1, flags=re.M)
        replacement, two = re.subn(
            r"^  m_Size: \{[^}]+\}$",
            "  m_Size: " + collider_size.group(1),
            replacement, count=1, flags=re.M)
        if one != 1 or two != 1:
            raise RuntimeError("Cannot update sprite geometry for " + name)

        # Ground Tilemap is on sorting layer index 6; Default is behind it.
        replacement = re.sub(r"^  m_SortingLayerID: -?\d+$",
                             "  m_SortingLayerID: -2062554445",
                             replacement, count=1, flags=re.M)
        replacement = re.sub(r"^  m_SortingLayer: -?\d+$",
                             "  m_SortingLayer: 6",
                             replacement, count=1, flags=re.M)

        # Ground's actual Terrain Sliced (16x16) green/brown tile atlas.
        if ("Obstacle" in name or "Barricade" in name or
                "Platform" in name or "SkySteps" in name or "EchoLift" in name):
            terrain = "fc584d329b77b9545b9c9c8bb829e553"
            dirt = "Obstacle" in name or "Barricade" in name
            sprite = "8704577166008928185" if dirt else "-7517053698926566003"
            replacement = re.sub(r"^  m_Sprite: \{[^}]+\}$",
                f"  m_Sprite: {{fileID: {sprite}, guid: {terrain}, type: 3}}",
                replacement, count=1, flags=re.M)
            replacement = re.sub(r"^  m_DrawMode: \d+$", "  m_DrawMode: 2",
                                 replacement, count=1, flags=re.M)
            replacement = re.sub(r"^  m_Color: \{[^}]+\}$",
                                 "  m_Color: {r: 1, g: 1, b: 1, a: 1}",
                                 replacement, count=1, flags=re.M)
        elif "Hazard" in name or "RuneTrap" in name:
            # Project sprite already used by Assets/Animation/spikes.asset.
            replacement = re.sub(r"^  m_Sprite: \{[^}]+\}$",
                "  m_Sprite: {fileID: 21300000, guid: 0049772a1acaf4a49b69bb504ffe2669, type: 3}",
                replacement, count=1, flags=re.M)

        group = group[:renderer.start()] + replacement + group[renderer.end():]
        rebuilt.append(group)
        count += 1

    updated = original[:a] + "".join(rebuilt) + original[b:]
    if count != 48:
        raise RuntimeError(f"Expected exactly 48 renderable gameplay objects, got {count}")
    if updated != original:
        SCENE.write_text(updated, encoding="utf-8", newline="")
        print(f"ThreeScene: aligned {count} sprite dimensions with physics.")
    else:
        print(f"ThreeScene: all {count} sprite dimensions already aligned.")


if __name__ == "__main__":
    main()
