# Story of Ellen — Portfolio Capture Guide

This guide defines the media set for a recruiter-facing showcase. Capture only gameplay that you are allowed to publish; do not redistribute third-party source textures, sprites, PSDs, or asset-pack files.

## Hero GIF — 8 to 12 seconds

Capture one continuous sequence that communicates the game immediately:

1. Run through a visually strong section of `OneScene`.
2. Jump or double-jump over a hazard.
3. Fire one projectile at an enemy.
4. Collect a coin, bullet pickup, or power-up.
5. End on forward movement rather than a menu or death screen.

Recommended export: 16:9, 1280×720 or 1920×1080 source, 30–60 FPS capture, then a lightweight web GIF/WebP/MP4 derivative for GitHub.

## Screenshots

Capture 4–5 images with different purposes rather than near-duplicates:

- **Hero gameplay:** Ellen in motion with readable environment depth.
- **Combat:** projectile in flight or enemy interaction.
- **Platforming:** jump/trap section that shows level design.
- **Progression:** checkpoint, collectible, power-up, or progress UI.
- **Second environment:** a visually distinct section from `TwoScene`.

Keep HUD elements that communicate gameplay, but avoid debug overlays, editor gizmos, Scene view, Console windows, or temporary test objects.

## Short Gameplay Reel — 25 to 40 seconds

Suggested edit:

- 0–4 s — title card: `Story of Ellen — 2D Action Platformer`
- 4–12 s — traversal and double jump
- 12–20 s — projectile combat
- 20–28 s — traps / enemy encounter / defence or booster
- 28–35 s — checkpoint or level completion
- final 2–3 s — `Unity · C# · Vüsal Aliyev`

Avoid long menus, loading screens, repeated deaths, or static camera shots.

## README Media Layout

When captures are ready, place publishable files under:

```text
docs/portfolio/media/
├── story-of-ellen-hero.webp
├── story-of-ellen-combat.webp
├── story-of-ellen-platforming.webp
├── story-of-ellen-progression.webp
└── story-of-ellen-level-2.webp
```

The hero animation should appear immediately below the README intro. Screenshots should be placed after `What I Built`, so the reviewer first understands the game and then sees the implementation details.

## Quality Checklist

- use Game view or a clean standalone build
- 16:9 framing
- no Unity editor chrome
- no debug text/log overlays
- stable frame pacing
- readable character silhouette
- show one mechanic per shot
- do not upload raw third-party asset-pack source files
- verify publication rights before making the showcase public
