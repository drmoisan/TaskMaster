# RunSettings Tests Preservation Baseline ([P0-T12])

Timestamp: 2026-09-03T11-57

Command:
1. `git -C <repo-root> merge-base origin/main HEAD`
2. `git -C <repo-root> rev-parse HEAD:tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

EXIT_CODE: 0

MERGE BASE SHA: 87233f867ad60c0a5c0d19b09cc121ae536d7ba1

PHASE0 HEAD BLOB HASH: 4b168b07967b692fdb0574aefd7a5734dfeb0d9c

Output Summary: The preservation comparison for `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is anchored to this branch's own starting blob, `4b168b07967b692fdb0574aefd7a5734dfeb0d9c`, rather than to `origin/main:tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, because `origin/main` has advanced past the merge base and its copy of that file carries an additional `It` block this branch does not have, so an `origin/main` blob hash could never match a correctly preserved file here. `[P3-T7]` compares against the `PHASE0 HEAD BLOB HASH` value above. The `MERGE BASE SHA` value above is character-identical to the one recorded by `[P0-T4]` in `evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md`, and it is the ref `[P4-T8]` and `[P4-T11]` anchor their diffs to.
