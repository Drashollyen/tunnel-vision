# Polish Plan

How we get from working screens to a polished game by Nov 19. Each phase ends with something you can play and check. Phases 1 to 4 are UI. Phase 5 is what's left outside the UI.

## Decisions

**Level UI stays in one `HUD` prefab, nested inside `LevelRig`.** Every level already gets it that way. Each element stays its own prefab inside `HUD`, so one can be reworked without touching the others.

**Everything animates with unscaled time,** so it still works while the game is paused.

**Every scene change goes through one fader,** so no script calls `SceneManager.LoadScene` directly.

## Phase 1: Transitions

These go first because every later screen uses them.

| # | Build | Done when |
|---|---|---|
| 1.1 | `SceneFader`: one object kept across scenes, with a full-screen black image. `SceneFader.Load(name)` fades out, loads, fades in. | Menu to wheel to level and back all fade, and clicks are blocked during a fade |
| 1.2 | Replace every `LoadScene` call with `SceneFader.Load`: `IntroScreenController`, `FerrisWheelSelect`, `LevelGoal` | No direct `LoadScene` left outside the fader |
| 1.3 | `ButtonHover`: scales a button up slightly and eases back. Added to every button the same way `InterfaceSounds` adds its trigger. | Hover feels smooth, also with keyboard selection, and disabled buttons don't react |
| 1.4 | `PanelSwitcher`: fades one panel out and the next in, using a `CanvasGroup`. Replaces the `SetActive` calls in `IntroScreenController`. | Main, Options and Info cross-fade, and the `IntroScreen` tests still pass |

## Phase 2: Finish The Level Select

| # | Build | Done when |
|---|---|---|
| 2.1 | Decide the wheel art (see below), and import or draw it | Credited, 16 pixels per unit, Point filtering |
| 2.2 | Replace the hub, spokes and cabin bodies with the art | Looks like the GDD sketch, with upright cabins |
| 2.3 | Add a fairground background and a Select Level title | Nothing placeholder left on screen |
| 2.4 | Locked cabin look: grey with a padlock icon | Locked levels read at a glance |
| 2.5 | Blocked feedback: on Enter, the cabin shakes, the disabled sound plays, and "Complete Level N First" fades in | Trying a locked level is obvious, and spamming Enter doesn't break it |
| 2.6 | Empty cabin message for levels that don't exist yet: "Coming Soon" | Never silent |
| 2.7 | Selected cabin highlight, with a gentle arrow bob | The selection is clear while the wheel is still |
| 2.8 | Key hints: arrows, Enter, Escape | A new player knows the controls |
| 2.9 | Point the Play button at the wheel, and decide where the tutorial goes | The game can be reached from the menu. `IntroScreenTests` updated |

**Commit and open the PR here.**

## Phase 3: Rework The HUD

| # | Build | Card | Done when |
|---|---|---|---|
| 3.1 | Choose one UI style: pixel panels, icons and a font | #59 | One test screen looks right |
| 3.2 | Lives: eye pops or blinks shut when lost, with a small shake | #59 | A lost life is noticeable without reading |
| 3.3 | Round Over: replace the large text with a short banner that fades | #59 | Readable, and doesn't hide the respawn |

## Phase 4: The Rest Of The Level UI

In build order. The detail for each is in [ui-tasks.md](ui-tasks.md).

| # | Element | Card | Depends on |
|---|---|---|---|
| 4.1 | Hunger timer, top right | #34, #26 | Team decision on what running out does |
| 4.2 | Pause menu with shared Options | #31 | Nothing |
| 4.3 | Game Over screen: Retry, Quit To Menu | #32 | Replaces the 2-second restart in `PlayerRespawn` |
| 4.4 | Level Complete screen: time, Next Level, wheel | #32 | Timer. `LevelGoal` shows this before returning to the wheel |
| 4.5 | Checkpoint Reached banner | #59 | Checkpoint event |
| 4.6 | Level name banner: "Big Top" at the start | None yet | Nothing |
| 4.7 | Tutorial prompts | #47 | Where the tutorial goes |
| 4.8 | Collectible counter | #34 | #27 |
| 4.9 | Ending screen | #55 | Level 5 |
| 4.10 | Music and effects sliders | #56 | Audio mixer |

## Phase 5: Outside The UI

The biggest risk to the deadline is here. These need owners now, alongside the phases above.

| Work | Card |
|---|---|
| Level 1: clown, moving platform, real art | #38, #35, #46 |
| Enemies: sword swallower, jack-in-the-box, balloon animal, circus ball | #37 to #42 |
| Levels 2 to 5 | #49 to #52 |
| Cone: soft edge and length | #21 |
| Sound and music | #56 to #58 |

## Wheel Art Options

| Option | Cost | Catch |
|---|---|---|
| Team draws a rim and one cabin | Free | Best fit: separate pieces let the cabins stay upright. Needs someone's time |
| [Simple Ferris Wheel (animated)](https://filipemarcal.itch.io/ferris-wheel-animated) | $1 | Licence isn't stated. If the cabins are part of the image, they'd tilt as it turns, so it would only work as decoration |
| [KR Amusement Park Tileset](https://kokororeflections.itch.io/kr-amusement-park-tileset-for-rpgs) | $19.99 | Has a Ferris wheel and rides in 32 and 48 px, but it's made for top-down RPGs, so it may not suit |

For the HUD, [Simple UI Pack](https://serenesnow.itch.io/simple-ui-pack) has hearts and icons but no panels. Kenney's pixel UI sets are the other candidate for panels and buttons.
