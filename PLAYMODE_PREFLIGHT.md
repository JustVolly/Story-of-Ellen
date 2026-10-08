# Story of Ellen - Play Mode Preflight

## Production scene migration
- Open the gameplay-foundation branch in Unity and let the import finish.
- Run Ellen > Production > Upgrade All Scenes.
- Review the generated StartingScene, OneScene and TwoScene changes before committing scene files.
- Run Ellen > Production > Validate Migrated Scenes.
- Run Ellen > Validate Vertical Slice.
- Start Play Mode from StartingScene and complete both levels end-to-end.

Run this after Unity finishes importing the branch and before tuning gameplay.

## Import and compile
- Confirm Console has zero compile errors.
- Confirm the committed .meta files under Assets/Scripts/Production and Assets/Editor import without GUID conflicts or missing-meta errors.
- Open OneScene and run Ellen > Validate Vertical Slice.

## Player lifecycle
- Spawn at expected start point.
- Take one hit: health decreases once, flash/shake plays, invulnerability prevents rapid repeat damage.
- Die to enemy, trap, thorns, and timeout: all paths use PlayerHealth and respawn once.
- Respawn restores gravity, colliders, animation, controls, and full health.
- Releasing left/right while airborne preserves vertical velocity.
- Double jump count resets on landing.

## Spirit World
- Material-only objects are correct on scene start.
- Toggle Spirit World: energy drains, HUD updates, presentation transition plays.
- Energy reaching zero returns to Material World.
- SpiritGate collider/visual state matches the active world.
- First Spirit entry shows tutorial once.

## Progression and encounters
- Shrine becomes active checkpoint and heals.
- Memory and secret counters update objective HUD.
- Combat arena locks once, tracks all configured enemies, then unlocks.
- Ability pickup unlocks once, displays feedback, persists after restart.
- Traversal uses the unlocked ability without PlayerMovement overriding it.

## Boss and results
- Boss HUD appears at arena start and tracks HP.
- Phase 2 and 3 trigger at expected thresholds.
- Boss defeat unlocks arena and completes boss objective.
- Result panel shows time, deaths, memories, secrets, and rank.
- Better time/death/rank records persist without worse runs overwriting them.

## Pause and presentation
- Pause keeps Time.timeScale at zero.
- Hit-stop never unpauses the game.
- Camera shake does not fight the camera follow transform.
- No repeating coroutines, NullReferenceException, or per-frame error logs.

## Mobile
- Touch movement, jump, attack, Spirit toggle, Dash, and Wall Jump are reachable.
- Test target device for stable frame pacing and zero recurring GC spikes during normal gameplay.
