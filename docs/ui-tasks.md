# UI Tasks

The remaining UI work, easiest first. Each task builds on what the one before it taught you.

Everything here uses uGUI and follows the same setup as the lives display. See [ui-prefabs-design.md](ui-prefabs-design.md) for how the prefabs fit together.

## Steps For Every Element

1. Close Unity, then make a new branch from `revamp` in GitHub Desktop. Reopen Unity.
2. Build the element as its own prefab in `Assets/_Project/Prefabs/UI/`.
3. Add it to `HUD` if it's part of the in-game UI.
4. Try it in Play mode in a scene with `HUD` and `Player`. Don't save shared scenes like `ConeTest`. Use a temporary test key on your branch if you need one.
5. Work through the task's checks in Play mode. Fix anything that fails.
6. Remove any temporary test scenes or test keys.
7. Open a PR into `revamp`, and say what you tried.

**Rules that apply to all of them:**
- Capitalize every word in titles and labels, like "Quit To Menu".
- Use `Nunito-Bold SDF` so the text matches the menus.
- Don't use Inspector slots to point at things in a scene, because a prefab can't keep them. Use `FindAnyObjectByType`, as `LivesDisplay` does.
- Add any new art or sound to `CREDITS.md` when you add it.

---

## Task 0: Finish The Lives Display

Branch: `ui/lives-display` (already exists)

- [x] Round Over and Game Over message
- [x] Remove `HudTestKeys`, `UITest` and `LivesDisplayTests`
- [ ] Drop `HUD` and `Player` into a scene and check the eyes show with nothing to wire up
- [ ] Open the PR into `revamp`. Say it helps #24 and #34, mention the small `Chaplin.cs` and `PlayerMovement.cs` cleanups, and that it includes the `feat/player-movement` merge.

---

## Task 1: Timer

Card: #34. Suggested branch: `ui/hud-timer`

**Goal:** a timer on the HUD that counts up during a level.

**What to make:**
- A `TimerDisplay` prefab with a TextMeshPro label, anchored top right.
- A `TimerDisplay` script that adds `Time.deltaTime` in `Update` and shows the time as `00:00`.

**Hints:**
- Look at how `LivesDisplay` is anchored, then do the same for the top right corner.
- `Mathf.FloorToInt` and the string format `{minutes:00}:{seconds:00}` will get you the display.
- `Time.deltaTime` stops when the game is paused, so Task 5 will pause your timer with no extra work.

**Checks:**
- [ ] It starts at `00:00` and counts up once a second
- [ ] It rolls over correctly at one minute (`00:59` to `01:00`)
- [ ] It stays top right at 1920x1080, 1280x720 and 16:10
- [ ] It's readable over a dark level

---

## Task 2: Collectible Counter

Card: #34. Suggested branch: `ui/hud-collectibles`. This task finishes #34, so that PR can say `Closes #34`.

**Goal:** show how many collectibles the player has picked up.

**What to make:**
- A `PlayerCollectibles` script on `Player.prefab` holding the count, with an `Add()` method and an event for when it changes.
- A `CollectibleCounter` prefab and script that shows the count on the HUD.

**Hints:**
- `PlayerLives` and `LivesDisplay` are your template. It's the same pattern: the player holds the number and fires an event, and the display listens and updates.
- Subscribe in `OnEnable` and unsubscribe in `OnDisable`, like `LivesDisplay` does. Forgetting to unsubscribe is a common cause of errors after reloading a scene.
- The real pickup comes from the collectible work on the board. Until then, use a test key that calls `Add()`.

**Checks:**
- [ ] It shows 0 at the start
- [ ] It goes up by one each time `Add()` is called
- [ ] It sits neatly with the timer and lives, and is readable over a dark level
- [ ] A scene with no `Player` logs a clear error and doesn't crash

---

## Task 3: Game Over Screen

Card: #32. Suggested branch: `ui/game-over-screen`

**Goal:** when lives run out, add Retry and Quit To Menu buttons under the "Game Over" text that `RoundOverMessage` already shows.

**Decide first:** #24 says the level restarts at zero lives, and #32 says game over leads to a retry. Showing this screen and letting Retry restart the level fits both, but check with your team and update #24.

**What to make:**
- A `GameOverScreen` prefab with the two buttons, hidden at the start, placed so it sits under the "Game Over" title.
- A script that listens to `PlayerLives.LivesChanged` and shows the buttons at zero lives. `RoundOverMessage.cs` already does the listening part, so read it first.

**Hints:**
- Keep the script on an object that stays active, and turn on only a child panel. A script on a switched-off object won't run or hear the event.
- Retry: `SceneManager.LoadScene(SceneManager.GetActiveScene().name)`.
- Quit To Menu: load `IntroScreen`.
- If you stop the game with `Time.timeScale = 0`, set it back to 1 **before** loading a scene. It carries over to the next scene, and everything will look frozen.
- Buttons need an EventSystem using `InputSystemUIInputModule`. `IntroScreen` has one to copy. Your first screen with buttons is where to decide how levels get one. Putting it inside `HUD` is the easiest for builders, but a scene with two logs a warning.

**Checks:**
- [ ] Hidden during play, and shown when lives reach zero
- [ ] Retry restarts the level with all lives, and the timer back at `00:00`
- [ ] Quit To Menu goes to the intro screen, and the menu buttons work
- [ ] Mouse hover and click work, and the menu sounds play if you've added `InterfaceSoundTrigger`

---

## Task 4: Level Complete Screen

Card: #32. Suggested branch: `ui/level-complete-screen`. This task finishes #32, so that PR can say `Closes #32`.

**Goal:** when the player reaches the end of a level, show the result and move on.

**What to make:**
- A `LevelComplete` trigger prefab that level builders place at the end of a level.
- A `LevelCompleteScreen` prefab with the time, the collectible count, and Next Level and Quit To Menu buttons.

**Hints:**
- `OnTriggerEnter2D` on a Collider2D with **Is Trigger** ticked. Check it's the player that touched it.
- Next Level can use `SceneManager.GetActiveScene().buildIndex + 1`. Compare against `SceneManager.sceneCountInBuildSettings` so the last level goes somewhere sensible instead of failing.
- Give `TimerDisplay` a way to read the time and to stop, so the finished time stays still.
- This relies on the scene order in Build Profiles, so note that for the level builders.

**Checks:**
- [ ] Touching the trigger shows the screen once, and nothing else sets it off
- [ ] The time and count on the screen match the HUD
- [ ] Next Level loads the next scene in Build Profiles (try it with two test scenes)
- [ ] The last level doesn't cause an error

---

## Task 5: Pause Menu

Card: #31. Suggested branch: `ui/pause-menu`. This is the hardest one, so it comes last.

**Goal:** Esc pauses the game, with Resume, Options and Quit To Menu.

**What to make:**
- First, turn `Options Panel` from `IntroScreen` into an `OptionsPanel` prefab, and check the intro screen still works. Do this before anything else, so both menus share one options page.
- A `PauseMenu` prefab and script, added to `HUD`.

**Hints:**
- `Keyboard.current.escapeKey.wasPressedThisFrame` tells you Esc was pressed this frame.
- Pause and resume with `Time.timeScale`. Set it back to 1 before quitting to the menu.
- The options page's Back button is wired to `IntroScreenController`, which won't be in a level. A `UnityEvent` or a small script on the prefab can let each menu decide what Back does.
- UI buttons still work at `timeScale` 0, but anything animated with `Time.deltaTime` won't move. `Time.unscaledDeltaTime` gets around that.
- Check what the player can still do while paused. `PlayerMovement` and the vision cone may still read the mouse or keyboard.
- Don't let Esc open the pause menu while Game Over or Level Complete is showing.

**Checks:**
- [ ] Esc pauses, and Esc or Resume carries on
- [ ] The timer, player and enemies all stop while paused
- [ ] The player can't turn or act while paused
- [ ] Options opens, changes are saved, and Back returns to the pause menu
- [ ] Quit To Menu works, and starting again isn't frozen
- [ ] The intro screen's options page still works

---

## Only If There's Time

**Level select (#64):** let players replay unlocked levels from the menu. The board marks this as only if there is time.

## Before Submission

From #63: make sure every test scene and all test code is gone, and `CREDITS.md` is complete.
