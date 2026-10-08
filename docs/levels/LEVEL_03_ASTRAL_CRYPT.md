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

## Chapter presentation: guided discovery

Seven contextual, one-shot storytelling triggers are placed along the existing
corridor: Entry (-20), First Ascent (115), Secret Balcony (246), Spirit Well
(272), Dash Trial (425), Final Ascent (548), and Last Gate (681).

Each trigger shows a concise instruction in a non-interactive screen-space
banner. Fade animations use unscaled time so pause and hit-stop do not leave a
stuck banner. All generated HUD objects and triggers live below `[LevelDesign]`;
a rebuild removes and recreates them without touching the existing mobile HUD.

The optional balcony now uses 10-unit-wide landings at 14-unit
center-to-center spacing, leaving an approximately 4-unit clear gap rather than
a demanding 7-unit leap. The build validator also checks horizontal gaps for
the balcony, memory detour and final descent. These are structural guardrails,
not substitutes for verifying jump reachability in Play Mode.

The visual route uses seven cool-blue pulsing crypt beacons built from the
project's altar sprite, not fabricated placeholder assets. They live under
`[ProductionArt]`, have no colliders, and are replaced when the art layer is
rebuilt. The scripted pulsing uses one cached SpriteRenderer per beacon.

## What changes in the repository

- `EllenProductionLevelDesigner` now generates 44 authored one-way crypt ledges
  (24 inside the shafts, 8 on the secret balcony route, 5 by Memory 2,
  7 as a safer final descent). Each uses a repository crypt sprite, a
  `BoxCollider2D`, and `PlatformEffector2D`.
- `EllenLevelThreeBuilder.BuildLevelThree()` upgrades, rebuilds, validates and **saves
  only** `Assets/Scenes/ThreeScene.unity`.
- `EllenCryptGuidancePresenter`, `EllenCryptGuidanceCue` and
  `EllenCryptBeaconPulse` add seven guided story cues and seven animated,
  no-collision environment beacons. The Level 3 validator verifies wiring.
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
2. Walk through the seven story cue locations. Confirm that the banner
   fades after a few seconds, never blocks touch input, and shows each cue
   once per level run. Confirm each blue beacon pulses gently, with no
   new solid collider.
3. Climb each shaft via touch, keyboard or gamepad; verify platform
   effector one-way collision and recovery from a missed jump.
4. Collect first memory at shaft summit, then find upper balcony secret.
5. Reach Spirit Well, shift into Spirit World, open the Spirit seal and
   pass the gate before energy depletes.
6. Dash through the dash seal and breakable barrier.
7. Reach final shrine and well; climb second shaft, collect optional third
   memory and descend via the authored landings.
8. Confirm the final gate stays closed with fewer than two memories,
   opens after two, and the result screen persists campaign progress.
9. Inspect camera boundaries, enemy hitboxes, 60fps performance, touch
   controls and missing-prefab warnings.

## Known external issue

`Environment_Middle_Crypt.prefab` references a missing `MagicFire` source prefab
GUID `002960d04fe97db4a8821f7ff377e362` in three places.
This is not repaired by the Level 3 builder; restore the source .prefab/.meta from
the original asset package or replace the three instances using the Unity Editor
after asset review. Do not fabricate a matching GUID.
