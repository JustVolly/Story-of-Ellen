# Story of Ellen — Production Campaign Content

This feature is based on `gameplay-foundation` (not the un-upgraded `main` branch).

## What is implemented

* `EllenProductionSceneInstaller` prepares the menu and levels 1–3 with production movement, abilities, HUD, save/level-flow and audio systems.
* `EllenProductionLevelDesigner` authors objectives, level beats, shrines, memory fragments, Spirit gates, dash barriers, the level 2 Guardian encounter and the level 3 wall-jump ascent.
* `EllenLevelArtBuilder` integrates existing repository assets into the three gameplay scenes. It assembles theme-matched multi-layer backgrounds (mountains/desert/graveyard), foliage, rocks, ruins, spirit altars, readable barriers, ascent wall silhouettes and ambient audio.
* `EllenCampaignContentBuilder` orchestrates all scene operations, checks critical asset imports and validates scene structure before saving generated content. New `EllenAmbienceVolume` applies saved SFX volume to each loop.

The scene builder deliberately **does not erase or overwrite legacy terrain, camera, player, enemies or handcrafted geometry**. It only adds/replaces the dedicated `[Production]`, `[LevelDesign]` and `[ProductionArt]` layers.

## Prerequisites

1. Check out the feature branch and install Git LFS: `git lfs install && git lfs pull`. Image/audio files are committed as Git LFS pointers, so the real assets must be downloaded before building.
2. Open the Unity project in **Unity 6000.5.2f1** and allow package import and compilation to complete.
3. Before modifying playable scenes, commit or back up your local scene edits. The full campaign builder saves the scene files.

## Build the full campaign (Unity Editor)

Select **Ellen > Production > Build Complete Campaign**. This action:

1. Fails early if core environment sprites/prefabs or any playable scene are missing.
2. Upgrades StartingScene and OneScene/TwoScene/ThreeScene.
3. Rebuilds the generated gameplay layouts (idempotently), including updated wall-jump shaft geometry.
4. Replaces the generated `[ProductionArt]` on each level and refreshes dependent landmark art.
5. Validates progression, abilities, MemoryFragment, shrines, level completion and the Level 2 boss/Level 3 wall-jump content.
6. Saves the scenes, ensures all four scenes are enabled in Build Settings and restores the previously open scene.

Then select **Ellen > Production > Validate Complete Campaign**. Use **Rebuild Art In Current Level** if only a visual refresh is needed.

### Headless Unity scene bake

From the project directory (with a properly licensed Unity Editor installed):

```sh
Unity -batchmode -quit -projectPath "$(pwd)" \
  -executeMethod EllenCampaignContentBuilder.BuildCompleteCampaign \
  -logFile campaign-build.log
```

Inspect the exit status and the editor log; **do not** treat the headless script itself as a Play Mode test. Review and commit the changed `.unity` scene files after running it.

## Echo puzzles and Spirit Wells (third development pass)

The level designer now creates persistent-in-run **Ability Seals** and reusable
**Spirit Wells** using the original crypt/altar sprites and particle effects.

- **Spirit Well:** standing within its aura while energy is below maximum restores
  3 energy units. It then enters an 8-second recharge period. Standing at full
  energy does not waste an activation. Cooldown is indicated by a dimmed altar.
- **Spirit Seal:** toggle Spirit World inside the nearby sensor to permanently
  deactivate that blocker for the current attempt.
- **Dash Seal:** dash through the nearby sensor to permanently break its blocker.
  A non-dashing approach does not unlock the seal.
- **Combat compatibility:** character bullets first look for `EnemyHealth` on
  the impacted collider's parent hierarchy. This makes damage work on production
  enemies and Guardian child colliders without removing the old enemy responses.

| Scene | New content | Positions in world X |
| --- | --- | --- |
| OneScene | Spirit Well tutorial, Spirit Seal | 70, 92 |
| TwoScene | Dash Seal before first authored dash barrier | 135 |
| ThreeScene | Spirit Well, Spirit Seal, Dash Seal, final recharge Well | 280, 300, 435, 554 |

The Unity editor **Build Complete Campaign** command rebuilds authored gameplay
then updates art and saves the scenes. **Validate Complete Campaign** checks exact
well/seal counts *and* their serialized collider, visual, Spirit World and
particle-system references. This is static validation only, not proof that all
routes are traversable at gameplay speed.

## Level-by-level direction

| Scene | Identity | Authored mechanics | Visual assets |
| --- | --- | --- | --- |
| OneScene | Spirit Discovery | Spirit gate tutorial, memory hunt, shrines and objective exit | Mountain background, forests, green-cyan altar details |
| TwoScene | Dash Mastery | Dash barriers, combat transition, Guardian boss, arena/HUD | Desert layers, amber ruins, arena landmarks |
| ThreeScene | Spirit Ascent | Two taller wall-jump shafts, Spirit/Dash chain, memory fragments, objective exit | Graveyard layers, crypt pillars, blue spectral monuments |

Scenery is decoration only: prefab colliders are disabled. Gameplay collisions remain in the authored terrain and `[LevelDesign]` layer.

## Input and gameplay polish (second development pass)

The campaign bake installs `EllenGameplayInput` and `EllenFallRecovery` on the
existing player. Neither script replaces the mobile button handlers.

| Action | Keyboard/mouse | Gamepad |
| --- | --- | --- |
| Move | A/D or Left/Right | Left stick or D-pad |
| Jump / double jump | Space, W or Up | South (A/Cross) |
| Dash | Left/Right Shift | East (B/Circle) |
| Wall-jump | E | North (Y/Triangle) |
| Spirit World | Q | West (X/Square) |
| Shoot | F, J or left mouse | Right shoulder |
| Pause / resume | Escape | Start/Menu |

Gameplay recovery and progression changes:
- Dying and respawning clears jump buffers, wall-jump locks and dash cooldown/state.
- Running out of Spirit energy inside a gate delays collision restoration until Ellen exits the former barrier area.
- The Guardian advances between attacks and flashes red before a phase-dependent charge; its warning commits to one direction so dodging is meaningful.
- Falling below the world at y = -85 triggers normal death and checkpoint recovery.
- A depleted legacy level timer refills on respawn instead of instantly killing Ellen repeatedly.
- Starting a New Journey clears prior results, rankings and unlocked abilities while preserving audio settings.
- Completing Level 3 marks the full campaign finished in the save data.

## Manual Play Mode acceptance test

- [ ] Clean import: Console contains no compiler errors and all Git LFS assets render.
- [ ] StartingScene: Play, Continue/New Journey, Settings, controls and Quit UI work. Start a New Journey after a completed campaign and confirm old ranks and collectibles are cleared while audio settings are retained.
- [ ] Desktop controls: test keyboard movement/jump, dash, wall-jump, Spirit mode, shooting and Escape pause/resume; test all equivalent gamepad buttons. On mobile, verify touching HUD buttons does not fire a projectile.
- [ ] Death recovery: die during dash or wall-jump, then confirm speed/gravity and jump counters reset; expire the scene timer and verify it restarts after respawn; fall below y = -85 and confirm checkpoint recovery.
- [ ] Spirit collision safety: let energy expire while Ellen is *inside* a Spirit gate, then verify the gate solidifies after she clears it rather than trapping or launching her.
- [ ] Guardian telegraph: wait for red anticipation flash, dodge its committed attack direction, verify phase two/three speed changes and that the boss cannot immediately attack without recovery time.
- [ ] Level 1: player spawns on ground, camera follows, double jump works, Spirit toggle opens both Spirit gates, at least one memory is accessible, shrine heals and sets checkpoint, objective exit completes level.
- [ ] Level 1 Spirit puzzle: walk into Spirit Seal normally (it stays closed), toggle Spirit World while inside the sensor, verify it opens permanently; expend Spirit energy then recharge at the Well. A full bar must not spend the Well's cooldown.
- [ ] Level 2 Dash Seal: walking into the barrier does not open it, dashing through its sensor does, and its visual disappears after activation.
- [ ] Level 3 chain: both Spirit and Dash Seals operate in order; recharge Well before Spirit gate and second Well before final ascent work even after repeated respawns. Verify no newly generated blockers conflict with terrain or existing routes.
- [ ] Combat: shoot the Guardian's sprite and child colliders; each real projectile hit reduces its production health, changes phases, and ultimately triggers victory. Verify legacy snail/non-production enemies still respond normally.
- [ ] Level 2: Dash unlock available, dash gates break only while dashing, memory route is navigable, boss spawns when entering arena, health/UI/phases function, defeating it opens the exit, and reaching the exit (not simply killing the boss) progresses to Level 3.
- [ ] Level 3: Wall-jump ability available, both wall-jump shafts are reachable from ground, elevated memory pickups are reachable, Spirit gate and dash barrier both work, 2 memories unlock the objective exit, final result triggers.
- [ ] Check that no art sprite occludes player, interactive sprites remain visible, no collisions were accidentally created by props, no camera clipping at x=-60..890. Check ambience volume follows the SFX setting; shrine respawns must reset dash/fall momentum.
- [ ] Test 16:9, 16:10 and narrow mobile resolutions; inspect console and FPS/memory on target hardware.
- [ ] Re-run **Build Complete Campaign** and verify no duplicates of bosses, shrines, HUD, backgrounds or ProductionVisual children.
- [ ] Run **Validate Complete Campaign** after reopening the saved scenes, then build and play the complete standalone/mobile application.

## Current limitation

The GitHub operation commits **editor generation code**, not rewritten scene YAML: actual scene serialization must be performed by Unity with the real LFS assets available. Scene rendering, compilation and Play Mode functionality are not verified until that Unity pass runs. Third-party art and audio licensing also requires a distribution audit before commercial release.
