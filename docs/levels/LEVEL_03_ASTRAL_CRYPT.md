# Chapter 3 — The Astral Crypt (ThreeScene)

## Design intent

Third chapter = *earned mastery*, not another linear corridor. Keep existing Unity terrain, enemies,
tilemaps, camera and art assets; bake a dedicated, deterministic gameplay overlay on top.
The palette uses imported Graveyard parallax, crypt stone and cyan spectral highlights.

### Route & pacing

| Zone | World X | Main interaction | Optional reward |
| --- | ---: | --- | --- |
| Crypt threshold | 18–118 | Readability / shrine / Wall Jump introduction | Safe recharge |
| First vertical ascent | 120–145 | Alternating one-way ledges inside the 40-unit wall shaft | Memory 1 at summit |
| Spirit balcony | 145–280 | Descending upper route, secret discovery, Spirit Well | Secret balcony |
| Echo seal | 280–400 | Spirit-required seal, temporary Spirit Gate, alternate memory climb | Memory 2 |
| Dash chain | 435–520 | Dash seal + breakable obstacle | Midway shrine |
| Final ascent | 554–604 | Spirit Well, Wall Jump mastery, Memory 3 | Elevated high path |
| Exit descent | 604–718 | One-way landings down to objective gate and completion zone | Rank challenge |

Requirements: collect **2 of 3 memories**, no required secret, no boss requirement. Two
Spirit Wells, two checkpoints, two wall-jump shafts. Levels 1–2 remain unaffected.

## What changes in the repository

- `EllenProductionLevelDesigner` now generates 44 authored one-way crypt ledges
  (24 inside the shafts, 8 on the secret balcony route, 5 by Memory 2,
  7 as a safer final descent). Each uses a repository crypt sprite, a
  `BoxCollider2D`, and `PlatformEffector2D`.
- `EllenLevelThreeBuilder.BuildLevelThree()` upgrades, rebuilds, validates and **saves
  only** `Assets/Scenes/ThreeScene.unity`.
- Legacy scene geometry, original prefabs, OneScene, and TwoScene are not
  overwritten by the Level 3-only workflow.

## Bake from Unity

With Unity Editor closed, update the project code and run the Unity Editor version
in `ProjectSettings/ProjectVersion.txt` (currently 6000.5.2f1):

```powershell
& "D:\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe" -batchmode -quit `
  -projectPath "D:\Unity_Projects\Story of Ellen" `
  -executeMethod EllenLevelThreeBuilder.BuildLevelThree `
  -logFile "D:\Unity_Projects\Ellen-Level3-Build.log"
```

Or use **Ellen → Production → Build Level 3 - Astral Crypt**.
Validate with **Ellen → Production → Validate Level 3 - Astral Crypt**.

The committed `ThreeScene.unity` file remains the legacy layout until Unity runs
the builder and saves the generated layer. Commit that scene file after visual
and Play Mode inspection.

## Playtest gate

1. Check entry spawn and HUD. Walk to first shrine.
2. Climb each shaft via touch, keyboard or gamepad; verify platform
   effector one-way collision and recovery from a missed jump.
3. Collect first memory at shaft summit, then find upper balcony secret.
4. Reach Spirit Well, shift into Spirit World, open the Spirit seal and
   pass the gate before energy depletes.
5. Dash through the dash seal and breakable barrier.
6. Reach final shrine and well; climb second shaft, collect optional third
   memory and descend via the authored landings.
7. Confirm the final gate stays closed with fewer than two memories,
   opens after two, and the result screen persists campaign progress.
8. Inspect camera boundaries, enemy hitboxes, 60fps performance, touch
   controls and missing-prefab warnings.

## Known external issue

`Environment_Middle_Crypt.prefab` references a missing `MagicFire` source prefab
GUID `002960d04fe97db4a8821f7ff377e362` in three places.
This is not repaired by the Level 3 builder; restore the source .prefab/.meta from
the original asset package or replace the three instances using the Unity Editor
after asset review. Do not fabricate a matching GUID.
