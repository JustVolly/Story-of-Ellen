# Story of Ellen

A 2D Unity platform adventure with Spirit World traversal, dash/wall-jump abilities, memory fragments, checkpoints and a three-level campaign.

**Engine:** Unity 6000.5.2f1 • URP 2D • Tilemap • Cinemachine

## Production campaign

The gameplay foundation adds production movement, UI, progression, Spirit World, and boss systems. The content branch provides a repeatable scene baker using the art, audio and prefabs already in this repository.

| Level | Scene | Theme |
| --- | --- | --- |
| 1 — Spirit Discovery | `Assets/Scenes/OneScene.unity` | Mountains and haunted forest |
| 2 — Dash Mastery | `Assets/Scenes/TwoScene.unity` | Desert ruins and Guardian encounter |
| 3 — Spirit Ascent | `Assets/Scenes/ThreeScene.unity` | Graveyard, crypt, Spirit/Dash mastery |

### Build the playable scenes

1. Check out the branch containing `Assets/Editor/EllenCampaignContentBuilder.cs`.
2. Run `git lfs install && git lfs pull` to download the actual art and sound assets.
3. Open in Unity **6000.5.2f1**, wait for compilation, and choose **Ellen > Production > Build Complete Campaign**.
4. Choose **Ellen > Production > Validate Complete Campaign**.
5. Run Play Mode on the menu and all three scenes, review the generated content, then commit the saved `.unity` scene files.

See **[production level build guide](docs/LEVEL_CONTENT_BUILD.md)** for detailed CLI instructions and acceptance tests.

> Editor content generation is implemented, but this branch does not claim a completed Unity Play Mode or shipping build verification. The generated scenes must be baked and tested in Unity before merging into a release branch.
