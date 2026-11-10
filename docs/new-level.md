# Starting A New Level

1. In the Project window, select `Assets/_Project/Scenes/Levels/LevelTemplate` and press **Ctrl+D**. Rename the copy, for example `Level1_BigTop`, and open it.
2. Open **Window > 2D > Tile Palette** and choose `BigTop`. Paint solid ground on `Grid/Ground` and decoration on `Grid/Deco`.
3. Select `LevelRig/CameraBounds` and use **Edit Collider** to fit the new level. The camera stops at these edges.
4. Move `LevelRig/Player` to where the level starts, on solid ground. His Y should be the top of the floor plus 1.42, so his feet are just above it. Then press Play and check he stands still. If he falls through painted ground, select `Grid/Ground` and untick and re-tick `Tilemap Collider 2D`, then save the scene.
5. Replace the sprites under `Background`. If the level is wider than the template, move or widen them so they still cover the view at both ends.
6. Drop prefabs into the matching empty groups: `Platforms`, `Enemies`, `Hazards`, `Pickups`, `Checkpoints` and `Goal`.
7. If the level has pits, select `Hazards/KillZone` and make its Box Collider wider than the level, sitting below the bottom of the camera bounds. Place `Prefabs/Level/Checkpoint` with its origin on top of the ground, with nothing dangerous at that spot, because the player respawns there.
8. Add a `LevelGoal` at the end of the level and set **Level Index** to its place on the wheel, counting from 0 (Level 2 is 1). If it stays at 0, the next level never unlocks. Then add the scene to **Level Scenes** on the wheel and to the build's scene list.

## The Rig

`LevelRig` holds what every level shares: Nicholas, the camera, the global light, the HUD and the camera bounds. Open the prefab to change something for every level. A change made inside one level is an override and shows in bold in the Inspector, so only make those when they're meant for that level alone, as with `CameraBounds`.

`Global Light 2D` stays at intensity 0, the same as `ConeTest`, so nothing is visible outside the cone: not the ground, the props or the background.
