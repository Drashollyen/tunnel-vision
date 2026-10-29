# Tunnel Vision

A 2D Unity game made by a small group. Built with **Unity 6000.6.3f1** and the Universal Render Pipeline (2D).

![Main menu](Docs/Screenshots/main-menu-1920x1080.png)

## Quick start

You need:

- [Unity Hub](https://unity.com/download) with editor **6000.6.3f1** installed (Hub > Installs > Install Editor > Archive, or let Hub install it when you open the project)
- [Git for Windows](https://git-scm.com/download/win) (includes Git Bash and Git LFS) or Git + Git LFS on macOS/Linux
- Optional: the [.NET SDK](https://dotnet.microsoft.com/download) so the pre-commit hook can check C# formatting

Then, in Git Bash or a terminal:

```bash
git clone https://github.com/Drashollyen/tunnel-vision.git
```

```bash
cd tunnel-vision
```

```bash
bash scripts/setup.sh
```

`setup.sh` pulls the Git LFS files (art, audio, fonts), installs the repo's git hooks and configures Unity Smart Merge for scenes and prefabs. Run it once per clone.

Open the project: **Unity Hub > Add > Add project from disk >** select the `tunnel-vision` folder. The first open takes a few minutes while Unity builds its `Library` folder. Open `Assets/_Project/Scenes/MainMenu.unity` and press Play.

Read **[CONTRIBUTING.md](CONTRIBUTING.md)** before your first change. It explains branches, commits and pull requests.

## Project layout

```
Assets/
  _Project/                 Our game. Everything we make goes in here.
    Scenes/                 MainMenu (first scene in the build), Game (placeholder)
    Scripts/Runtime/        Game code (TunnelVision.Runtime assembly)
    Scripts/Editor/         Editor-only tools (TunnelVision.Editor assembly)
    Tests/EditMode/         Fast unit tests
    Tests/PlayMode/         Tests that load scenes and play them
  ThirdParty/
    Bloodlines UI/          UI kit from the Unity Asset Store (don't edit, see CREDITS.md)
  Settings/                 URP and Input System settings
  TextMesh Pro/             TMP essential resources
Docs/Screenshots/           Menu previews (regenerate with Tunnel Vision > Capture Menu Previews)
.github/                    CI workflows, PR/issue templates, main branch ruleset
.githooks/                  Local checks installed by scripts/setup.sh
```

## Main menu

`MainMenu` is generated from the Bloodlines UI prefabs by `Tunnel Vision > Rebuild Menu Scenes` (see `MainMenuSceneBuilder.cs`).

- **Play** loads the scene named in `MainMenuController > Game Scene Name` (default `Game`).
- **Options** has master volume, interface sound volume and on/off, and fullscreen. Settings are saved between sessions.
- **Quit** closes the game (or stops Play mode in the editor).

The panels are centered, scale with screen resolution (Canvas Scaler, reference 1920×1080), and resize to fit their contents (Vertical Layout Group + Content Size Fitter). You can add, remove or reorder buttons and the frame adjusts.

Once you start editing the menu by hand in the scene, stop using the rebuild menu item, because it overwrites the scene.

| Main menu | Options |
|---|---|
| ![Main menu at 1280x1024](Docs/Screenshots/main-menu-1280x1024.png) | ![Options at 1280x1024](Docs/Screenshots/options-1280x1024.png) |

## Continuous integration

Every pull request runs:

| Check | What it does |
|---|---|
| **Lint** | `.meta` files match assets, no `Library/`/`Temp/` or big non-LFS files, C# formatting matches `.editorconfig`, branch is up to date with `main` |
| **Unity tests** | EditMode and PlayMode tests via [GameCI](https://game.ci). Skipped (counts as passed) when a PR only changes docs or GitHub config |
| **Build Windows** | Only on PRs that change the build setup (`build.yml`, e.g. a Dependabot builder update), so a new builder is proven before merging. Skipped (counts as passed) on all other PRs |
| **PR title** | Title follows `type: description` (it becomes the commit message on `main`) |

Each merge to `main` that changes the game (`Assets/`, `Packages/`, `ProjectSettings/`) also produces a **Windows build**. Nothing is built twice: a build-setup change is tested on its PR and not rebuilt after merging. Download it from the run's *Artifacts* section under the **Actions** tab. You can also run the *Build* workflow by hand.

## Credits

See [CREDITS.md](CREDITS.md) for third-party assets and their licenses.
