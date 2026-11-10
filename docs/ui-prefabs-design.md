# UI Prefabs And Lives Display

## Goal

Level builders should be able to drag the HUD into a scene and have it work, with no rebuilding or rewiring. The lives display, shown as eyes in the top left, comes first. The other UI elements follow the same pattern.

## How The UI Is Organised

Prefabs live in `Assets/_Project/Prefabs/UI/`.

`HUD.prefab` holds the canvas, with each element nested inside it as its own prefab. Builders drag in `HUD` once.

| Prefab | What it shows | Card |
|---|---|---|
| `LivesDisplay` | Eyes, top left | Done |
| `RoundOverMessage` | "Round Over" or "Game Over", centred | Done |
| `TimerDisplay`, `CollectibleCounter` | Hunger timer countdown and collectible count | #34, #26 |
| `PauseMenu` | Resume, options, quit | #31 |
| `GameOverScreen`, `LevelCompleteScreen` | Retry and next level buttons | #32 |
| `CheckpointMessage` | "Checkpoint Reached", top centre | #59 |
| `PromptDisplay` | Tutorial prompts, bottom centre | #47 |

The full task list is in [ui-tasks.md](ui-tasks.md).

The canvas uses Screen Space Overlay, scaling from 1920x1080, match 0.5, sort order 10.

### Changing The HUD For One Level

A level can move, restyle or turn off elements on its own copy, and can add new objects to it. Those changes stay in that scene as overrides. Edits to `HUD.prefab` reach every level, except for settings a level has overridden.

Don't use **Apply All** in the Overrides dropdown unless the change is meant for every level. The level template note in #28 should repeat this.

If several levels end up needing the same different layout, make a prefab variant then. We don't need one yet.

## Lives Display

```
HUD (Canvas prefab)
├─ LivesDisplay (nested prefab, anchored top left, 40px in)
│  ├─ Eye 1
│  ├─ Eye 2
│  └─ Eye 3
└─ RoundOverMessage (nested prefab, full screen, drawn on top)
   └─ Panel (hidden until needed)
      ├─ Title
      └─ Subtitle
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
- `LivesDisplay` only dims eyes. The end of round text is handled by `RoundOverMessage`.

## Round Over Message

Shown in the middle of the screen whenever the player loses a life.

| Lives left | Title | Subtitle |
|---|---|---|
| 2 | Round Over | 2 Lives Remaining |
| 1 | Round Over | 1 Life Remaining |
| 0 | Game Over | none |

- "Round Over" hides after 2 seconds (**Show Seconds** on the component). Losing another life restarts the 2 seconds.
- "Game Over" stays until lives are restored. Retry and Quit To Menu come in #32.
- The timing uses unscaled time, so the message still goes away if the game is paused.
- Like `LivesDisplay`, it finds `PlayerLives` by itself and logs a clear error if there isn't one.
- The title is red, size 156, and the subtitle is white, size 62, both in `Jersey10 SDF`.

## Not Included

- Real damage (#24)
- Timer and collectible count (#34)
- Pixel art for the eye

## Testing

Everything is tested by hand in the Unity Editor. There are no automated tests or test keys for the HUD.

For each element:

1. Build it as a prefab in `Assets/_Project/Prefabs/UI/` and add it to `HUD` if it belongs there.
2. Drop `HUD` and `Player` into a scene, without saving shared scenes, and work through the element's checks in Play mode.
3. Check the prefab works in someone else's scene with nothing to wire up.
4. Remove any temporary test scenes or test code before the PR merges.

Until real damage arrives in #24, nothing in the game takes a life. To check the lives display or Round Over again before then, add a temporary test key on your own branch and remove it before opening the PR.
