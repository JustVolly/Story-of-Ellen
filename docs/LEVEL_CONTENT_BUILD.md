# Story of Ellen — Production Campaign Content

This feature is based on `gameplay-foundation` (not the un-upgraded `main` branch).

## What is implemented

* `EllenProductionSceneInstaller` prepares the menu and levels 1–3 with production movement, abilities, HUD, save/level-flow and audio systems.
* `EllenProductionLevelDesigner` authors objectives, level beats, shrines, memory fragments, Spirit gates, dash barriers, the level 2 Guardian encounter and the level 3 wall-jump ascent.
* `EllenLevelArtBuilder` integrates existing repository assets into the three gameplay scenes. It assembles theme-matched multi-layer backgrounds (mountains/desert/graveyard), foliage, rocks, ruins, spirit altars, readable barriers, ascent wall silhouettes and ambient audio.
* `EllenCampaignContentBuilder` orchestrates all scene operations, checks critical asset imports and validates scene structure before saving generated content.

The scene builder deliberately **does not erase or overwrite legacy terrain, camera, player, enemies or handcrafted geometry**. It only adds/replaces the dedicated `[Production]`, `[LevelDesign]` and `[ProductionArt]` layers.

## Prerequisites

1. Check out the feature branch and install Git LFS: `git lfs install && git lfs pull`. Image/audio files are committed as Git LFS pointers, so the real assets must be downloaded before building.
2. Open the Unity project in **Unity 6000.5.2f1** and allow package import and compilation to complete.
3. Before modifying playable scenes, commit or back up your local scene edits. The full campaign builder saves the scene files.

## Build the full campaign (Unity Editor)

Select **Ellen > Production > Build Complete Campaign**. This action:

1. Fails early if core environment sprites/prefabs or any playable scene are missing.
2. Upgrades StartingScene and OneScene/TwoScene/ThreeScene.
3. Builds authored gameplay layouts (idempotently).
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

## Level-by-level direction

| Scene | Identity | Authored mechanics | Visual assets |
| --- | --- | --- | --- |
| OneScene | Spirit Discovery | Spirit gate tutorial, memory hunt, shrines and objective exit | Mountain background, forests, green-cyan altar details |
| TwoScene | Dash Mastery | Dash barriers, combat transition, Guardian boss, arena/HUD | Desert layers, amber ruins, arena landmarks |
| ThreeScene | Spirit Ascent | Two taller wall-jump shafts, Spirit/Dash chain, memory fragments, objective exit | Graveyard layers, crypt pillars, blue spectral monuments |

Scenery is decoration only: prefab colliders are disabled. Gameplay collisions remain in the authored terrain and `[LevelDesign]` layer.

## Manual Play Mode acceptance test

- [ ] Clean import: Console contains no compiler errors and all Git LFS assets render.
- [ ] StartingScene: Play, Continue/New Journey, Settings, controls and Quit UI work.
- [ ] Level 1: player spawns on ground, camera follows, double jump works, Spirit toggle opens both Spirit gates, at least one memory is accessible, shrine heals and sets checkpoint, objective exit completes level.
- [ ] Level 2: Dash unlock available, dash gates break only while dashing, memory route is navigable, boss spawns when entering arena, health/UI/phases function, its death unlocks exit, results progress to Level 3.
- [ ] Level 3: Wall-jump ability available, both wall-jump shafts are reachable from ground, elevated memory pickups are reachable, Spirit gate and dash barrier both work, 2 memories unlock the objective exit, final result triggers.
- [ ] Check that no art sprite occludes player, interactive sprites remain visible, no collisions were accidentally created by props, no camera clipping at x=-60..890.
- [ ] Test 16:9, 16:10 and narrow mobile resolutions; inspect console and FPS/memory on target hardware.
- [ ] Re-run **Build Complete Campaign** and verify no duplicates of bosses, shrines, HUD, backgrounds or ProductionVisual children.
- [ ] Run **Validate Complete Campaign** after reopening the saved scenes, then build and play the complete standalone/mobile application.

## Current limitation

The GitHub operation commits **editor generation code**, not rewritten scene YAML: actual scene serialization must be performed by Unity with the real LFS assets available. Scene rendering, compilation and Play Mode functionality are not verified until that Unity pass runs. Third-party art and audio licensing also requires a distribution audit before commercial release.
