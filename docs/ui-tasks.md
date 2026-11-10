# UI Tasks

All the UI work left in the prototype, in the order to do it. Each task builds on what the one before it taught you.

Everything here uses uGUI and follows the same setup as the lives display. See [ui-prefabs-design.md](ui-prefabs-design.md) for how the prefabs fit together.

## Overview

| # | Element | Where | Card | Waits on |
|---|---|---|---|---|
| 1 | Hunger Timer Display | HUD | #34, #26 | Nothing (build the timer itself if #26 isn't started) |
| 2 | Collectible Counter | HUD | #34 | #27 for the real pickup |
| 3 | Game Over Screen | HUD | #32 | Nothing |
| 4 | Level Complete Screen | HUD | #32 | #25 for the level order |
| 5 | Pause Menu And Shared Options | HUD and intro screen | #31 | Nothing |
| 6 | Checkpoint Message | HUD | #59 | #23 for the checkpoint event |
| 7 | Tutorial Prompts | Placed in Level 1 | #47 | Level 1 layout (#45) |
| 8 | Ending Screen | Its own scene | #55 | Task 4 |
| 9 | Music And Effects Sliders | Options | #56 | The audio mixer |
| 10 | Design Pass | Everything | #59 | Team pixel art |

Tasks 1 to 5 don't need anyone else's work, so do them first. Tasks 6 to 9 hang off teammates' cards, so pick them up as those land. Task 10 goes last, once the art is settled.

Already done: lives display and Round Over message (#77), options page, info note and menu sounds (#71, #75).

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
- Use `Jersey10 SDF` so the text matches the menus.
- Don't use Inspector slots to point at things in a scene, because a prefab can't keep them. Use `FindAnyObjectByType`, as `LivesDisplay` does.
- When the UI needs to know something happened in the game, have the gameplay script fire an event and the UI listen, like `PlayerLives.LivesChanged`. Then you can build the UI before the gameplay card is finished, using a test key to fire the event.
- Add any new art or sound to `CREDITS.md` when you add it.

---

## Task 1: Hunger Timer Display

Cards: #34 and #26. Suggested branch: `ui/hud-timer`

**Decide first:** #34 asks for "a timer" and #26 asks for a hunger timer that runs out. Two clocks on screen would be confusing, so the HUD timer should **be** the hunger timer, counting down. Check this with the team and note it on both cards. #26 also needs to say whether running out costs a life or restarts the level.

**Goal:** a countdown on the HUD, top right, that shows how long Nicholas has before he keels over.

**What to make:**
- A `HungerTimer` script that holds the time. If nobody has started #26, make it yourself and say so on the card. Give it a `Time Limit` in seconds, set per level, a read-only `TimeLeft` and `TimeTaken`, a way to stop it, and a `TimeRanOut` event.
- A `TimerDisplay` prefab and script with a TextMeshPro label, anchored top right, that reads `HungerTimer` and shows `00:00`.

**Hints:**
- Where `HungerTimer` lives decides how levels set their time. On its own object in the level template (#28) is easiest for builders, since each level edits its own copy.
- Look at how `LivesDisplay` is anchored, then do the same for the top right corner.
- `Mathf.CeilToInt` for a countdown, so it shows `00:01` for the last second rather than `00:00`. The string format `{minutes:00}:{seconds:00}` does the rest.
- Turn the label red under 10 seconds so running low is obvious.
- `Time.deltaTime` stops when the game is paused, so Task 5 will pause your timer with no extra work.
- What happens on `TimeRanOut` belongs to #26. The display only shows the number.

**Checks:**
- [ ] It starts at the level's time limit and counts down once a second
- [ ] It rolls over correctly at one minute (`01:00` to `00:59`)
- [ ] It turns red under 10 seconds, and stops at `00:00` without going negative
- [ ] Changing the time limit in one scene doesn't change other scenes
- [ ] It stays top right at 1920x1080, 1280x720 and 16:10
- [ ] It's readable over a dark level

---

## Task 2: Collectible Counter

Card: #34. Suggested branch: `ui/hud-collectibles`. This task finishes #34, so that PR can say `Closes #34`.

**Goal:** show how many collectibles the player has picked up.

**What to make:**
- A `PlayerCollectibles` script on `Player.prefab` holding the count, with an `Add()` method and an event for when it changes. Agree this with whoever takes #27, since their pickup will call `Add()`.
- A `CollectibleCounter` prefab and script that shows the count on the HUD, with a small icon next to the number.

**Hints:**
- `PlayerLives` and `LivesDisplay` are your template. It's the same pattern: the player holds the number and fires an event, and the display listens and updates.
- Subscribe in `OnEnable` and unsubscribe in `OnDisable`, like `LivesDisplay` does. Forgetting to unsubscribe is a common cause of errors after reloading a scene.
- Until #27 lands, use a test key that calls `Add()`.
- Place it under the timer, or next to the lives, so the top of the screen stays tidy.

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
- Buttons need an EventSystem using `InputSystemUIInputModule`. `IntroScreen` has one to copy. This is your first in-level screen with buttons, so decide here how levels get one. Putting it inside `HUD` is the easiest for builders, but a scene with two logs a warning.

**Checks:**
- [ ] Hidden during play, and shown when lives reach zero
- [ ] Retry restarts the level with all lives, and the timer back at the level's time limit
- [ ] Quit To Menu goes to the intro screen, and the menu buttons work
- [ ] Mouse hover and click work, and the menu sounds play if you've added `InterfaceSoundTrigger`

---

## Task 4: Level Complete Screen

Card: #32. Suggested branch: `ui/level-complete-screen`. This task finishes #32, so that PR can say `Closes #32`.

**Goal:** when the player reaches the end of a level, show the result and move on.

**What to make:**
- A `LevelCompleteScreen` prefab with the time taken, the collectible count, and Next Level and Quit To Menu buttons.
- If #25 hasn't made the goal yet, a `LevelGoal` trigger prefab that level builders place at the end of a level. Agree with whoever has #25 which of you makes it, so there aren't two.

**Hints:**
- `OnTriggerEnter2D` on a Collider2D with **Is Trigger** ticked. Check it's the player that touched it.
- #25 wants the level order "set in one place". Next Level should ask that one place for the next scene rather than working it out itself. If it doesn't exist yet, `SceneManager.GetActiveScene().buildIndex + 1` works for now, compared against `SceneManager.sceneCountInBuildSettings`.
- After Level 5, Next Level goes to the ending (Task 8), not an error.
- Stop `HungerTimer` when the goal is reached, so the finished time stays still and the timer can't run out on this screen.

**Checks:**
- [ ] Touching the goal shows the screen once, and nothing else sets it off
- [ ] The time and count on the screen match the HUD
- [ ] The timer stops, and can't cost a life while the screen is showing
- [ ] Next Level loads the next level (try it with two test scenes)
- [ ] The last level doesn't cause an error

---

## Task 5: Pause Menu And Shared Options

Card: #31. Suggested branch: `ui/pause-menu`. This is the hardest of the first five, so it comes last.

**Goal:** Esc pauses the game, with Resume, Options and Quit To Menu.

**What to make:**
- First, turn `Options Panel` from `IntroScreen` into an `OptionsPanel` prefab, and check the intro screen still works. Do this before anything else, so both menus share one options page and Task 9 only adds sliders once.
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

## Task 6: Checkpoint Message

Card: #59 (the "checkpoints feel rewarding" part). Suggested branch: `ui/checkpoint-message`. Pick this up once #23 has checkpoints in.

**Goal:** a short "Checkpoint Reached" message when Nicholas touches a cotton candy machine, so reaching one feels rewarding.

**What to make:**
- A `CheckpointMessage` prefab in `HUD`, a small banner near the top centre that fades in and out over about 1.5 seconds.
- Ask whoever has #23 for a `CheckpointReached` event, or add one to their checkpoint script in your PR.

**Hints:**
- `RoundOverMessage` already shows text for a set time and hides it again. The same idea works here, with a fade using a `CanvasGroup`'s alpha.
- Only show it the first time each checkpoint is reached, not every time Nicholas walks past.
- It shouldn't cover the Round Over message. If both happen at once, Round Over wins.

**Checks:**
- [ ] It shows once per checkpoint, and fades out by itself
- [ ] Walking back over the same checkpoint doesn't show it again
- [ ] It doesn't block the player's view of the level for long
- [ ] It doesn't overlap the timer, lives or Round Over message

---

## Task 7: Tutorial Prompts

Card: #47 (this task is the UI side of it). Suggested branch: `ui/tutorial-prompts`. Pick this up once Level 1's layout (#45) exists, so you know where prompts go.

**Goal:** prompts that teach move, jump and look in Level 1, without stopping play.

**What to make:**
- A `TutorialPrompt` prefab that level builders place in the level: a trigger area plus the text to show, set in the Inspector, like "Press Space To Jump".
- A `PromptDisplay` in `HUD`, bottom centre, that shows whichever prompt the player is standing in and fades it when they leave.

**Hints:**
- This is the one place where the thing in the level talks to the HUD, instead of the player. The prompt can find `PromptDisplay` with `FindAnyObjectByType` when the player enters.
- Check the real keys in `PlayerMovement` and the cone script before writing the text, so the prompts match the controls.
- Keep each prompt to one short line. The level starts dark, so the text has to be readable on black.

**Checks:**
- [ ] Each prompt shows when the player walks into its area, and fades when they leave
- [ ] The player can keep moving while a prompt is up
- [ ] The text matches the actual controls
- [ ] A level builder can place a new prompt and change its text without touching code

---

## Task 8: Ending Screen

Card: #55. Suggested branch: `ui/ending-screen`. Do this after Task 4.

**Goal:** after the House of Mirrors, Nicholas reaches the food stalls and the game ends on a simple screen.

**What to make:**
- An `Ending` scene, last in the level order, with a title, a line or two about Nicholas finally getting food, and a Back To Menu button.
- If it's simple to carry over, the total time and collectibles across all five levels.

**Hints:**
- It's a menu scene, so start from how `IntroScreen` is set up rather than from `HUD`.
- To carry totals between scenes, a small static class that each level adds to on completion is enough. Reset it when Play is pressed in the menu.
- No cutscenes, so a still background of the food stalls and text is the whole ending.

**Checks:**
- [ ] Finishing Level 5 loads the ending
- [ ] Back To Menu goes to the intro screen and starting again works
- [ ] Totals, if you add them, reset on a new game

---

## Task 9: Music And Effects Sliders

Card: #56 (the options side). Suggested branch: `ui/audio-sliders`. Pick this up once the mixer exists.

**Goal:** the options page controls music and effects separately, as well as the master volume it has now.

**What to make:**
- Music Volume and Effects Volume sliders on the `OptionsPanel` prefab from Task 5, saved through `GameSettings` like the existing master volume.

**Hints:**
- `OptionsMenu.SetMasterVolume` shows how a slider is saved and applied. The new ones set an exposed mixer parameter instead.
- Mixer volume is in decibels, so a 0 to 1 slider needs converting: `Mathf.Log10(value) * 20`. Keep the slider's minimum just above 0, because `Log10(0)` breaks.
- Because the options page is shared after Task 5, the pause menu gets the sliders for free.

**Checks:**
- [ ] Each slider changes only its own sounds
- [ ] Settings are kept after quitting and reopening the game
- [ ] The sliders work from both the intro screen and the pause menu

---

## Task 10: Design Pass

Card: #59. Suggested branch: `ui/design-pass`. Do this last, once the team's pixel art is settled.

**Goal:** make the UI look like it belongs to the same game as the levels.

**What to do:**
- Replace the eye sprite in `LivesDisplay` with the team's pixel art eye. That's only the sprite on the three eyes.
- Line up spacing and sizes across the HUD so the lives, timer and counter feel like one set.
- Add a little feedback where it helps, such as the eye blinking shut when a life is lost or the counter bumping when something is picked up. Use unscaled time so it still works around pausing.

**Checks:**
- [ ] Every screen uses the same font and colours
- [ ] The HUD is readable over the darkest and brightest parts of every level
- [ ] Pixel art UI images are crisp, not blurry, at 1920x1080 and 1280x720

---

## Only If There's Time

- **Level select (#64):** let players replay unlocked levels from the menu. Use the level order from #25, and save which levels are unlocked with `PlayerPrefs`.
- **Level name banner:** show the zone name, like "Big Top", for a couple of seconds when a level starts. It's not on the board, so add a card first if the team wants it.

## Before Submission

From #63: make sure every test scene and all test code is gone, `CREDITS.md` is complete, and the level template (#28) has `HUD` in it so every level gets the full UI.
