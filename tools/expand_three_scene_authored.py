#!/usr/bin/env python3
"""Expand already baked ThreeScene with visible, functional Level 3 encounters.

Preserves the original scene, all prefab links and the 26 authored objects.
Idempotent: a second run detects the expansion and changes nothing.
"""
from __future__ import annotations

from collections import Counter
from pathlib import Path
import re

PROJECT = Path(__file__).resolve().parents[1]
SCENE = PROJECT / "Assets/Scenes/ThreeScene.unity"
ROOT_MARK = "--- !u!1 &8104000000000000000\n"
SCENE_ROOTS = "--- !u!1660057539 &9223372036854775807\nSceneRoots:"
PREFIX = "Astral_Expansion_"

TEMPLATES = {
    "barricade": "8104000000000000002",
    "platform": "8104000000000000018",
    "hazard": "8104000000000000093",
    "lift": "8104000000000000108",
    "sentinel": "8104000000000000120",
}
SPECS = (
    [("barricade", (x, -39.6)) for x in (119, 245, 465, 638)]
    + [("platform", (x, y)) for x, y in (
        (229, -35), (236, -31), (243, -27), (250, -23), (257, -19),
        (440, -34), (448, -30), (456, -26), (464, -22), (472, -18),
    )]
    + [("hazard", (x, -41.5)) for x in (134, 315, 480, 588)]
    + [("lift", (190, -30)), ("lift", (640, -28))]
    + [("sentinel", (185, -37.5)), ("sentinel", (610, -37.5))]
)
LABELS = {
    "barricade": "Barricade",
    "platform": "SkySteps",
    "hazard": "RuneTrap",
    "lift": "EchoLift",
    "sentinel": "Sentinel",
}


def main() -> None:
    original = SCENE.read_text(encoding="utf-8")
    if PREFIX in original:
        print("ThreeScene expansion already exists; unchanged.")
        return

    if original.count(ROOT_MARK) != 1 or original.count(SCENE_ROOTS) != 1:
        raise RuntimeError("Base authored level or scene roots not present.")
    start, end = original.index(ROOT_MARK), original.index(SCENE_ROOTS)
    if start >= end:
        raise RuntimeError("Unexpected ThreeScene YAML object ordering.")

    generated = original[start:end]
    boundaries = list(re.finditer(r"^--- !u!1 &(\d+)\n", generated, re.M))
    groups = {}
    for i, found in enumerate(boundaries):
        following = (boundaries[i + 1].start()
                     if i + 1 < len(boundaries) else len(generated))
        groups[found.group(1)] = generated[found.start():following]

    if any(id_ not in groups for id_ in TEMPLATES.values()):
        raise RuntimeError("Original authored obstacle templates are missing.")

    preexisting_ids = set(re.findall(
        r"^--- !u!\d+ &(-?\d+)", original, re.M))
    next_id = 8104100000000000000
    added_transforms = []
    added_blocks = []
    counts = Counter()

    def allocate() -> str:
        nonlocal next_id
        result = str(next_id)
        next_id += 1
        if result in preexisting_ids:
            raise RuntimeError("Unity fileID collision: " + result)
        preexisting_ids.add(result)
        return result

    for kind, (x, y) in SPECS:
        counts[kind] += 1
        name = f"{PREFIX}{LABELS[kind]}_{counts[kind]:02d}"
        block = groups[TEMPLATES[kind]]
        old_ids = re.findall(r"^--- !u!\d+ &(\d+)", block, re.M)
        if not old_ids or len(old_ids) < 4:
            raise RuntimeError("Unexpected prefab component template for " + kind)
        new_ids = [allocate() for _ in old_ids]

        for old, new in zip(old_ids, new_ids):
            block = re.sub(r"(?<!\d)" + re.escape(old) + r"(?!\d)",
                           lambda _match, replacement=new: replacement, block)

        block, replaced = re.subn(r"^  m_Name: .*",
                                  "  m_Name: " + name, block,
                                  count=1, flags=re.M)
        if replaced != 1:
            raise RuntimeError("Missing GameObject name: " + name)
        block, replaced = re.subn(
            r"^  m_LocalPosition: \{[^}]+\}$",
            "  m_LocalPosition: {x: %s, y: %s, z: 0}" % (x, y),
            block, count=1, flags=re.M)
        if replaced != 1:
            raise RuntimeError("Missing transform position for " + name)

        if kind == "lift":
            move = ("  travel: {x: 0, y: 4}" if counts[kind] == 1
                    else "  travel: {x: 3, y: 0}")
            block = re.sub(r"^  travel: \{[^}]+\}$", move,
                           block, count=1, flags=re.M)
            block = re.sub(r"^  period: .*",
                           "  period: " + ("4.8" if counts[kind] == 1 else "3.8"),
                           block, count=1, flags=re.M)
        if kind == "sentinel":
            block = re.sub(r"^  attackInterval: .*",
                           "  attackInterval: 3.2", block, count=1, flags=re.M)
            block = re.sub(r"^  sightRange: .*",
                           "  sightRange: 13", block, count=1, flags=re.M)

        added_blocks.append(block)
        added_transforms.append(new_ids[1])

    root_start = generated.index("--- !u!4 &8104000000000000001\nTransform:\n")
    next_block = generated.index("--- !u!", root_start + 10)
    root = generated[root_start:next_block]
    parent_marker = "  m_Father: {fileID: 0}\n"
    if root.count(parent_marker) != 1:
        raise RuntimeError("Authored root parent is invalid.")
    root = root.replace(parent_marker, "".join(
        "  - {fileID: " + transform + "}\n"
        for transform in added_transforms) + parent_marker)

    generated = generated[:root_start] + root + generated[next_block:]
    expanded = original[:start] + generated + "".join(added_blocks) + original[end:]

    all_ids = re.findall(r"^--- !u!\d+ &(-?\d+)", expanded, re.M)
    if len(all_ids) != len(set(all_ids)):
        raise RuntimeError("Duplicated Unity YAML fileIDs.")
    if len(SPECS) != 22 or len(added_transforms) != 22:
        raise RuntimeError("Unexpected expansion count.")
    for key, expected in (
        ("Astral_Expansion_Barricade_", 4),
        ("Astral_Expansion_SkySteps_", 10),
        ("Astral_Expansion_RuneTrap_", 4),
        ("Astral_Expansion_EchoLift_", 2),
        ("Astral_Expansion_Sentinel_", 2),
    ):
        if len(re.findall(r"^  m_Name: " + key, expanded, re.M)) != expected:
            raise RuntimeError("Failed to serialize " + key)

    if not expanded.endswith(original[end:]):
        raise RuntimeError("SceneRoots was modified unexpectedly.")

    SCENE.write_text(expanded, encoding="utf-8", newline="")
    print("ThreeScene: baked 22 additional visible gameplay objects "
          "(4 barricades, 10 elevated platforms, 4 hazards, "
          "2 moving lifts, 2 sentinels).")


if __name__ == "__main__":
    main()
