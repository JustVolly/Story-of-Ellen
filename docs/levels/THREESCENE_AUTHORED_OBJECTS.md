# ThreeScene: authored gameplay objects

The ThreeScene YAML file is modified by `tools/bake_three_scene_authored.py`
instead of relying on a separate Unity Editor menu to generate content.
The script is deterministic, uses standard Python, creates a backup through
Git history, preserves legacy object IDs and serialized prefab links, and is
idempotent.

It adds a **[AstralCryptAuthored]** root containing:
- four physical ruin barricades;
- fifteen visible one-way stepping-stone platforms;
- three magenta contact-damage rune hazards;
- two kinematic moving platforms;
- two ranged, 3-HP spectral sentinels using the existing PlayerHealth and
  EnemyHealth gameplay APIs.

A main-branch GitHub Actions workflow bakes these objects into the committed
`Assets/Scenes/ThreeScene.unity` without sending the giant Unity scene through
the remote file editing API. It commits only ThreeScene. If GitHub Actions is
unavailable, run the script locally from the project root:

```powershell
py -3 tools/bake_three_scene_authored.py
```

Then inspect the scene in Unity and commit only ThreeScene when satisfied.
The obstacles have been placed against existing player spawn x≈-41,
finish x≈724, and legacy floor y≈-42. A Unity Play Mode pass is required
to verify collider shapes, reachability, damage feedback and enemy difficulty.

The procedurally built `[LevelDesign]` route remains a separate optional
overlay; these hand-authored gameplay objects are in `[AstralCryptAuthored]`
and are not deleted by production layer rebuilds.
