# Final repository-root coverage document check (P10-T6)

Timestamp: 2026-09-14T21-13

Purpose: confirm no Pester run in this delivery left a stray `coverage.xml` at the repository root. Such a file is matched by neither the repository ignore file nor the formatter ignore file, and CSharpier processes `*.xml`, so a stray root document would be a format-check liability.

## Glob listing

Tool: Glob
Pattern: `<repo-root>/coverage.xml`

As in P0-T16, the Glob tool matches on the file name rather than on the exact path, so it returns three pre-existing documents under archived feature folders:

```
<repo-root>\docs\features\archive\2026-06-08-hierarchical-lcppn-folder-prediction-177\evidence\baseline\2026-06-10T12-31\coverage.xml
<repo-root>\docs\features\archive\2026-06-08-hierarchical-lcppn-folder-prediction-177\evidence\qa-gates\2026-06-12T15-26\coverage.xml
<repo-root>\docs\features\archive\2026-07-03-quickfiler-navigation-key-collision-232\evidence\coverage\2026-07-03T16-58\coverage.xml
```

None of the three is at the repository root, all three predate this delivery, all three sit under `docs/features/archive/**/evidence/**` which the formatter ignore file excludes, and this delivery touched none of them. The set is identical to the one P0-T16 recorded, so no document was added to it.

## Direct existence check

Command: `pwsh -NoProfile -Command '<worktree prologue>; "RootCoverageXmlExists=" + (Test-Path -LiteralPath "<repo-root>/coverage.xml")'`
EXIT_CODE: 0
Output: `RootCoverageXmlExists=False`

## Verdict

**No repository-root `coverage.xml` exists.** No deletion was required, and the loop does not restart at P10-T1.

Every Pester invocation in this delivery set `CodeCoverage.OutputPath` explicitly, so the Pester default of `coverage.xml` relative to the working directory was never relied on. The paths used were `coverage/pester-coverage.xml` for the measurements of record, and two additional paths for the negative-path proofs, `coverage/pester-negative-coverage.xml` in P8-T3 and `coverage/pester-p8t4-coverage.xml` in P8-T4, both of which were deleted after use. All three lie inside the `coverage/` directory, which `.gitignore` ignores at its line 144 with a single negated exception for the directory keep-file at line 145.

Output Summary: no repository-root `coverage.xml` exists, confirmed by a direct `Test-Path` returning `False`. The name-based Glob returns the same three archived documents P0-T16 recorded and no others.
