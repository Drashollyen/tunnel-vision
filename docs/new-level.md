# Starting A New Level

New levels start as a copy of Level 1, which already has the player, camera, death sequence and HUD set up.

1. In the Project window, select `Assets/_Project/Scenes/Levels/Level1_BigTop` and press **Ctrl+D**. Rename the copy, for example `Level2_Stalls`, and open it.
2. Delete what belongs to Level 1 only: the children of `Platforms`, `Enemy`, `DecoSign` and `Background`. Keep everything else (see [What Every Level Keeps](#what-every-level-keeps)).
3. Build the ground under `Platforms`. Each platform is a Sprite Renderer with **Draw Mode** set to **Tiled**, plus a **Box Collider 2D** the same size. Set each platform's **Layer** to **Ground**, or Nicholas can't jump off it.
4. Select `Confiner` and use **Edit Collider** to fit the new level. The camera stops at these edges.
5. Move `Player` to where the level starts, standing on a platform. Press Play and check he lands and stands still.
6. If the level has pits, make sure `KillPlane` sits below them and its Box Collider is wider than the level.
7. Place the cotton candy `Checkpoint` on top of the ground, with nothing dangerous at that spot. Its **Respawn Offset** is where the player comes back, measured from the checkpoint, so check it lands on solid ground.
8. Move `LevelGoal` (with its `Door`) to the end of the level. Set **Level Index** to its place on the wheel, counting from 0 (Level 2 is 1). If it stays at 0, the next level never unlocks.
9. Add the scene to the build's scene list (**File > Build Profiles**), and on the level select's `Wheel` object, add its name to **Level Scenes** in the matching slot.

## What Every Level Keeps

| Object | What it does |
|---|---|
| `Player` | The shared `Player` prefab. Change the prefab to change Nicholas in every level. A change made inside one level is an override and shows in bold in the Inspector |
| `Main Camera`, `CinemachineCamera` | Follow the player and stay inside `Confiner` |
| `Global Light 2D` | Stays at intensity 0, so nothing is visible outside the cone. The death sequence lights it up briefly |
| `DeathCanvas` | The black fade for the death sequence. It sits just under the HUD, so the Round Over text shows on top |
| `HUD` | Lives eyes and the Round Over / Game Over text |
| `PlayerUI` | The EventSystem and the gradient overlay |
| `KillPlane` | Costs a life when the player falls into a pit |

The player's `Player Respawn` uses this level's `Global Light 2D` and `BlackFade`. They're already linked in the copy, so leave them in place.
