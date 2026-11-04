# Lives Display Build Plan

**Goal:** Turn the eye lives display into a drag-in `HUD` prefab that works in any scene with no wiring.

**Approach:** `LivesDisplay` finds `PlayerLives` by itself, and `PlayerLives` moves onto `Player.prefab`. Code changes and the automated test come first. The prefabs and test scene are then made by hand in Unity, tried against the checklist, and the test code is removed before the PR merges.

**Tech:** Unity 6000.6.3f1, uGUI, Unity Test Framework (PlayMode).

**Spec:** [ui-prefabs-design.md](ui-prefabs-design.md)

## Rules For Every Task

- Close Unity before switching branches.
- Branch from `revamp`. The PR goes back into `revamp`.
- PR titles use `type: what changed`.
- Nothing in commits, PRs or files mentions AI tools, and commits have no co-author lines.
- Test scenes and test keys must not reach `revamp`.

## Cases To Watch

Each of these has a test in Task 1.

- A level turns the HUD off, a life is lost, and the HUD comes back on: the eyes show the current lives.
- The eye count and `maxLives` differ: a warning is logged, and nothing crashes.
- The display is destroyed, as on a scene reload: later life changes cause no errors.
- The player is destroyed before the display: no errors.

---

## Task 1: Make The Display Find The Player (Code)

**Who:** Code, with Unity closed if tests are run from the command line.

**Files:**
- Modify: `Assets/_Project/Scripts/Runtime/Gameplay/PlayerLives.cs`
- Modify: `Assets/_Project/Scripts/Runtime/UI/LivesDisplay.cs`
- Modify: `Assets/_Project/Scripts/Runtime/UI/HudTestKeys.cs`
- Create: `Assets/_Project/Tests/PlayMode/LivesDisplayTests.cs`

**Produces for later tasks:** `PlayerLives.MaxLives`. `LivesDisplay` and `HudTestKeys` have no `playerLives` Inspector slot. `LivesDisplay` has no `youDiedText` slot.

- [ ] **Step 1: Create the branch**

```bash
git switch -c ui/lives-display origin/revamp
```

Expected: the new branch contains `ec0e9dd`, and `docs/` comes across untracked.

- [ ] **Step 2: Write the failing tests in `LivesDisplayTests`**

Namespace `TunnelVision.Tests`. Helpers:
- `CreatePlayer()` makes a GameObject with `PlayerLives`, which defaults to 3 lives.
- `CreateDisplay(int eyeCount)` makes an inactive GameObject with `LivesDisplay` and `eyeCount` `Image`s, sets the private `eyes` field by reflection, then activates it.
- `[TearDown]` uses `Object.DestroyImmediate` on everything created, so no `PlayerLives` leaks into the next test.

Each test waits one frame after creating objects, so `Start` has run. "Bright" means `Color.white`, and "dim" means `new Color(0.3f, 0.3f, 0.3f, 0.4f)`.

| Test | Setup | Assert |
|---|---|---|
| `Start_ShowsAllEyesBright` | Player, 3 eyes | All 3 bright |
| `LoseLife_DimsTheLastBrightEye` | Player, 3 eyes, `LoseLife()` | Eyes 0 and 1 bright, eye 2 dim |
| `Restart_BrightensAllEyes` | Player, 3 eyes, `LoseLife()` x3, `Restart()` | All 3 bright |
| `NoPlayerLives_LogsErrorAndTurnsOff` | No player, 3 eyes | `LogAssert.Expect(LogType.Error, "LivesDisplay: no PlayerLives found in the scene. Add the Player prefab.")`, and `display.enabled` is false |
| `EyeCountMismatch_LogsWarning_AndKeepsWorking` | Player, 2 eyes, `LoseLife()` x2 | `LogAssert.Expect(LogType.Warning, "LivesDisplay: has 2 eyes but the player has 3 max lives.")`, eye 0 bright, eye 1 dim |
| `ReEnabled_ShowsCurrentLives` | Player, 3 eyes, disable the display, `LoseLife()`, enable it | Eye 2 dim |
| `DestroyedDisplay_StopsListening` | Player, 3 eyes, destroy the display, `LoseLife()` | No errors logged |
| `PlayerDestroyedFirst_CausesNoErrors` | Player, 3 eyes, destroy the player, then the display | No errors logged |

- [ ] **Step 3: Run the tests and confirm they fail**

In Unity: **Window > General > Test Runner > PlayMode**, run `LivesDisplayTests`.

Or with Unity closed:

```bash
"C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter LivesDisplayTests -testResults lives-results.xml -logFile lives-test.log
```

Expected: failures with `NullReferenceException`, because the `playerLives` slot is empty. Don't commit the result or log files.

- [ ] **Step 4: Add `public int MaxLives => maxLives;` to `PlayerLives`**

- [ ] **Step 5: Change `LivesDisplay`**

- Make `playerLives` a plain private field, and remove `youDiedText` and the line that uses it.
- `Awake()`: `playerLives = FindAnyObjectByType<PlayerLives>();`. If it's null, log the exact error from the test, set `enabled = false` and return.
- `OnEnable()`: subscribe to `LivesChanged`, then call `ShowLives(playerLives.CurrentLives)` so a re-enabled display catches up.
- `OnDisable()`: unsubscribe only if `playerLives != null`.
- `Start()`: if `eyes.Length != playerLives.MaxLives`, log the exact warning from the test. Then call `ShowLives(playerLives.CurrentLives)`, because the player's `Awake` may run after this display's `OnEnable`.

- [ ] **Step 6: Change `HudTestKeys`**

Make `playerLives` a plain private field, set it in `Awake()` with `FindAnyObjectByType<PlayerLives>()`, and return early from `Update()` if it's null.

- [ ] **Step 7: Run the tests and confirm they pass**

Same command as Step 3. Expected: all 8 pass. Then run the whole PlayMode suite and confirm `IntroScreenTests` still passes.

- [ ] **Step 8: Commit**

```bash
git add Assets/_Project/Scripts/Runtime Assets/_Project/Tests/PlayMode docs/ui-prefabs-design.md docs/lives-display-plan.md
git commit -m "ui: make the lives display find the player by itself"
```

---

## Task 2: Make The Prefabs (Unity Editor)

**Who:** kemuri, by hand in Unity.

**Files:**
- Create: `Assets/_Project/Prefabs/UI/LivesDisplay.prefab`
- Create: `Assets/_Project/Prefabs/UI/HUD.prefab`
- Modify: `Assets/_Project/Prefabs/Player.prefab`

**Depends on:** Task 1 being compiled in Unity.

- [ ] **Step 1: Add lives to the player**

Open `Player.prefab`, select the root `Player`, and add the `Player Lives` component. Leave `Max Lives` at 3. Save the prefab.

Check: the component shows on the `Player` root object.

- [ ] **Step 2: Create the folder**

Create `Assets/_Project/Prefabs/UI/`.

- [ ] **Step 3: Make `LivesDisplay.prefab`**

Open `UITest.unity`. Drag `Lives Display` from the Hierarchy into `Prefabs/UI/`, and choose **Original Prefab** if asked. Rename the asset `LivesDisplay`.

Check: `Lives Display` turns blue in the Hierarchy, and its `Eyes` list still has 3 entries.

- [ ] **Step 4: Make `HUD.prefab`**

Drag `HUD Canvas` into `Prefabs/UI/`, choose **Original Prefab**, and rename the asset `HUD`. Open `HUD`, delete `You Died Text`, rename the root `HUD`, and save.

Check: `HUD` contains `Lives Display` as a blue nested prefab, with anchors top left, position (40, -40). The Canvas Scaler is still 1920x1080, match 0.5, and the Canvas sort order is 10.

- [ ] **Step 5: Leave `UITest` unchanged**

Open another scene and choose **Don't Save** for `UITest`.

- [ ] **Step 6: Commit**

In GitHub Desktop, check that only the prefabs, their `.meta` files and the `Prefabs/UI` folder `.meta` are listed, and discard anything else Unity changed. Commit message: `ui: add HUD and lives display prefabs`

---

## Task 3: Try It In Unity (Test Scene)

**Who:** kemuri, by hand in Unity.

**Files:**
- Create: `Assets/_Project/Scenes/Test/LivesTest.unity` (temporary)

**Depends on:** Task 2.

- [ ] **Step 1: Build the scene**

Create an empty scene and save it as `Scenes/Test/LivesTest.unity`. Don't add it to Build Profiles. Keep the Main Camera, drag in `Player` and `HUD`, then add an empty object named `Test Keys` with the `Hud Test Keys` component.

Check: no slots to fill in on `Hud Test Keys`. `Player` falling with no ground under it is fine.

- [ ] **Step 2: Run the checklist**

Work through **Lives Checklist** in the spec. Note anything that fails, with what you saw.

- [ ] **Step 3: Fix and repeat**

For each failure, fix it, add a test to `LivesDisplayTests` if it's a code bug, and rerun the checklist from the top. Commit fixes on the branch with `fix: ...`.

- [ ] **Step 4: Undo the trial changes**

Revert the `maxLives` change, the disabled `Player`, the moved display and the changed eye colour. Check in GitHub Desktop that `Player.prefab` and `HUD.prefab` show no leftover differences.

---

## Task 4: Remove The Test Code And Open The PR

**Who:** Code, after kemuri confirms the checklist passes.

**Files:**
- Delete: `Assets/_Project/Scenes/Test/` and its `.meta`
- Delete: `Assets/_Project/Scripts/Runtime/UI/HudTestKeys.cs` and its `.meta`
- Maybe delete: `Assets/_Project/Scenes/UITest.unity` and its `.meta`

- [ ] **Step 1: Ask kemuri about `UITest.unity`**

Delete it only on a yes. Without `HudTestKeys`, its `Game Manager` would have a missing script if it stays.

- [ ] **Step 2: Delete the test files**

Check that this search of `Assets/_Project` finds nothing, not counting any kept `UITest.unity`:

```bash
git grep -n "HudTestKeys\|LivesTest" -- "Assets/_Project/*.cs" "ProjectSettings/*"
```

- [ ] **Step 3: Run the whole PlayMode suite**

Expected: all tests pass, and the Console shows no compile errors.

- [ ] **Step 4: Commit**

```bash
git commit -am "ui: remove lives test code"
```

Include the deleted files, and check with `git status` that nothing is left over.

- [ ] **Step 5: Push and open the PR into `revamp`**

Ask kemuri before pushing. Title: `ui: add HUD and lives display prefabs`. The body says what was tried from the checklist and notes that it helps #24 (removes `HudTestKeys`) and #34 (the HUD now exists as a prefab). It closes neither card.

- [ ] **Step 6: Confirm CI passes on the PR**
