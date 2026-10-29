#!/usr/bin/env bash
# One-time setup after cloning. Run from the repo root in Git Bash (Windows) or a terminal (macOS/Linux):
#   bash scripts/setup.sh
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

echo "→ Git LFS"
git lfs install --local
git lfs pull

echo "→ Git hooks (.githooks)"
git config core.hooksPath .githooks

echo "→ Safer defaults for this repo"
git config pull.rebase true
git config rebase.autoStash true
git config fetch.prune true
git config push.autoSetupRemote true

echo "→ Unity Smart Merge for scenes and prefabs"
version=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt | tr -d '\r')
candidates=(
  "/c/Program Files/Unity/Hub/Editor/$version/Editor/Data/Tools/UnityYAMLMerge.exe"
  "C:/Program Files/Unity/Hub/Editor/$version/Editor/Data/Tools/UnityYAMLMerge.exe"
  "/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/Tools/UnityYAMLMerge"
  "$HOME/Unity/Hub/Editor/$version/Editor/Data/Tools/UnityYAMLMerge"
)
merge_tool=""
for c in "${candidates[@]}"; do
  if [[ -f "$c" ]]; then merge_tool="$c"; break; fi
done
if [[ -n "$merge_tool" ]]; then
  git config merge.unityyamlmerge.name "Unity SmartMerge"
  git config merge.unityyamlmerge.driver "\"$merge_tool\" merge -h -p --force %O %B %A %A"
  git config merge.unityyamlmerge.recursive binary
  git config mergetool.unityyamlmerge.cmd "\"$merge_tool\" merge -p \"\$BASE\" \"\$REMOTE\" \"\$LOCAL\" \"\$MERGED\""
  git config mergetool.unityyamlmerge.trustExitCode false
  echo "  using $merge_tool"
else
  echo "  ⚠ Unity $version not found in the default Hub folder. Install it via Unity Hub, then re-run this script."
fi

echo "✔ Done. Open the project in Unity Hub (Add > select this folder) using Unity $version."
