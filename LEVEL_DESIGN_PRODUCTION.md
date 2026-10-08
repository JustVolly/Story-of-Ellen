# Story of Ellen — Production Level Design

This document defines the production gameplay identity of the two build levels while preserving the original authored geometry.

## Level 1 — Spirit Discovery

**Player promise:** learn that the world has two readable states and use Spirit World deliberately rather than as a cosmetic toggle.

**Runtime span:** approximately x=-41 to x=724.

**Pacing**
1. **Warm-up / movement read** — original opening geometry remains intact.
2. **Spirit discovery** — the first Spirit barrier forces a clear Material/Spirit interaction.
3. **Vertical exploration** — a high-route Memory rewards players who read the existing elevated path.
4. **First shrine / relief beat** — checkpoint and full heal after the first meaningful traversal section.
5. **Mid-level mastery** — legacy hazards remain, while Memory/secret placement creates optional route-reading.
6. **Second shrine** — reduces repetition before the long final third.
7. **Final Spirit mastery** — second Spirit barrier tests the mechanic again without introducing a new rule.
8. **Objective gate / finish** — at least one Memory is required to exit; all three Memories remain optional mastery targets for S rank.

**Production content**
- 2 Spirit gates
- 3 Memory Fragments
- 2 Spirit Shrines
- 1 optional secret area
- 1 Memory objective barrier
- 1 result trigger
- Spirit landmarks / particle language
- Rank tuning: S 115s, A 180s, 3/3 Memories required for S

This makes Level 1 teach, reinforce, rest, then test.

## Level 2 — Dash Mastery

**Player promise:** convert movement knowledge into speed and commitment, then finish with a real combat climax.

**Runtime span:** approximately x=-41 to x=789.

**Pacing**
1. **Ability reveal** — Dash unlocks on entry and the mobile DASH control appears.
2. **Safe first application** — the first Dash barrier proves that normal movement cannot solve every route.
3. **Traversal escalation** — original geometry remains, but Memory placement rewards controlled Dash use and route inspection.
4. **Mid-level shrine** — checkpoint before the level's denser middle.
5. **Second Dash test** — another breakable barrier reinforces timing without adding a new input.
6. **Final shrine / calm-before-climax** — recovery point before the Guardian.
7. **Guardian arena** — entrance closes, boss activates, exit remains locked.
8. **Three-phase finish** — Guardian speed and lunge pressure increase at ~66% and ~33% HP.
9. **Result / campaign finish** — boss defeat records the run; the original Finish remains reachable and returns the player to the menu after campaign completion.

**Production content**
- Dash unlock on entry
- Dedicated mobile DASH control
- 2 Dash-breakable barriers
- 3 Memory Fragments
- 2 Spirit Shrines
- 1 optional secret
- 1 production Guardian boss cloned from the existing Snail visual language
- Boss arena entrance/exit barriers
- Boss health + phase HUD
- Result recording/presentation
- Rank tuning: S 130s, A 205s, all 3 Memories improve mastery/rank value

## Design rules

- Existing level geometry is preserved unless Play Mode proves a collision or pacing defect.
- Mandatory objectives stay minimal; optional exploration feeds rank/replay value.
- Checkpoints are placed before expensive repetition, not immediately after every challenge.
- A mechanic is introduced safely before it becomes mandatory.
- Visual particles communicate Spirit, Dash, objective, shrine and boss states without requiring new external art.
- Any generated production layer lives under `[LevelDesign]` and can be rebuilt independently from the original scene.

## Unity authoring workflow

1. Run **Ellen > Production > Upgrade Build Scenes**.
2. Open OneScene and TwoScene separately.
3. Run **Ellen > Production > Validate Current Scene**.
4. Playtest using `PLAYMODE_PREFLIGHT.md`.
5. Use **Ellen > Production > Rebuild Current Level Design** only when intentionally regenerating the production layer.
