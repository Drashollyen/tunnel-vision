# Contributing to Tunnel Vision

`main` is always playable. Nobody pushes to it directly: every change goes through a branch and a pull request (PR), and must pass CI before it's merged. Reviews are optional but encouraged. If you haven't set up your clone yet, do the [Quick start](README.md#quick-start) first.

## The everyday loop

**1. Start from an up-to-date main**

```bash
git switch main
```

```bash
git pull
```

**2. Make a branch for one piece of work**

Name it `type/short-description`, for example `feat/pause-menu`, `fix/options-volume`, `art/player-sprites`, `level/tunnel-01`.

```bash
git switch -c feat/pause-menu
```

**3. Work in Unity, then commit small, focused steps**

```bash
git status
```

```bash
git add Assets/_Project/Scripts/Runtime/UI/PauseMenu.cs Assets/_Project/Scripts/Runtime/UI/PauseMenu.cs.meta
```

```bash
git commit -m "feat: add pause menu"
```

Commit messages use **`type: what changed`** (lower case, present tense). Types: `feat`, `fix`, `art`, `audio`, `level`, `ui`, `refactor`, `test`, `docs`, `chore`, `ci`, `perf`. The commit hook rejects anything else.

Before committing, read `git status`. If a scene, prefab or `ProjectSettings` file shows up that you didn't mean to change, leave it out: `git restore <file>`.

**4. Before pushing, catch up with main**

Other people merge while you work. Rebase replays your commits on top of the newest `main`, so history stays a straight line:

```bash
git fetch origin
```

```bash
git rebase origin/main
```

If there's a conflict, Git stops and lists the files. Fix them (for `.unity`/`.prefab` files Smart Merge usually does it for you, see below), then `git add <file>` and `git rebase --continue`. To back out completely: `git rebase --abort`.

**5. Push and open a PR**

```bash
git push --force-with-lease
```

(The first push of a new branch is just `git push`. `--force-with-lease` is only needed after a rebase, and it refuses to overwrite anything someone else pushed.)

Open the PR on GitHub and fill in the template. Asking a groupmate to review is optional but recommended, especially for scenes, prefabs and anything that changes how the game plays. The PR title becomes the single commit on `main`, so give it the same `type: description` format.

**6. Get it green and merged**

The PR can only merge when:

- **Lint**, **Unity tests**, **Build Windows** and **PR title** checks pass (Unity tests and Build Windows show as skipped when your change doesn't need them; that counts as passing)
- it's up to date with `main` (if GitHub shows *"This branch is out-of-date"*, repeat step 4)
- all review comments (if anyone left some) are resolved

Then you (the author) click **Squash and merge**. The branch is deleted automatically. Go back to step 1 for your next task.

## What the hooks stop you from doing

`scripts/setup.sh` installs these in `.githooks/`:

| Hook | Blocks |
|---|---|
| `pre-commit` | committing on `main`; assets without `.meta` files (or orphaned `.meta`s); badly formatted C# |
| `commit-msg` | messages that aren't `type: description` |
| `pre-push` | pushing to `main`; pushing a branch that is behind `main` |

CI runs the same checks, so skipping hooks (`--no-verify`) only means you find out later.

## Unity rules that save hours

- **Always commit `.meta` files** together with their asset. They hold the GUID that every reference points to. A missing `.meta` breaks references on everyone else's machine.
- **Move, rename and delete assets inside Unity**, not in Explorer/Finder, so the `.meta` moves with them.
- **One person per scene at a time.** Two people editing the same `.unity` file is the most common source of painful conflicts. Say in the group chat which scene you're in. Build reusable things as **prefabs** so people can work on them separately.
- **Our stuff goes in `Assets/_Project/`.** Don't edit files in `Assets/ThirdParty/`. Copy what you need into `_Project` first.
- **Big binary files** (PNG, PSD, WAV, FBX, fonts...) go through Git LFS automatically, based on `.gitattributes`. If you add a new kind of binary file, add its extension there.
- **Don't commit** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds, or `.csproj`/`.sln` files. `.gitignore` already handles this. If `git status` shows them anyway, ask before committing.
- **Unity version is fixed** at the one in `ProjectSettings/ProjectVersion.txt`. Don't accept Hub's offer to upgrade the project. Upgrades are their own PR, agreed with the group.

### Smart Merge for scenes and prefabs

`setup.sh` configures Unity's `UnityYAMLMerge` as the merge driver for `.unity`, `.prefab`, `.asset` and other Unity YAML files, so many scene conflicts resolve automatically during a rebase. If one still conflicts, run `git mergetool` on it, or keep one side completely (`git checkout --ours <file>` / `--theirs`) and redo the smaller change in Unity.

## Writing code

- C# lives in `Assets/_Project/Scripts/Runtime` (namespace `TunnelVision.*`). Editor tools go in `Scripts/Editor`.
- Formatting follows `.editorconfig`. Visual Studio and Rider pick it up automatically. To auto-fix from a terminal:

  ```bash
  dotnet format whitespace Assets/_Project --folder
  ```

- Naming: `PascalCase` for types, methods and properties; `camelCase` for `[SerializeField] private` fields; `_camelCase` for other private fields.
- Add tests for logic: EditMode tests in `Tests/EditMode` for plain C#, PlayMode tests in `Tests/PlayMode` for things that need a scene. Run them in **Window > General > Test Runner**.

## For maintainers

### Unity license secrets (needed once for CI)

GameCI needs a Unity license to run tests and builds. In GitHub: **Settings > Secrets and variables > Actions > New repository secret**:

| Secret | Value |
|---|---|
| `UNITY_LICENSE` | Full contents of `C:\ProgramData\Unity\Unity_lic.ulf` (Windows) after activating a Personal license in Unity Hub |
| `UNITY_EMAIL` | Email of the Unity account that owns that license |
| `UNITY_PASSWORD` | Password of that Unity account |

Use a dedicated Unity account for CI if you can. See the [GameCI activation docs](https://game.ci/docs/github/activation).

### Branch rules

`main` is protected by the ruleset in `.github/rulesets/main.json`: PRs only (no approval required, so authors merge their own PRs), conversations resolved, required checks (`Lint`, `Unity tests`, `Build Windows`, `PR title`) on an up-to-date branch, linear history, no force pushes or deletion. To re-apply it after editing the file:

```bash
gh api -X PUT repos/Drashollyen/tunnel-vision/rulesets/$(gh api repos/Drashollyen/tunnel-vision/rulesets --jq '.[] | select(.name=="main") | .id') --input .github/rulesets/main.json
```
