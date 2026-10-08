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
- Run `Ellen/Production/Upgrade Build Scenes` once after import. This upgrades StartingScene, OneScene and TwoScene, saves them, and wires the production HUD/menu/player systems.
- Open each build scene and run `Ellen/Production/Validate Current Scene`; fix every reported error before tuning.
- Confirm Console has zero compile errors.
- Confirm the committed .meta files under Assets/Scripts/Production and Assets/Editor import without GUID conflicts or missing-meta errors.
- Open OneScene and run Ellen > Validate Vertical Slice.

## Player lifecycle
- Spawn at expected start point.
- Take one hit: health decreases once, flash/shake plays, invulnerability prevents rapid repeat damage.
- Die to enemy, trap, thorns, and timeout: all paths use PlayerHealth and respawn once.
- Respawn restores gravity, colliders, animation, controls, and full health.
- Releasing left/right while airborne preserves vertical velocity.
- Run acceleration/deceleration and turn acceleration feel responsive without instant velocity snapping.
- Short jump release produces a clean jump cut; held jump gets apex hang and a faster fall afterward.
- A jump pressed just before landing buffers into the landing instead of wasting the air jump.
- Double jump count resets on landing.

## Spirit World
- Material-only objects are correct on scene start.
- Tap the Spirit HUD: Spirit World toggles, energy drains, HUD updates, and background presentation transitions.
- Energy reaching zero returns to Material World.
- SpiritGate collider/visual state matches the active world.
- First Spirit entry shows tutorial once.

## Progression and encounters
- Shrine becomes active checkpoint and heals.
- Memory and secret counters update objective HUD.
- Combat arena locks once, tracks all configured enemies, then unlocks.
- Entering TwoScene unlocks Dash once, displays the ability toast, persists after restart, and exposes the mobile DASH control.
- Ability pickup unlocks once, displays feedback, persists after restart.
- Traversal uses the unlocked ability without PlayerMovement overriding it.

## Boss and results
- TwoScene final win action records campaign completion and returns to StartingScene.
- Main menu then shows REPLAY LEVEL 2; NEW JOURNEY resets level/ability progression while preserving records/settings.
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

## Level 1 — Spirit Discovery
- Confirm the first Spirit gate near the early route cannot be crossed in Material World and opens immediately in Spirit World.
- Collect any one of the three Memory Fragments; the final Memory objective barrier must then unlock.
- Verify the high-path Memory is reachable without requiring unintended collision exploits.
- Activate both Spirit Shrines and confirm each becomes the active respawn point.
- Check the upper-ruins secret is optional and does not block completion.
- Confirm the second Spirit gate creates a final mastery check without trapping the player at zero Spirit energy.
- Reach the result trigger before the legacy Finish and confirm rank/time/deaths/memories are recorded once.

## Level 2 — Dash Mastery
- Dash unlocks on entry and the DASH mobile button is visible and usable.
- Both orange Dash barriers block normal movement, cannot be jumped over accidentally, and break during Dash even if Dash starts while already inside the sensor.
- Activate both shrines and confirm the second checkpoint prevents replaying too much of the level after a boss death.
- Confirm all three Memory Fragments and the secret are optional exploration rewards.
- Enter the Guardian arena: entrance closes, exit stays locked, boss and boss HUD activate.
- Guardian takes projectile damage through EnemyHealth, changes phase at roughly 66% and 33% HP, and cannot leave the intended arena.
- On Guardian death, barriers open, boss HUD hides, result data records once, and the legacy Finish remains reachable.
- Completing the final Finish records campaign completion and returns to StartingScene.
