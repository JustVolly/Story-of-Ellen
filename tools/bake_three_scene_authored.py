#!/usr/bin/env python3
"""Bake REAL level-three obstacles into the serialized Unity scene.

Safe to repeat. Uses only Python's standard library and only writes ThreeScene.
The scene's legacy objects, prefab links, tilemaps and GUID stay unchanged.
"""
from __future__ import annotations
from pathlib import Path
import re

PROJECT = Path(__file__).resolve().parents[1]
SCENE = PROJECT / "Assets/Scenes/ThreeScene.unity"
ROOT = "[AstralCryptAuthored]"
ASSETS = {
    "wall": "ec7b15b4e844242449aa6af9e8ca036b",
    "ruin": "568472cb1cba85345b4f9ebb4c86d55d",
    "altar": "23f9958b9c709bb48923bea13d4a70d6",
}
SCRIPTS = {
    "hazard": "bb04ee4231184c86824aefb3a45e5f01",
    "moving": "fd2cc3a3ba5c42449cb5b01d4326ab8d",
    "sentinel": "e00758338f164a71b8b11fc3ce73bbf2",
    "enemy_health": "9a4f80fe15499c1ad95dc9f479e73a0e",
}
SCENE_ROOT_MARKER = "--- !u!1660057539 &9223372036854775807\nSceneRoots:\n"


def main() -> None:
    source = SCENE.read_text(encoding="utf-8")
    if f"m_Name: '{ROOT}'" in source:
        print(f"{SCENE}: authored Level 3 objects already present; no changes")
        return
    if len(source) < 2_000_000 or source.count(SCENE_ROOT_MARKER) != 1:
        raise RuntimeError("Unexpected ThreeScene format. Refusing to modify it.")

    old_ids = set(re.findall(r"^--- !u!\d+ &(-?\d+)", source, re.M))
    ids = set(old_ids)
    counter = 8104000000000000000

    def new_id() -> str:
        nonlocal counter
        value = str(counter)
        counter += 1
        if value in ids:
            raise RuntimeError("Duplicate Unity fileID: " + value)
        ids.add(value)
        return value

    def template(tag: int) -> str:
        match = re.search(rf"^--- !u!{tag} &-?\d+\n", source, re.M)
        if match is None:
            raise RuntimeError(f"Missing Unity YAML template {tag}")
        end = source.find("--- !u!", match.end())
        return source[match.start():end if end != -1 else None]

    samples = {
        "renderer": template(212),
        "collider": template(61),
        "effector": template(251),
        "rigidbody": template(50),
    }

    def fmt(value: float | int) -> str:
        value = float(value)
        return f"{value:.3f}".rstrip("0").rstrip(".") if value else "0"

    def vec3(x: float, y: float, z: float) -> str:
        return f"{{x: {fmt(x)}, y: {fmt(y)}, z: {fmt(z)}}}"

    root_go, root_transform = new_id(), new_id()
    blocks: list[str] = []
    root_children: list[str] = []
    names: list[str] = []

    def replace_once(text: str, pattern: str, new: str) -> str:
        changed, count = re.subn(pattern, lambda match: new, text, count=1, flags=re.M)
        if count != 1:
            raise RuntimeError("Unity template substitution failed: " + pattern)
        return changed

    def add(name: str, x: float, y: float, size: tuple[float, float],
            sprite: str, color: tuple[float, float, float, float],
            *, scale: tuple[float, float] = (1, 1), sort: int = 25,
            collider: str | None = None, one_way: bool = False,
            moving: bool = False,
            scripts: tuple[tuple[str, dict[str, str]], ...] = ()) -> None:
        go, trans, rend = new_id(), new_id(), new_id()
        components = [trans, rend]
        box_id = new_id() if collider else None
        if box_id:
            components.append(box_id)
        eff_id = new_id() if one_way else None
        if eff_id:
            components.append(eff_id)
        body_id = new_id() if moving else None
        if body_id:
            components.append(body_id)
        script_entries = [(new_id(), key, props) for key, props in scripts]
        components += [entry[0] for entry in script_entries]

        component_lines = "\n".join(
            f"  - component: {{fileID: {part}}}" for part in components)
        blocks.append(f"""--- !u!1 &{go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{component_lines}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
""")
        blocks.append(f"""--- !u!4 &{trans}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {vec3(x, y, 0)}
  m_LocalScale: {vec3(scale[0], scale[1], 1)}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {root_transform}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
""")
        root_children.append(trans)

        part = samples["renderer"]
        part = replace_once(part, r"^--- !u!212 &\d+", f"--- !u!212 &{rend}")
        part = replace_once(part, r"m_GameObject: \{fileID: \d+\}",
                            f"m_GameObject: {{fileID: {go}}}")
        part = replace_once(part, r"m_Sprite: \{fileID: [^}]+\}",
                            f"m_Sprite: {{fileID: 21300000, guid: {ASSETS[sprite]}, type: 3}}")
        part = replace_once(part, r"m_SortingOrder: -?\d+", f"m_SortingOrder: {sort}")
        part = replace_once(part, r"m_Color: \{[^}]+\}",
                            "m_Color: {r: %s, g: %s, b: %s, a: %s}" %
                            tuple(fmt(v) for v in color))
        part = replace_once(part, r"m_Size: \{[^}]+\}",
                            f"m_Size: {{x: {fmt(size[0])}, y: {fmt(size[1])}}}")
        blocks.append(part)

        if box_id is not None:
            part = samples["collider"]
            part = replace_once(part, r"^--- !u!61 &\d+", f"--- !u!61 &{box_id}")
            part = replace_once(part, r"m_GameObject: \{fileID: \d+\}",
                                f"m_GameObject: {{fileID: {go}}}")
            part = replace_once(part, r"m_IsTrigger: \d+",
                                f"m_IsTrigger: {1 if collider == 'trigger' else 0}")
            part = replace_once(part, r"m_UsedByEffector: \d+",
                                f"m_UsedByEffector: {1 if one_way else 0}")
            part = replace_once(part, r"m_Offset: \{[^}]+\}",
                                "m_Offset: {x: 0, y: 0}")
            part = replace_once(part, r"m_Size: \{[^}]+\}",
                                f"m_Size: {{x: {fmt(size[0] / scale[0])}, "
                                f"y: {fmt(size[1] / scale[1])}}}")
            blocks.append(part)

        if eff_id is not None:
            part = samples["effector"]
            part = replace_once(part, r"^--- !u!251 &\d+", f"--- !u!251 &{eff_id}")
            part = replace_once(part, r"m_GameObject: \{fileID: \d+\}",
                                f"m_GameObject: {{fileID: {go}}}")
            part = replace_once(part, r"m_UseOneWay: \d+", "m_UseOneWay: 1")
            part = replace_once(part, r"m_SurfaceArc: \d+", "m_SurfaceArc: 160")
            blocks.append(part)

        if body_id is not None:
            part = samples["rigidbody"]
            part = replace_once(part, r"^--- !u!50 &\d+", f"--- !u!50 &{body_id}")
            part = replace_once(part, r"m_GameObject: \{fileID: \d+\}",
                                f"m_GameObject: {{fileID: {go}}}")
            part = replace_once(part, r"m_BodyType: \d+", "m_BodyType: 1")
            part = replace_once(part, r"m_Constraints: \d+", "m_Constraints: 4")
            part = replace_once(part, r"m_Mass: [^\n]+", "m_Mass: 1")
            part = replace_once(part, r"m_UseFullKinematicContacts: \d+",
                                "m_UseFullKinematicContacts: 1")
            blocks.append(part)

        for component_id, key, props in script_entries:
            attrs = "\n".join(f"  {field}: {value}" for field, value in props.items())
            blocks.append(f"""--- !u!114 &{component_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPTS[key]}, type: 3}}
  m_Name:
  m_EditorClassIdentifier:
{attrs}
""")
        names.append(name)

    for i, (x, y, w, h) in enumerate([
        (44, -39.2, 2.3, 4.2), (169, -39.5, 2.1, 3.6),
        (372, -39.1, 2.5, 4.7), (615, -39.7, 2.2, 3.5)
    ], 1):
        add(f"Astral_Obstacle_{i:02d}_Ruin", x, y, (w, h),
            "ruin" if i % 2 == 0 else "wall", (.51, .73, .95, 1),
            scale=(1, 1.5), sort=28, collider="solid")

    stepping = [
        (76, -35), (84, -31), (92, -27), (100, -23), (108, -19),
        (326, -35), (334, -31), (342, -27), (350, -23), (358, -19),
        (565, -35), (573, -31), (581, -27), (589, -23), (597, -19)
    ]
    for i, (x, y) in enumerate(stepping, 1):
        add(f"Astral_Platform_{i:02d}", x, y, (5.4, .8), "ruin",
            (.62, .82, 1, 1), scale=(1.25, .5),
            sort=26, collider="solid", one_way=True)

    for i, x in enumerate((209, 405, 664), 1):
        add(f"Astral_Hazard_{i:02d}_BurningRune", x, -41.5,
            (4.8, 1.1), "altar", (1, .24, .4, .95),
            scale=(1.4, .65), sort=36, collider="trigger",
            scripts=(("hazard", {"damage": "1"}),))

    for i, (x, y, dx, dy, period, phase) in enumerate([
        (254, -31, 0, 3, 4.2, 0),
        (491, -33, 2.5, 0, 3.6, 1.5),
    ], 1):
        add(f"Astral_MovingPlatform_{i:02d}", x, y, (4.8, .9),
            "wall", (.43, 1, .86, 1), scale=(1, .65),
            sort=30, collider="solid", moving=True,
            scripts=(("moving", {
                "travel": f"{{x: {fmt(dx)}, y: {fmt(dy)}}}",
                "period": fmt(period), "phase": fmt(phase)
            }),))

    for i, (x, y) in enumerate(((295, -37.5), (528, -37.5)), 1):
        add(f"Astral_Sentinel_{i:02d}", x, y, (2.2, 3.4),
            "altar", (.59, .6, 1, 1), scale=(.9, 1.2),
            sort=40, collider="trigger",
            scripts=(
                ("sentinel", {"sightRange": "16", "attackInterval": "2.5",
                              "boltSpeed": "6", "boltLifetime": "3"}),
                ("enemy_health", {"maxHealth": "3",
                                  "hitEffect": "{fileID: 0}",
                                  "deathEffect": "{fileID: 0}"}),
            ))

    children = "\n".join(f"  - {{fileID: {value}}}" for value in root_children)
    root_yaml = f"""--- !u!1 &{root_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {root_transform}}}
  m_Layer: 0
  m_Name: '{ROOT}'
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{root_transform}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{children}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""
    offset = source.index(SCENE_ROOT_MARKER)
    edited = source[:offset] + root_yaml + "".join(blocks) + source[offset:]
    edited = edited.rstrip() + f"\n  - {{fileID: {root_transform}}}\n"
    new_yaml_ids = re.findall(r"^--- !u!\d+ &(-?\d+)", edited, re.M)
    if len(new_yaml_ids) != len(set(new_yaml_ids)):
        raise RuntimeError("YAML fileID collision; no changes written.")
    if len(names) != 26 or len(set(names)) != len(names):
        raise RuntimeError("Unexpected object count.")
    for guid in SCRIPTS.values():
        if guid not in edited:
            raise RuntimeError("Missing gameplay script reference: " + guid)
    if not edited.endswith(f"  - {{fileID: {root_transform}}}\n"):
        raise RuntimeError("SceneRoots was not updated.")
    SCENE.write_text(edited, encoding="utf-8", newline="")
    print(f"ThreeScene: added {len(names)} visible and physical gameplay objects "
          f"under {ROOT} (4 obstacles, 15 platforms, 3 hazards, "
          "2 moving platforms, 2 sentinels).")
    print(f"Unity YAML component records: {len(new_yaml_ids)}; "
          f"previous records: {len(old_ids)}")


if __name__ == "__main__":
    main()
