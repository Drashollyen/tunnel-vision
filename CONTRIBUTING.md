# Contributing

Set up your clone first: [README.md](README.md#setup).

> [!IMPORTANT]
> **The 5 rules**
>
> 1. **Assign yourself a card** before you start.
> 2. **Close Unity before you switch branches.**
> 3. **Work on your own branch, made from `revamp`.** Never work on `main` or `revamp`.
> 4. **Your PR goes into `revamp`, not `main`.** GitHub picks `main` by default, so change it.
> 5. **Only one person works in a scene at a time.**

Everyone works on their own branch. Finished work goes into `revamp`, the shared copy of the game. `revamp` goes into `main` at the end.

```
main ──────────────────────────────────────────► final merge (Nov 19)
  └─ revamp ──●─────────●─────────●────────────►
              ▲         ▲         ▲
     feat/hazards  feat/enemy-ai  audio/sound-effects
```

## 1. Pick or create a card

Every task is a card on the [project board](https://github.com/orgs/Drashollyen/projects/1). Each card is a GitHub issue, and cards aren't given out, so pick what you want to do.

**To pick a card:**

1. Open the card and read **Goal** and **Done when**.
2. Check **Assignees** on the right. **If someone is already assigned, pick another card.**
3. Click **assign yourself**.
4. On the board, drag the card to **In Progress**.

Labels show the type of work: `code`, `ui`, `level-design`, `prefabs`, `art`, `sound`, `setup`. Milestones M0 to M7 show the order, so start with the lowest one you can.

**If you can't finish a card**, remove yourself from **Assignees**, drag it back to **Todo**, and leave a comment saying how far you got.

**To add a new card:**

1. Go to [Issues > New issue](https://github.com/Drashollyen/tunnel-vision/issues/new/choose) and choose **Task / feature** or **Bug report**.
2. Fill it in, and write **Done when** so anyone can tell when it's finished.
3. On the right, add a label, a milestone, and the **Tunnel Vision Roadmap** project.

## 2. Set up your branch BEFORE opening Unity

> [!WARNING]
> **Close Unity first, every time you switch branches.** Switching with Unity open swaps files underneath it. You can lose unsaved work or end up with changes on the wrong branch.

Do this in GitHub Desktop:

1. **Close Unity.**
2. Click **Current Branch** and choose `revamp`.
3. Click **Fetch origin**, then **Pull origin** if it appears. You now have the latest game.
4. Click **Current Branch > New Branch** and enter a name.
5. If asked what to base it on, **choose `revamp`**. Click **Create Branch**.
6. **Check that Current Branch at the top shows your new branch.**
7. Now open the project in Unity.

Name branches `type/short-description`, for example `feat/pause-menu`, `fix/options-volume` or `art/player-sprites`.

Types are `feat`, `fix`, `art`, `audio`, `level`, `ui`, `refactor`, `test`, `docs`, `chore`, `ci` and `perf`.

> [!NOTE]
> **If Desktop asks about changed files when you switch**, you have uncommitted work. Choose **Leave my changes** and come back to commit them, or cancel and commit first.

## 3. Work in Unity

- **Save before you commit.** Press **Ctrl+S** for the scene, and use **File > Save Project** for prefabs and settings. Changes you haven't saved won't appear in GitHub Desktop.
- **Only one person in a scene at a time.** Tell the group which scene you're in.
- **Build with prefabs where you can**, so people can work without touching the same scene.

More detail is in [Unity rules](#unity-rules).

## 4. Commit

1. In GitHub Desktop, open the **Changes** tab.
2. **Tick only the files you meant to change.**
3. Right-click any scene, prefab or `ProjectSettings` file you didn't mean to change and choose **Discard changes**.
4. Write the summary as `type: what changed`, in lower case. For example: `feat: add pause menu`.
5. Click **Commit to your-branch**.

Commit often. C# whitespace is fixed for you when you commit, so you don't need to run a command. A commit may take a few extra seconds because of this.

If a commit is rejected, Desktop shows the reason. Fix what it says and try again.

## 5. Get the latest `revamp`

Do this before opening a PR, and whenever you need someone else's work. Your branch doesn't pick up other people's merged work by itself.

1. **Close Unity.**
2. In GitHub Desktop, click **Fetch origin**.
3. Go to **Branch > Merge into current branch**.
4. Choose **`origin/revamp`** and click **Create a merge commit**.
5. Open Unity and check the game still runs.

> [!WARNING]
> **Don't use Branch > Update from main.** That brings in `main`, not `revamp`.

On a conflict, Desktop lists the files. Scenes and prefabs are usually merged for you. If they aren't, see [Scene and prefab conflicts](#scene-and-prefab-conflicts).

## 6. Push and open a PR

1. Click **Push origin**, or **Publish branch** the first time.
2. Click **Preview Pull Request > Create pull request**.
3. **Change the base branch from `main` to `revamp`.** This is the `base:` dropdown at the top of the page.
4. Write the title as `type: what changed`. For example: `feat: add pause menu`.
5. Fill in the template and link your card, for example `Card: #22`. See [What to write in the PR](#what-to-write-in-the-pr).
6. Add screenshots or a video for anything visual.

> [!CAUTION]
> **Check the base branch says `revamp` before you click Create.** If it says `main`, change it. Your work would otherwise skip `revamp`.

### What to write in the PR

> [!IMPORTANT]
> **Replace the hint text in the template with your own words.** The `<!-- ... -->` lines are hidden on the PR page, but they still end up in the merge commit message.

- **Say what you actually tested.** "It works" doesn't help anyone. Say what you clicked and what happened.
- **Tick only what's true.** If a box doesn't apply, leave it unticked and say why.

A filled-in PR looks like this:

```markdown
## What does this change?

Adds the pause menu, opened with Esc. Card: #31

## How did you test it?

Paused during ConeTest, changed the volume in Options, then resumed. Quit to menu also works.

## Screenshots / video

(drag your screenshot in here)

## Checklist

- [x] The base branch at the top of this page is `revamp`, not `main`
- [x] PR title follows `type: description` (it becomes the commit message)
- [x] I got the latest `revamp` and checked the game still runs
- [x] Every new/moved asset has its `.meta` file committed
- [x] No unrelated changes (scenes/prefabs I didn't mean to touch, ProjectSettings noise)
- [ ] New third-party assets are listed in `CREDITS.md` (no new assets in this PR)
```

## 7. Merge

Wait for the checks to finish. **Don't merge while a check is red or still running.** A skipped check counts as passed.

Click **Squash and merge**. Your branch is deleted automatically.

> [!IMPORTANT]
> **Open the game after your PR merges.** If something broke, tell the group and fix it first.

## 8. Finish the card

**Close your card by hand and check it's in Done on the board.** `Closes #22` only closes cards on PRs into `main`, so it won't do this for PRs into `revamp`.

Then go back to [step 1](#1-pick-or-create-a-card).

## If something goes wrong

| Problem | What to do |
|---|---|
| I made changes on `revamp` or `main` | The commit is blocked, so nothing is lost. Create a new branch and choose **Bring my changes** |
| Unity changed files I didn't touch | Right-click them in **Changes** and choose **Discard changes** |
| My work seems to be gone | Check **Current Branch**. You may be on a different branch. Close Unity before switching back |
| `.meta` file error | Open Unity so it creates the file, then commit it with the asset |
| Lint failed on C# formatting | Install the .NET SDK, then in Git Bash run `dotnet format whitespace Assets/_Project --folder`, then commit |
| Images or sounds are missing | In Git Bash, run `git lfs pull` |
| PR is against `main` by mistake | On the PR page, click **Edit** next to the title and change the base to `revamp` |
| I'm stuck | Ask the group before forcing anything. **Never delete the project folder to fix a problem**, because your uncommitted work is in it |

## Command line

If you prefer commands, these replace the GitHub Desktop steps. **Close Unity before switching branches here too.**

**Start a branch**

```bash
git switch revamp
```

```bash
git pull
```

```bash
git switch -c feat/pause-menu
```

**Commit**

```bash
git status
```

```bash
git add <files>
```

```bash
git commit -m "feat: add pause menu"
```

Leave out scenes, prefabs or `ProjectSettings` you didn't mean to change: `git restore <file>`.

**Get the latest `revamp`**

```bash
git fetch origin
```

```bash
git merge origin/revamp
```

**Push**

```bash
git push
```

Then open the PR on GitHub, with `revamp` as the base branch.

## Git hooks

These run automatically when you commit or push:

| Hook | What it does |
|---|---|
| `pre-commit` | Blocks commits on `main` and `revamp`, and missing or orphaned `.meta` files. Fixes C# whitespace in the files you're committing |
| `commit-msg` | Blocks messages not in `type: description` format |
| `pre-push` | Blocks direct pushes to `main` and `revamp` |

CI runs the same `.meta` and formatting checks.

## Unity rules

- **Commit `.meta` files with their assets.**
- **Move, rename and delete assets inside Unity**, not in File Explorer.
- Put new work in `Assets/_Project/`. Don't edit `Assets/ThirdParty/`; copy files into `_Project` instead.
- Binary files (PNG, PSD, WAV, FBX, fonts) go through Git LFS via `.gitattributes`. Add new binary extensions there.
- Don't commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds or `.csproj`/`.sln` files.
- Don't upgrade the Unity version in `ProjectSettings/ProjectVersion.txt` outside its own PR.

### Pixel art

The game uses pixel art. Import sprites with these settings so they stay crisp and the same size:

- **Pixels Per Unit:** 16
- **Filter Mode:** Point (no filter)
- **Compression:** None

### Scene and prefab conflicts

`setup.sh` sets up Unity Smart Merge, which resolves most scene and prefab conflicts automatically. If one still conflicts, run `git mergetool`, or keep one side (`git checkout --ours <file>` or `--theirs`) and redo the other change in Unity.

## Credits

**If you add an asset, add it to [CREDITS.md](CREDITS.md) in the same PR.** It needs the name, a link, and the licence if there is one.

## Code

- Runtime code: `Assets/_Project/Scripts/Runtime`, namespace `TunnelVision.*`. Editor tools: `Scripts/Editor`.
- Formatting follows `.editorconfig`.
- Naming: `PascalCase` for types, methods and properties; `camelCase` for `[SerializeField] private` fields; `_camelCase` for other private fields.
- Tests: `Tests/EditMode` for plain C#, `Tests/PlayMode` for scene tests. Run them in **Window > General > Test Runner**.
