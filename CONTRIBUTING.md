# Contributing

All changes go through a branch and a pull request. Nothing is pushed to `main` directly. Set up your clone first: [README.md](README.md#setup).

## Workflow

Same steps either way. Use [GitHub Desktop](#github-desktop) or the [command line](#command-line).

Branch names: `type/short-description`, e.g. `feat/pause-menu`, `fix/options-volume`, `art/player-sprites`.

Commit messages and PR titles: `type: what changed`, lower case. Types: `feat`, `fix`, `art`, `audio`, `level`, `ui`, `refactor`, `test`, `docs`, `chore`, `ci`, `perf`.

### GitHub Desktop

**1. Update main:** **Current Branch > main**, then **Fetch origin** and **Pull origin**.

**2. Create a branch:** **Current Branch > New Branch**, enter the name, base it on `main`, click **Create Branch**.

**3. Commit:** in the **Changes** tab, tick only the files you meant to change. Right-click any scene, prefab or `ProjectSettings` file you didn't mean to change > **Discard changes**. Write the summary as `feat: add pause menu`, then click **Commit to feat/pause-menu**.

**4. Update from main:** **Branch > Update from main**. On a conflict, Desktop lists the files: open each, fix it, then click **Continue merge**. Scenes and prefabs are usually merged for you (see [Scene and prefab conflicts](#scene-and-prefab-conflicts)).

**5. Push and open a PR:** click **Push origin** (or **Publish branch** the first time), then **Preview Pull Request > Create pull request**. Fill in the template on GitHub.

**6. Merge:** see [Merging](#merging).

If a commit or push is rejected, Desktop shows the hook's message. Fix what it says and try again.

### Command line

**1. Update main**

```bash
git switch main
```

```bash
git pull
```

**2. Create a branch**

```bash
git switch -c feat/pause-menu
```

**3. Commit**

```bash
git status
```

```bash
git add <files>
```

```bash
git commit -m "feat: add pause menu"
```

Check `git status` before committing. Leave out scenes, prefabs or `ProjectSettings` you didn't mean to change: `git restore <file>`.

**4. Rebase on main**

```bash
git fetch origin
```

```bash
git rebase origin/main
```

On a conflict: fix the files, `git add <file>`, then `git rebase --continue`. To cancel: `git rebase --abort`.

**5. Push and open a PR**

```bash
git push --force-with-lease
```

Open the PR on GitHub and fill in the template. The PR title becomes the commit on `main`, so use the `type: description` format.

**6. Merge:** see [Merging](#merging).

### Merging

The PR can merge when:

- **Lint**, **Unity tests**, **Build Windows** and **PR title** pass (skipped counts as passed)
- the branch is up to date with `main` (if not, repeat step 4, or click **Update branch** on the PR page)
- all review comments are resolved

Click **Squash and merge**. The branch is deleted automatically.

## Git hooks

| Hook | Blocks |
|---|---|
| `pre-commit` | commits on `main`, missing or orphaned `.meta` files, unformatted C# |
| `commit-msg` | messages not in `type: description` format |
| `pre-push` | pushes to `main`, branches behind `main` |

CI runs the same checks.

## Unity rules

- Commit `.meta` files with their assets.
- Move, rename and delete assets inside Unity, not in Explorer.
- Avoid editing the same scene as someone else at the same time. Use prefabs for reusable parts.
- Put new work in `Assets/_Project/`. Don't edit `Assets/ThirdParty/`; copy files into `_Project` instead.
- Binary files (PNG, PSD, WAV, FBX, fonts) go through Git LFS via `.gitattributes`. Add new binary extensions there.
- Don't commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds or `.csproj`/`.sln` files.
- Don't upgrade the Unity version in `ProjectSettings/ProjectVersion.txt` outside its own PR.

### Scene and prefab conflicts

`setup.sh` sets up Unity Smart Merge, which resolves most scene and prefab conflicts automatically. If one still conflicts, run `git mergetool`, or keep one side (`git checkout --ours <file>` or `--theirs`) and redo the other change in Unity.

## Code

- Runtime code: `Assets/_Project/Scripts/Runtime`, namespace `TunnelVision.*`. Editor tools: `Scripts/Editor`.
- Formatting follows `.editorconfig`. To fix formatting:

  ```bash
  dotnet format whitespace Assets/_Project --folder
  ```

- Naming: `PascalCase` for types, methods and properties; `camelCase` for `[SerializeField] private` fields; `_camelCase` for other private fields.
- Tests: `Tests/EditMode` for plain C#, `Tests/PlayMode` for scene tests. Run them in **Window > General > Test Runner**.
