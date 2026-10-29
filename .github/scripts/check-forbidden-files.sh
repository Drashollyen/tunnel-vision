#!/usr/bin/env bash
# Fails if generated/local-only folders or oversized non-LFS files were committed.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
status=0

if git ls-files | grep -qiE '^(Library|Temp|Obj|Logs|UserSettings|Build|Builds)/'; then
  echo "::error::Generated Unity folders are committed (Library/, Temp/, Logs/, UserSettings/, Build/...). Remove them with 'git rm -r --cached <folder>'."
  git ls-files | grep -iE '^(Library|Temp|Obj|Logs|UserSettings|Build|Builds)/' | head -20
  status=1
fi

# Binary files over 5 MB must go through Git LFS (see .gitattributes).
limit=$((5 * 1024 * 1024))
lfs_files=$(git lfs ls-files --name-only 2>/dev/null || true)
while IFS= read -r f; do
  [[ -f "$f" ]] || continue
  size=$(wc -c < "$f")
  if (( size > limit )) && ! grep -qxF "$f" <<< "$lfs_files"; then
    echo "::error file=$f::'$f' is $((size / 1024 / 1024)) MB and not tracked by Git LFS. Add its extension to .gitattributes."
    status=1
  fi
done < <(git ls-files)

if [[ $status -eq 0 ]]; then
  echo "No forbidden files."
fi
exit $status
