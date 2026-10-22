# Phase 0 — Coverage-Bearing Test Baseline

Timestamp: 2026-09-13T23-15

Build lock: ACQUIRED 879 at 2026-09-13T23:14:28, RELEASED by 879 at 2026-09-13T23:15:33.

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

`-CoverageOutput` was left at its default, `coverage\coverage.cobertura.xml`. The runner
deletes the raw collector document unless its parent directory is exactly the repository
`coverage` directory, so the default is load-bearing and was not changed.

EXIT_CODE: 1

ExpectedExitCode: 1

Cobertura Document State: RAW-COLLECTOR-OUTPUT

Output Summary:

```
Discovered 9 test assemblies.
line-rate      = 0.7125753506415995
lines-valid    = 83775
lines-covered  = 59696
branch-rate    = 0.5996239833244779
test total     = 7256
test failed    = 2
test passed    = 7254
```

Non-zero-exit path observed: path (a). `Invoke-MSTestWithCoverage.ps1` line 262 throws on a
non-zero collection exit code, and the collection exit code was non-zero because two tests
failed. That throw happens before post-processing at lines 383-384, so the document left at
`coverage/coverage.cobertura.xml` is the raw collector output, carrying absolute paths and
third-party packages. This is confirmed independently of the exit path: a fixed-string search
of the document for the literal `<sources>`, which post-processing injects and the raw
document does not carry, returned 0 hits. The repository-wide line-rate assertion at line 386
never ran, so the recorded exit code is attributable to the failing tests rather than to a
threshold breach.

Consequence for `[P5-T10]`: the denominator recorded here (`lines-valid = 83775`) is the raw
denominator, which includes third-party and non-first-party modules. A post-processed figure
is not comparable to it directly. `[P5-T10]` must either reproduce this raw counting method
or state explicitly which denominator each side of its comparison uses.

## Baseline Test Failures (recorded, not repaired)

Two tests were already failing on this tree before any change from this plan was applied:

```
ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic OUTCOME=Failed
InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic OUTCOME=Failed
```

Both are in the QuickFiler breadcrumb area and neither is in this item's blast radius. They
are recorded as the baseline state so that a later run showing the same two failures is not
read as a regression introduced by this work.

## Shell-Icon Stall Risk (recorded per plan section R3)

The plan records, as an execution risk rather than an acceptance condition, that four
`UtilitiesCS.Test` shell-icon test classes have stalled `vstest.console.exe` inside
`SHGetFileInfo` on this machine. No stall occurred in this run: the runner completed in
approximately 65 seconds and produced both a TRX and a Cobertura document. No runner
parameter was added and no narrower discovery scope was substituted.

## Discovery Scope Observation

The runner reported `Discovered 9 test assemblies.` No sibling-worktree assembly under
`.claude/worktrees/` appeared in the discovered set; the worktree carries no such directory.

## Evidence Hygiene

Neither the raw Cobertura document nor the TRX is committed. Both live under `coverage/`,
which `.gitignore` line 144 excludes via `coverage/*`, confirmed with `git check-ignore -v`.
Only the projected figures above are retained in this artifact.

## Coverage Obligations

Recorded per `[P0-T9]`.

- Repository-wide line coverage floor per `CLAUDE.md`: `>= 80%` measured on the testable
  denominator, that is production-only first-party code after the ratified COM/VSTO/WinForms
  exemptions. The `0.7126` figure above is the raw all-module line rate and is not that
  denominator, so it is not a breach of the floor by itself and is recorded as a measurement
  rather than as a verdict.
- New modules target `>= 90%` line coverage.
- Changed lines must not regress.
- `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` does not yet exist, so its baseline
  per-file line coverage is recorded as `NOT PRESENT AT BASELINE`.
