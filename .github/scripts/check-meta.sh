#!/usr/bin/env bash
# Verifies every asset under Assets/ has a .meta file and every .meta file has an asset.
# A missing .meta means Unity will generate a new GUID on each machine and break references;
# an orphaned .meta is left behind when someone deletes an asset outside Unity.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

status=0
# Unity ignores hidden files/folders and anything ending in "~", so skip those.
while IFS= read -r path; do
  if [[ ! -e "$path.meta" ]]; then
    echo "::error file=$path::Missing .meta file for '$path'. Open the project in Unity so it generates one, then commit it."
    status=1
  fi
done < <(git ls-files --cached --others --exclude-standard Assets \
  | grep -v '\.meta$' \
  | grep -vE '(^|/)\.|~(/|$)' \
  | while IFS= read -r f; do
      [[ -e "$f" ]] || continue  # deleted in the working tree but still in the index
      echo "$f"
      d=$(dirname "$f")
      while [[ "$d" != "Assets" && "$d" != "." ]]; do echo "$d"; d=$(dirname "$d"); done
    done | sort -u)

while IFS= read -r meta; do
  [[ -e "$meta" ]] || continue
  asset="${meta%.meta}"
  if [[ ! -e "$asset" ]]; then
    echo "::error file=$meta::Orphaned '$meta' (its asset no longer exists). Delete it, or delete assets from inside Unity next time."
    status=1
  fi
done < <(git ls-files --cached --others --exclude-standard Assets | grep '\.meta$')

if [[ $status -eq 0 ]]; then
  echo "Meta files OK."
fi
exit $status
