# UI Prefabs And Lives Display

## Goal

Level builders should be able to drag the HUD into a scene and have it work, with no rebuilding or rewiring. The lives display, shown as eyes in the top left, comes first. The other UI elements follow the same pattern.

## How The UI Is Organised

Prefabs live in `Assets/_Project/Prefabs/UI/`.

`HUD.prefab` holds the canvas, with each element nested inside it as its own prefab. Builders drag in `HUD` once.

| Prefab | What it shows | Card |
|---|---|---|
| `LivesDisplay` | Eyes, top left | First |
| `TimerDisplay`, `CollectibleCounter` | Timer and collectible count | #34 |
| `PauseMenu` | Resume, options, quit | #31 |
| `GameOverScreen`, `LevelCompleteScreen` | End of level screens | #32 |

The canvas keeps the settings it has in `UITest`: Screen Space Overlay, scaling from 1920x1080, match 0.5, sort order 10.

### Changing The HUD For One Level

A level can move, restyle or turn off elements on its own copy, and can add new objects to it. Those changes stay in that scene as overrides. Edits to `HUD.prefab` reach every level, except for settings a level has overridden.

Don't use **Apply All** in the Overrides dropdown unless the change is meant for every level. The level template note in #28 should repeat this.

If several levels end up needing the same different layout, make a prefab variant then. We don't need one yet.

## Lives Display

```
HUD (Canvas prefab)
└─ LivesDisplay (nested prefab, anchored top left, 40px in)
   ├─ Eye 1
   ├─ Eye 2
   └─ Eye 3
```

### Art

The eyes keep the sprite they use now, `eyeball2processing3.png` from Code Inferno's Rotating Eyeball. It's CC0 and already in `CREDITS.md`. It isn't pixel art, so it will be replaced when the team's art arrives. That only means changing the sprite on the three eyes in `LivesDisplay`.

### Behaviour

- A lost life dims the eye, using the existing `aliveColor` and `lostColor`.
- The three eyes are placed in the prefab, so they're visible while editing.
- If the eye count doesn't match the player's `maxLives`, the display logs a warning.

### Code Changes

- `PlayerLives` is added to `Player.prefab`, so every level gets it.
- `PlayerLives` gets a public read-only `MaxLives`, for the warning above.
- `LivesDisplay` loses its `playerLives` Inspector slot and finds `PlayerLives` in the scene when it starts.
- If there's no `PlayerLives` in the scene, `LivesDisplay` logs a clear error and turns itself off. It doesn't crash.
- `LivesDisplay` no longer handles the end of game text, which now reads "Game Over" instead of "You Died". It comes back in #32 as part of `GameOverScreen`. Only test keys can reach zero lives until #24 is done, so players won't miss it.

### Not Included

- Real damage (#24)
- Timer and collectible count (#34)
- Pixel art for the eye

## Testing Each Element

Every HUD element goes through the same steps:

1. Build the element, along with a test scene and any test keys it needs.
2. Try it in Unity using its checklist.
3. Fix anything that fails and try again.
4. Once it passes, delete the test scene and test keys in the same PR, before it merges. Test code never reaches `revamp`.

Test scenes go in `Assets/_Project/Scenes/Test/` and aren't added to the build.

Automated tests in `Assets/_Project/Tests/` stay after the manual test code is removed. They aren't part of the game build, and CI runs them on every PR, so they catch anyone breaking the element later.

### Lives Test Setup

`LivesTest.unity` contains only a camera, `Player` and `HUD`, plus an object with `HudTestKeys`. `HudTestKeys` is changed to find `PlayerLives` by itself. K loses a life, and at zero lives it restores all of them.

### Lives Checklist

- [ ] Press Play: three bright eyes in the top left, and no errors in the Console
- [ ] Press K: one eye dims each press, from the first life lost to the last
- [ ] Press K at zero lives: all three eyes come back
- [ ] Change the Game view to other sizes (1920x1080, 1280x720, 16:10): the eyes stay top left at a sensible size
- [ ] Set `maxLives` to 2 on the player and press Play: a warning appears in the Console
- [ ] Disable `Player` and press Play: a clear error appears in the Console, and the game keeps running
- [ ] Move the lives display on the scene's HUD copy, then edit the eye colour in `HUD.prefab`: the scene keeps its position and gets the new colour

Undo the last three changes after trying them.

### Automated Test

`LivesDisplayTests` in `Tests/PlayMode` covers dimming, restoring, and the missing `PlayerLives` error.

### Cleanup After Lives Pass

- Delete `LivesTest.unity` and `HudTestKeys.cs`. This also ticks off the `HudTestKeys` item on #24.
- `UITest.unity` still uses `HudTestKeys` and isn't in the build. Delete it only once kemuri confirms nothing in it is still needed.

## Who Does What

- Scripts, the automated test and test keys are written in code.
- The prefabs and test scene are made by hand in the Unity Editor, following short written steps.

## Later Elements

Each element gets its own short section here before it's built, with its own test setup and checklist. `PauseMenu` will be the first to have buttons, so it will need an EventSystem in the scene. That gets decided in #31.
