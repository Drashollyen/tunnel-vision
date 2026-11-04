# Team update: what you need to do

This is for anyone who has already cloned the project and run the setup script. You don't need to clone again or re-run setup. It takes about 10 minutes. This file is temporary and will be removed later.

> [!WARNING]
> **Close Unity before you start.** Keep it closed until step 4.

## 1. Save your work first

If you have unfinished work, commit it to your own branch before doing anything else.

1. In GitHub Desktop, open the **Changes** tab.
2. Tick only the files you meant to change, then click **Commit**.
3. Click **Push origin**, or **Publish branch** if it hasn't been pushed before.

> [!CAUTION]
> **Don't delete your project folder or click Discard on work you want to keep.** If you aren't sure, ask the group first.

## 2. Install the .NET SDK

C# whitespace now fixes itself when you commit, so you no longer need to run the `dotnet format` command. This needs the .NET SDK.

Open Git Bash and run:

```bash
dotnet --version
```

If it shows a version starting with `10`, skip to step 3. If not, run this, then close and reopen Git Bash and GitHub Desktop:

```bash
winget install Microsoft.DotNet.SDK.10
```

## 3. Switch to `revamp`

`revamp` is now the main working branch and the default branch until Nov 19.

1. In GitHub Desktop, click **Fetch origin**.
2. Click **Current Branch** and choose `revamp`.
3. Click **Pull origin** if it appears.

Your hooks update by themselves when you switch.

## 4. Check it works

1. Open the project in Unity. The first import can take a while.
2. Open `Assets/_Project/Scenes/IntroScreen.unity` and press Play.
3. Press **Play** in the menu. It should load the cone test scene.
4. **Close Unity.** If GitHub Desktop now lists changed files you didn't mean to change, right-click them and choose **Discard changes**.

## 5. Start your next piece of work

Follow this every time:

1. **Close Unity.**
2. Switch to `revamp` and pull.
3. Create your own branch from `revamp`.
4. Check that **Current Branch** shows your new branch.
5. **Now open Unity.**

Full steps are in [CONTRIBUTING.md](CONTRIBUTING.md).

## If you already have a branch

> [!WARNING]
> **`feat/player-cone-vision`, `ui/game-ui`, `AI/enemies-1` and `AI/enemies-2` are gone from GitHub.** Their work is already in `revamp`, or they were empty. **Don't click Publish branch on them, and don't keep working on them.** Start a new branch from `revamp`.

| If you're working on | Do this |
|---|---|
| **Sound** | Do step 1 so your branch is on GitHub, then ask for help bringing `revamp` into it. Conflicts are likely if it's an old branch |
| **Enemy behaviour** | Create `feat/enemy-ai` from `revamp`, not `main`. The card is [#37](https://github.com/Drashollyen/tunnel-vision/issues/37) |
| **Player or cone** | Your work is merged. Start a new branch from `revamp` for anything new |
| **Anything else** | Close Unity, go to **Branch > Merge into current branch**, choose `origin/revamp`, and click **Create a merge commit** |

## What changed

**How we work**

- Tasks are on the [project board](https://github.com/orgs/Drashollyen/projects/1). **Assign yourself a card before you start.**
- **Close Unity before you switch branches.**
- **Only one person works in a scene at a time.**
- PRs go into `revamp`, and GitHub picks that for you. If the base branch says `main`, change it.
- Write `Closes #22` in your PR, using your card's number. The card closes when the PR merges.
- If your PR says it's out of date, click **Update branch** and wait for the checks to run again.
- **Don't merge your own PR.** kemuri reviews and merges it.
- Add a line to `CREDITS.md` whenever you add an asset: name, link, and licence if there is one.

**The game**

- 5 levels, one per zone, instead of 25.
- No cutscenes or easter eggs.
- One type of collectible, as a proof of concept.
- Pixel art. Nicholas's art isn't final, and we'll redo it as a team.
- Everything is due **Nov 19**. The full plan is in the [roadmap](https://github.com/Drashollyen/tunnel-vision/issues/66).

## Tell the group if something doesn't match

These steps haven't been tried on everyone's setup yet. If GitHub Desktop shows something different, or a step fails, tell the group so we can fix this guide.
