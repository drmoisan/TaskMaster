# P2-T14 — Final toolchain clean-pass declaration, remediation cycle 1

Timestamp: 2026-09-02T01-46

## Clause 1 — the five commands of the final pass, in order

### 1. Format apply (P2-T1)

- Timestamp: 2026-09-02T01-32
- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1575 files in 2042ms.` `git status --porcelain` taken immediately
  before and immediately after the command was **identical** on this pass, so the command
  rewrote no path. Because CSharpier prints a processed-file count rather than a
  rewritten-file count, and exits 0 either way, the before-and-after tree observation is what
  distinguishes a clean run from a repairing one.
  Detail: `evidence/qa-gates/remediation-csharpier-format.md`.

### 2. Format verify (P2-T2)

- Timestamp: 2026-09-02T01-32
- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0
- Output Summary: `Checked 1575 files in 4937ms.` No file was reported as needing formatting;
  the reported set is empty. This is a read-only command whose exit code is a real signal.
  Detail: `evidence/qa-gates/remediation-csharpier-check.md`.

### 3. Analyzer build (P2-T3)

- Timestamp: 2026-09-02T01-33
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: `5 Warning(s)`, `0 Error(s)`. The warning count equals the
  `R_BASELINE_ANALYZER_SUMMARY` count of 5 and all five are the same uncoded System.Reactive
  `packages.config` notices; no coded diagnostic of any kind was emitted and no warning is
  new. `CoreCompile:` ran **57** times, so the gate was not vacuous.
  Detail: `evidence/qa-gates/remediation-analyzer-build.md`.

### 4. Nullable build (P2-T4)

- Timestamp: 2026-09-02T01-33
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: `5 Warning(s)`, `0 Error(s)`. No `CS86` diagnostic was reported, matching the
  empty P0-T7 baseline enumeration. `CoreCompile:` ran **72** times.
  Detail: `evidence/qa-gates/remediation-nullable-build.md`.

### 5. MSTest run with coverage (P2-T5)

- Timestamp: 2026-09-02T01-35
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` `Total tests: 6949`, `Passed: 6949`, `Failed: 0`,
  `Skipped: 0`. The run printed the literal `Done. Coverage artifact:`, so the coverage
  document on disk is post-processed. Post-change repository-wide line coverage **85.40%**,
  branch coverage **79.45%**. A second, scoped run in the same task confirmed all twelve named
  tests as passed, 12 discovered and 0 failed.
  Detail: `evidence/qa-gates/remediation-mstest-coverage-run.md`.

These five cover the four gates — format verification, analyzer build, nullable build and the
MSTest run — plus the format-apply step that precedes them.

## Clause 2 — all five ran in the same uninterrupted pass, and P2-T1 left no net change

All five commands above ran in one uninterrupted pass, in the order shown, with no
intervening edit to any file under `QuickFiler/` or `QuickFiler.Test/`.

**P2-T1 left no net change under `QuickFiler/` or `QuickFiler.Test/` during that pass**: its
`git status --porcelain` before and after were compared with `diff` and were identical, so it
rewrote no path at all on the pass that counts.

**Paths P2-T1 rewrote outside the two prefixes and then restored: none.** P2-T1 rewrote no
path outside `QuickFiler/` and `QuickFiler.Test/` on either of its passes, so no
`git checkout 807fb0bb6e5e49f43efa6b256b05960bf078ca19 --` restoration was issued for any
path, and the list this clause asks for is empty.

## Clause 3 — loop restarts

**Number of restarts: 1.**

| Restart | Trigger | Detail |
|---|---|---|
| 1 | P2-T1 pass 1 rewrote two files under the permitted prefixes | CSharpier reflowed `QuickFiler/Controllers/QfcHomeController.cs` (the `ReconcileCarriersToItems(batch.Items, batch.PreScored)` call collapsed onto one line) and `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` (one `.Returns(...)` collapsed, one `.ContainSingle(...)` expanded). Both rewrites are cosmetic reflow of this cycle's own new code and change no token. The Phase 2 loop rule requires a restart from P2-T1 whenever a step changes a file under those two prefixes, so the loop restarted and pass 2 of P2-T1 was run immediately; it rewrote nothing, and P2-T2 through P2-T5 then ran to completion without any further change. |

No step of the final pass failed, so no restart was triggered by a failure.

A later task, P2-T13, rewrote the `Timestamp:` line of 22 artifacts under `evidence/`. That
does not trigger the restart rule, which is scoped to `QuickFiler/` and `QuickFiler.Test/`;
no source file, project file or test file was touched after the pass completed.

## Clause 4 — the four remediation items, their closing evidence and their pinning gate

| Item | Closing evidence | Named test or token gate that pins it |
|---|---|---|
| **R1** — leg A displayed the pre-unhook carrier set | `evidence/regression-testing/r1-green.md`, `evidence/other/r1-reconciliation.md` | Test `RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary` (red at P1-T2 on a stage-two assertion, green at P1-T5). Tokens: `#678 R1` exactly once in `QfcHomeController.cs`, `#678 R1` exactly once in `QfcDatamodel.QueueProcessing.cs`, `#678 R1a` and `#678 R1b` exactly once each in `QfcQueue.Enqueue.cs`, `ReferenceEquals` present in `QfcHighConfidencePreFilter.cs`, and `describe one dequeue rather than two` at zero occurrences. |
| **R2** — `ProjectPredeterminedFolder` did not mirror `ProjectSuggestionPath` | `evidence/regression-testing/r2-r3-green.md`, `evidence/other/r2-projection-alignment.md`, `evidence/other/r2-decision.md` | Tests `AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder` and `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` (both red at P1-T7, green at P1-T10). Tokens: `#678 R2` exactly once and `A null or empty archive root` at zero occurrences in `QfcItemController.FolderHandling.cs`. |
| **R3** — adoption path did not observe the cancellation token | `evidence/regression-testing/r2-r3-green.md`, `evidence/other/r3-cancellation-observation.md` | Test `LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation` (red at P1-T7 with "no exception was thrown", green at P1-T10). Tokens: `#678 R3` exactly once and `cancel.ThrowIfCancellationRequested();` present in `QfcItemController.FolderHandling.cs`. |
| **R4** — evidence timestamps were not real clock values | `evidence/other/r4-timestamp-correction.md`, `evidence/qa-gates/remediation-timestamp-fidelity.md` | Not a behaviour change, so no test pins it. The gate is the P1-T13 anchored diff: 17 added and 17 removed lines, every one a `Timestamp:` line, across 12 files, with no other field and no other file touched. P2-T13 is the forward-looking half and records a plan defect in its own re-measurement clause. |

## Output Summary

Five commands, one uninterrupted pass, all EXIT_CODE 0: format apply, format verify, analyzer
build (5 warnings / 0 errors, 57 `CoreCompile:`), nullable build (0 `CS86`, 72 `CoreCompile:`)
and the MSTest coverage run (6949/6949 passed, 85.40% line, 79.45% branch). One loop restart,
caused by cosmetic CSharpier reflow of two of this cycle's own files on the first format pass.
P2-T1 rewrote nothing on the final pass and restored no path, because it rewrote none outside
the permitted prefixes. All four remediation items closed, each with its evidence path and its
pinning test or token gate named.
