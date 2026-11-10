# Tunnel Vision

2D Unity game for Windows. **Unity 6000.6.3f1**, Universal Render Pipeline (2D).

- **Tasks:** [project board](https://github.com/orgs/Drashollyen/projects/1)
- **Plan:** [roadmap issue](https://github.com/Drashollyen/tunnel-vision/issues/66)
- **How to work on the game:** [CONTRIBUTING.md](CONTRIBUTING.md)

> [!IMPORTANT]
> **Never open Unity until you are on your own branch.** Unity works on whichever branch is selected in GitHub Desktop. See [Set up your branch](CONTRIBUTING.md#2-set-up-your-branch-before-opening-unity).

## Setup

You only do this once.

**1. Install these**

- [Unity Hub](https://unity.com/download) with editor **6000.6.3f1**
- [Git for Windows](https://git-scm.com/download/win), which includes Git Bash and Git LFS
- [GitHub Desktop](https://desktop.github.com/), unless you prefer Git commands
- [.NET SDK 10](https://dotnet.microsoft.com/download), which fixes C# whitespace for you when you commit

**2. Clone the project**

In GitHub Desktop, go to **File > Clone repository > URL**, paste the address below and click **Clone**.

```
https://github.com/Drashollyen/tunnel-vision.git
```

Or in Git Bash:

```bash
git clone https://github.com/Drashollyen/tunnel-vision.git
```

`revamp` is the default branch until Nov 19, so that's the branch you start on.

**3. Run the setup script**

In File Explorer, right-click the `tunnel-vision` folder and choose **Show more options > Open Git Bash here**. Then run:

```bash
bash scripts/setup.sh
```

This downloads the large files, installs the checks that run when you commit, and sets up Unity Smart Merge.

> [!WARNING]
> **Skipping the setup script causes problems.** Without it, images and sounds can be missing, and merging scenes is much harder.

**4. Add the project to Unity Hub**

Go to **Unity Hub > Add > Add project from disk** and select `tunnel-vision`. **Don't open it yet.** Follow [Set up your branch](CONTRIBUTING.md#2-set-up-your-branch-before-opening-unity) first.

## Project layout

```
Assets/
  _Project/                 Game content. All new work goes here.
    Art/                    Sprites by subject: Player, Enemies, Environment, Tiles, UI
    Audio/                  Music and sound effects
    Fonts/                  TextMesh Pro font assets
    Physics/                Physics materials and player controls (Input)
    Prefabs/                Reusable objects: Player, Level, UI
    Scenes/Menus/           IntroScreen (first in build), LevelSelect
    Scenes/Levels/          Playable levels (start new ones from Level1_BigTop)
    Scenes/Dev/             Test scenes: ConeTest, Game
    Scripts/Runtime/        Game code (TunnelVision.Runtime), one folder per namespace
    Scripts/Editor/         Editor tools
    Tests/EditMode/         Unit tests
    Tests/PlayMode/         Scene tests
    Tiles/                  Tile assets and palettes
  ThirdParty/               Downloaded assets. Don't edit (see CREDITS.md)
  Settings/                 URP and Input System settings
  TextMesh Pro/             TMP resources
.github/                    CI workflows, templates, main ruleset
.githooks/                  Checks that run when you commit and push
```

## CI

These checks run on every PR:

| Check | What it does |
|---|---|
| **Lint** | `.meta` files, forbidden files, C# formatting. PRs into `main` must also be up to date with `main` |
| **Unity tests** | EditMode and PlayMode tests. Skipped for docs/GitHub-only changes |
| **Build Windows** | Runs only when `build.yml` changes. Skipped otherwise |
| **PR title** | `type: description` format |

Merges into `revamp` and `main` need these checks to pass and the branch to be up to date. Merges to `main` that change `Assets/`, `Packages/` or `ProjectSettings/` produce a Windows build. Download it from the run's **Artifacts** in the **Actions** tab.

## Credits

See [CREDITS.md](CREDITS.md).
