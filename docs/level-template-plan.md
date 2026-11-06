# Level Template Build Plan

**Goal:** A `LevelTemplate` scene that level builders duplicate to get a playable, lit, dressed empty level with real pixel art, for #28.

**Approach:** Everything every level shares goes in one `LevelRig` prefab: Nicholas, the camera, the darkness, the HUD and the camera bounds. A fix to the rig then reaches every level. The template scene holds the rig plus the parts each level changes: a tilemap painted with real tiles, a three-layer parallax background and empty groups for the M1 and M3 pieces. Art comes from the packs picked on Nov 3 (#17), so the template doubles as the first real use of them.

**Tech:** Unity 6000.6.3f1, URP 2D lights, Tilemap and Rule Tiles (`com.unity.2d.tilemap.extras`), Cinemachine 3, Pixel Perfect Camera, Unity Test Framework.

**Issue:** [#28 Build the level template scene](https://github.com/Drashollyen/tunnel-vision/issues/28). Related: #17 (art packs), #22, #23, #25, #27 (systems that drop into the template's empty groups), #54 (parallax per level).

## Rules For Every Task

- Close Unity before switching branches. Branch `feat/level-template` from `revamp`; the PR goes into `revamp` and says `Closes #28`.
- World art: 16 pixels per unit, Point filter, no compression (CONTRIBUTING). A pack drawn on another grid keeps its own PPU so one tile is one unit.
- Downloaded packs go in `Assets/ThirdParty/<Asset Name>/`, untouched, and get a `CREDITS.md` line in the same PR.
- Nothing in commits, PRs or files mentions AI tools, and commits have no co-author lines.
- No placeholder squares in the finished template. Every visible sprite comes from a credited pack or the team.
- Tests are written with the code; the user runs them in **Test Runner** and plays the scene by hand.

## Art Sources

From the Nov 3 research. Each still needs a look in Unity next to Nicholas before it's final.

| Use in the template | First choice | Fallback |
|---|---|---|
| Ground and wall tiles | Free 2D Mega Pack (Brackeys), Asset Store | Kenney Pixel Platformer (CC0, 18px tiles, so PPU 18) |
| Floor props (crates, signs, lamps) | Free 2D Mega Pack | Kenney Pixel Platformer |
| Big Top background, 3 layers | Team's `tent-bg.png` / `tent.png` for far and middle, Mega Pack props for near | Mega Pack backgrounds |
| Enemy preview (not placed) | Bandits, Hero Knight | none, enemies are M3 |

`curtain.png` stays out of the template until its source is confirmed (#19).

Asset Store packs need **Add to My Assets** on your account, then **Window > Package Manager > My Assets > Import**. Only you can do that step.

## Cases To Watch

Each has a test in Task 4 or a check in Task 5.

- Nicholas walks off either edge of the starting floor: walls stop him, nothing falls out of the world.
- The cone points away from every lit object: the screen is never pure black (far background still faint).
- A builder duplicates the template and moves the camera bounds: the camera stops at the new edges, not the template's.
- A builder deletes the HUD from one level by mistake: the level still plays, and the template test catches it in the template itself.
- Pixel Perfect Camera at window sizes that aren't a multiple of the reference resolution: no shimmer while walking.

---

## Task 1: Import the art and set it up

**Files:**
- Create: `Assets/ThirdParty/Brackeys/Free 2D Mega Pack/` (only the folders the template uses: tiles, props, backgrounds)
- Modify: `CREDITS.md`
- Modify: `.meta` files of imported sprites (import settings)

- [ ] **Step 1 (you):** Add Free 2D Mega Pack to My Assets and import it into `Assets/ThirdParty/Brackeys/`. Untick folders we won't use (characters, sounds, scripts, demo scenes) to keep the repo small.
- [ ] **Step 2:** Set every imported sprite to the pixel art settings. Tile sheets: Sprite Mode Multiple, sliced to the pack's tile size, pivot Center.
- [ ] **Step 3:** Open the sheets next to Nicholas in an unsaved scene and confirm the tile size reads right against him (about 2.5 tiles tall). If not, report back before going on; that decides the fallback.
- [ ] **Step 4:** Add the credit line: `| Free 2D Mega Pack, Brackeys | [assetstore.unity.com/packages/2d/free-2d-mega-pack-177430](https://assetstore.unity.com/packages/2d/free-2d-mega-pack-177430) | Standard Unity Asset Store EULA |`
- [ ] **Step 5:** Commit `art: import Free 2D Mega Pack for level tiles`.

This also ticks off most of #17. Bandits and Hero Knight can come in the same PR or with the enemy cards.

## Task 2: Tiles and palette

**Files:**
- Create: `Assets/_Project/Tiles/Ground.asset` (Rule Tile: top edge, inner fill, left/right edges, corners)
- Create: `Assets/_Project/Tiles/Props/*.asset` (one Tile per prop sprite)
- Create: `Assets/_Project/Tiles/Palettes/BigTop.prefab` (Tile Palette holding both)

- [ ] **Step 1:** Build `Ground` as a Rule Tile so builders paint with one brush and edges sort themselves out.
- [ ] **Step 2:** Make the `BigTop` palette with `Ground` first, then props.
- [ ] **Step 3:** Paint a 10-tile test strip in an unsaved scene: corners and edges join with no gaps or seams.
- [ ] **Step 4:** Commit `level: add ground rule tile and Big Top palette`.

Zones 2 to 5 get their own palettes under #44.

## Task 3: ParallaxLayer script

**Files:**
- Create: `Assets/_Project/Scripts/Runtime/Level/ParallaxLayer.cs`, namespace `TunnelVision.Level`
- Test: `Assets/_Project/Tests/EditMode/ParallaxLayerTests.cs`

**Interfaces:**
- Produces: `ParallaxLayer : MonoBehaviour` with `[SerializeField, Range(0f, 1f)] float factor` (0 moves with the world, 1 sticks to the camera) and `public static Vector3 OffsetFor(Vector3 cameraDelta, float factor)`. Reads `Camera.main` in `LateUpdate` and moves by `OffsetFor(delta, factor)`, x and y only.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void FactorZeroDoesNotMove() =>
    Assert.AreEqual(Vector3.zero, ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 0f));
[Test] public void FactorOneFollowsCameraExactly() =>
    Assert.AreEqual(new Vector3(4, 2, 0), ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 1f));
[Test] public void HalfFactorMovesHalfAsFar() =>
    Assert.AreEqual(new Vector3(2, 1, 0), ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 0.5f));
[Test] public void IgnoresCameraZ() =>
    Assert.AreEqual(0f, ParallaxLayer.OffsetFor(new Vector3(0, 0, -10), 1f).z);
```

- [ ] **Step 2:** You run EditMode tests: the four fail (type missing).
- [ ] **Step 3:** Implement `ParallaxLayer`.
- [ ] **Step 4:** You run EditMode tests: all pass.
- [ ] **Step 5:** Commit `level: add parallax layer`.

#54 reuses this for every level's background.

## Task 4: LevelRig prefab and the template scene

**Files:**
- Create: `Assets/_Project/Prefabs/Level/LevelRig.prefab`
- Create: `Assets/_Project/Scenes/Levels/LevelTemplate.unity`
- Modify: `ProjectSettings/TagManager.asset` (sorting layers, if missing: `Background`, `Ground`, `Deco`, `Body`, `Foreground`)
- Test: `Assets/_Project/Tests/PlayMode/LevelTemplateTests.cs`

**LevelRig hierarchy** (all prefab-internal references, so nothing needs rewiring after duplicating):

| Object | Components and settings |
|---|---|
| `LevelRig` | none, position (0,0) |
| `Player` | `Player.prefab` instance, at (0, 2) |
| `Main Camera` | Camera (orthographic), CinemachineBrain, Pixel Perfect Camera (16 PPU, reference 480 x 270, upscale render texture off, crop off) |
| `CinemachineCamera` | Follow `Player`, Position Composer (small dead zone, look-ahead off), Confiner 2D pointing at `CameraBounds`, Cinemachine Pixel Perfect extension |
| `CameraBounds` | PolygonCollider2D, Is Trigger, on Ignore Raycast; builders override its points per level |
| `Global Light 2D` | Global, intensity 0, same as `ConeTest`, so only the cone lights the world |
| `HUD` | `HUD.prefab` instance |
| `EventSystem` | EventSystem + InputSystemUIInputModule |

**LevelTemplate hierarchy:**

| Object | Contents |
|---|---|
| `LevelRig` | the prefab above |
| `Grid` | Grid (cell 1) with `Ground` tilemap (Ground layer, TilemapCollider2D used by composite, static Rigidbody2D, CompositeCollider2D; sorting `Ground`) and `Deco` tilemap (no collider; sorting `Deco`) |
| `Background` | `Far` (factor 0.9, Sprite-Unlit-Default material at 30% tint, so it's always faintly visible), `Middle` (0.6, lit), `Near` (0.3, lit); sorting `Background` |
| `Platforms`, `Enemies`, `Hazards`, `Pickups`, `Checkpoints`, `Goal` | empty groups for #35, #38-#42, #22, #27, #23, #25 |

Painted content: a floor about 40 tiles wide, walls 6 tiles high at both ends, one raised step to jump onto, and a few props on `Deco`. `CameraBounds` hugs the walls.

- [ ] **Step 1: Write the failing PlayMode tests** (load `LevelTemplate` by path through `EditorSceneManager.LoadSceneInPlayMode`, wait one second)

```csharp
[UnityTest] public IEnumerator HasExactlyOnePlayerWithLives()    // FindObjectsByType<PlayerLives>().Length == 1, CurrentLives == MaxLives
[UnityTest] public IEnumerator HudFindsThePlayer()               // a LivesDisplay exists and is active
[UnityTest] public IEnumerator PlayerLandsOnGround()             // after 1s: player rb.linearVelocity.y ~ 0 and player y > lowest Ground tile
[UnityTest] public IEnumerator WorldIsDarkButFarLayerUnlit()     // global Light2D intensity == 0; Far renderer uses an unlit material
[UnityTest] public IEnumerator CameraIsConfinedToBounds()        // CinemachineConfiner2D.BoundingShape2D is the rig's CameraBounds collider
```

- [ ] **Step 2:** You run PlayMode tests: they fail (scene missing).
- [ ] **Step 3:** Build the prefab and scene in the open Editor through the Unity CLI, so you don't have to close Unity. If it can't reach the Editor, you build them from the two tables.
- [ ] **Step 4:** You run PlayMode tests: all pass.
- [ ] **Step 5:** Commit `level: add level rig prefab and template scene`.

## Task 5: Hands-on check and the how-to note

**Files:**
- Create: `docs/new-level.md`
- Modify: `CONTRIBUTING.md` (one line linking the note), `ProjectSettings/EditorBuildSettings.asset` (template stays out of the build list)

- [ ] **Step 1:** Write `docs/new-level.md`, short, in steps: duplicate `LevelTemplate` (Ctrl+D) into `Scenes/Levels/`, rename to `Level1_BigTop` and so on; paint with the zone palette; move `CameraBounds` points; drop prefabs into the matching groups; never edit the rig inside a level unless it's meant for that level only (overrides show in bold); add the scene to the level order once #25 lands.
- [ ] **Step 2 (you): play the template.** Check:
  1. Nicholas lands on the floor, walks, jumps onto the step and can't leave past the walls.
  2. The cone lights tiles and props; the far background stays faintly visible everywhere.
  3. The camera stops at the walls and the background layers drift at different speeds.
  4. No pixel shimmer while walking, in both a small window and full screen.
  5. The HUD eyes show 3 lives.
- [ ] **Step 3 (you): duplicate test.** Duplicate the template, widen the floor, move `CameraBounds`, play: the camera follows the new edges. Delete the duplicate afterwards.
- [ ] **Step 4:** Commit `docs: explain how to start a new level`, push, open the PR into `revamp` with `Closes #28`.

## Open Points

- **Ben's `feat/player-movement`** has new Nicholas sprites and a camera confiner commit. If it merges first, the rig uses its Player prefab as is; if the confiner overlaps, keep one.
- **ConeTest** stays as the cone sandbox. Once Level 1 exists, the Play button still points at `ConeTest` until #25 changes it.
- **Kill zone at the bottom** belongs to #22. The template has walls and no pits, so it's playable without one.
