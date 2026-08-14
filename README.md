<div align="center">

# Story of Ellen

### A 2D action-platformer built around responsive traversal, combat, hazards, collectibles, and level progression.

`Unity 6000.5.2f1` · `C#` · `2D Physics` · `URP` · `Unity Input / UI Events`

</div>

---

## Overview

**Story of Ellen** is a Unity 2D action-platformer project that combines platforming, projectile combat, enemy encounters, traps, collectibles, power-ups, checkpoints, and scene-based progression.

The project currently ships three enabled build scenes:

- `StartingScene` — entry/menu flow
- `OneScene` — gameplay level
- `TwoScene` — gameplay level

Additional level names are already represented in the scene-management code, leaving room for continued expansion without changing the core progression model.

> **Portfolio note:** this repository remains private because the project contains third-party art/asset packs. A public showcase should expose only licensed media and selected author-owned source samples.

---

## What I Built

### Player Movement & Platforming

The player controller implements a full 2D traversal loop with:

- grounded and airborne horizontal movement
- double-jump handling
- left/right facing state
- animation-state updates
- Rigidbody2D-based movement
- checkpoint interaction
- respawn positioning
- trap and enemy interactions
- temporary movement boost behaviour
- UI-event-friendly input methods for mobile controls

### Projectile Combat

`CharacterAttack` manages a finite-ammo projectile loop with:

- facing-aware shot direction
- configurable projectile force
- Rigidbody2D impulse-based firing
- muzzle/shot particle feedback
- projectile lifetime management
- ammo limiting
- defensive validation for missing prefab, fire-point, or Rigidbody2D references

### Damage, Enemies & Hazards

The gameplay layer includes reusable systems for:

- player health and damage
- enemy movement and enemy damage
- projectile damage
- thorn and enemy traps
- defence states
- booster effects
- collectible ammunition and coins
- environmental plant/hazard behaviour

### Progression & Game Flow

The project contains scene-level systems for:

- checkpoint state
- respawn
- win / loss states
- pause and resume
- level completion
- next-level transitions
- level-progress percentage UI
- collectible tracking
- timed gameplay feedback

### Presentation & Runtime Systems

Beyond the core character loop, the project also contains:

- parallax scrolling
- custom `OcclusionCulling2D`
- particle feedback
- animation controllers
- prefab-based gameplay objects
- Tilemap-based environments
- TextMesh Pro UI

---

## Selected Source

| System | File | What it demonstrates |
| --- | --- | --- |
| Player controller | `Assets/Scripts/PlayerMovement.cs` | 2D movement, double jump, animation and gameplay-state integration |
| Projectile combat | `Assets/Scripts/CharacterAttack.cs` | Ammo, projectile spawning, directional firing and defensive validation |
| Game flow | `Assets/Scripts/ScenesManager.cs` | Checkpoints, respawn, level state, progress UI and scene transitions |
| Player health | `Assets/Scripts/PlayerHealth.cs` | Player lifecycle and damage state |
| Enemy movement | `Assets/Scripts/EnemyMovement.cs` | Reusable enemy traversal behaviour |
| Power-ups | `Assets/Scripts/PowerUps.cs` | Temporary gameplay modifiers and feedback |
| 2D culling | `Assets/Scripts/OcclusionCulling2D.cs` | Runtime visibility/performance support for a 2D scene |
| Parallax | `Assets/Scripts/ParallaxScrollar.cs` | Layered background motion |

---

## Engineering Notes

The project began as a gameplay-focused build and contains some tightly coupled legacy MonoBehaviours. The current portfolio pass deliberately improves the highest-value systems without breaking serialized Unity references, public UnityEvent methods, scene bindings, or prefab configuration.

For example, the projectile-combat path has been hardened so invalid prefab/reference configuration fails safely instead of continuing into a null-reference path. The public `AttackStart()` entry point and Inspector-facing fields remain compatible with existing scene wiring.

---

## Project Structure

```text
Story-of-Ellen/
├── Assets/
│   ├── Animation/
│   ├── Audio/
│   ├── Character/
│   ├── Enemies/
│   ├── Environment/
│   ├── PowerUps/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Sprites/
│   ├── Tilemaps/
│   └── Traps/
├── Packages/
├── ProjectSettings/
└── README.md
```

---

## Tech Stack

- **Engine:** Unity `6000.5.2f1`
- **Language:** C#
- **Rendering:** Universal Render Pipeline
- **Gameplay:** Unity 2D Physics / Rigidbody2D / Collider2D
- **UI:** Unity UI + TextMesh Pro
- **Animation:** Animator / 2D Animation
- **Input:** scene-configured Unity UI/event methods and Unity input tooling

---

## Portfolio Media

A concise capture plan for screenshots, GIFs, and a short gameplay reel is documented in [`docs/portfolio/CAPTURE_GUIDE.md`](docs/portfolio/CAPTURE_GUIDE.md). The goal is to demonstrate gameplay without redistributing third-party source assets.

---

<div align="center">

**VÜSAL ALİYEV**

*Game Developer · Software Engineer*

</div>
