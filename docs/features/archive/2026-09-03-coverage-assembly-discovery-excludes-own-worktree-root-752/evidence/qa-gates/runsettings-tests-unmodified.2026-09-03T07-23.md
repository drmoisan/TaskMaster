# RunSettings Tests Unmodified ([P3-T7])

Timestamp: 2026-09-03T12-17

Command:
1. `git -C <repo-root> rev-parse HEAD:tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
2. `git -C <repo-root> status --porcelain -uall -- tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

EXIT_CODE: 0

## Output of command 1, verbatim

```
4b168b07967b692fdb0574aefd7a5734dfeb0d9c
```

## Output of command 2, verbatim

```
```

(zero lines)

Output Summary: The blob hash equals the `PHASE0 HEAD BLOB HASH` recorded by `[P0-T12]` in `evidence/baseline/runsettings-tests-blob-hash.2026-09-03T07-23.md`, `4b168b07967b692fdb0574aefd7a5734dfeb0d9c`, and the porcelain output is zero lines. The two observations are decidable in both directions: the hash would catch a committed rewrite of that file and the porcelain would catch an uncommitted one, so a formatter rewrite could not pass unnoticed in either state. `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is therefore byte-identical to the blob this branch started from. The comparison is against that recorded value and not against `origin/main:tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, whose copy carries an additional `It` block this branch does not have and would report a mismatch on a correctly preserved file.
