# P5-T200 — Consolidated two-batch scope and anti-masking ledger (N1 + N2)

Timestamp: 2026-07-22T17-24Z

Command: `git status --porcelain; git diff --numstat -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs; git diff -- <both files> | grep '^-[^-]'; git diff --stat -- QuickFiler.Test/QuickFiler.Test.csproj coverage.config scripts/vscode/TaskMaster.cli.runsettings; sha256sum <both changed files, coverage.config, TaskMaster.cli.runsettings, QuickFiler.Test.csproj, the eight P5 production sources>; grep -nE 'Thread\.Sleep|Task\.Delay|DateTime\.(Now|UtcNow)|Stopwatch|DoNotParallelize|\[Ignore\]|TestCategory|SpinWait|while \(true\)' <both files>; grep -c '\[TestMethod\]' <both files>; grep -c 'DataRow' <both files>; wc -l <both files>`

EXIT_CODE: 0

## Consolidated proofs against the P5-T185 baseline

| Claim | Evidence |
|---|---|
| Exactly two files changed across N1 and N2, both tests | `git status --porcelain` lists only ` M QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` and ` M QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (plus the plan checklist file itself and the new evidence artifacts) |
| Zero production C# files changed; every P5-T185 production SHA-256 unchanged | No path under `QuickFiler/` appears in `git status`; the eight recorded production hashes are unchanged |
| Zero `QuickFiler.Test.csproj` include changes and zero new files | Project-file diff is empty; hash `06663711c83a1fe5de1b485d5b361db9edce43501e0c37a5af081dc0d0804fc7` is identical, so the 100-entry `Compile Include` inventory is hash-identical |
| `coverage.config` hash-identical | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` before and after |
| `scripts/vscode/TaskMaster.cli.runsettings` hash-identical | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` before and after |
| 17-class filter string unchanged | The filter recorded in P5-T185 is reused byte-identically at P5-T201; no narrowing and no extension |
| Both changed files at most 480 lines; every other P5 file within its existing bound | `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` = **341**; `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` = **480**; no other file was touched |
| No coverage or test exclusion added, widened, or moved; no threshold changed | No `[ExcludeFromCodeCoverage]`, `coverage.config`, runsettings, or threshold text was added or edited in either file or anywhere else |
| All 28 pre-existing cases present with every assertion unchanged in meaning; zero assertion removed, weakened, relaxed, or made conditional | N1 diff is `197 insertions / 0 deletions`. N2 diff is `261 insertions / 1 deletion`, and the single deleted line is the `using System;` directive that the expanded using block replaced — the unified diff contains no other `-` line, so no pre-existing case or assertion text was deleted or altered |
| Exactly ten cases added, five per batch | `[TestMethod]` counts: N1 partial 5 → 10, N2 partial 13 → 18; class totals moved 10 → 15 and 18 → 23 |
| All added cases are non-data-row | `grep -c 'DataRow'` = **0** in both files |
| No masking constructs in either changed file | The banned-pattern grep (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Stopwatch`, `[DoNotParallelize]`, `[Ignore]`, `TestCategory`, `SpinWait`, `while (true)`) returned no match in either file |

Post-correction hashes of the two changed files:

| File | SHA-256 | Lines |
|---|---|---:|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | `6ec48542768e3d195e2b6b844349de40d8e100fffee78d24a29fda48d2032fb5` | 341 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | `594d96f2a8f34e6e987d2ad7efeda6fce999152027924d83a15fc22b7f3e63db` | 480 |

## Ten added cases → nine allocated units → uncovered line numbers (each unit exactly once)

| Batch | New case | Allocated unit | Uncovered line(s) |
|---|---|---|---|
| N1 | `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched` | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 99 |
| N1 | `HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate` | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 118 |
| N1 | `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 122 |
| N1 | `Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost` | `BreadcrumbDropDownOpenCoordinator.Reset()` | 133 |
| N1 | `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary` | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 224-226 |
| N2 | `OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce` | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` (292-301) and `RetainCurrentSurface(...)` (324) | 292-301, 324 |
| N2 | `OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport` | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` (315 allocation) | 315 |
| N2 | `OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary` | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` (310-313 allocation) | 310-313 |
| N2 | `OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` | `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 153-156 |
| N2 | `NativeClosedCallback_HostClosedBeforeDrain_PerformsNoLateCloseWork` | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 413 |

All nine P5-T185 units are accounted for exactly once. Every case asserts real behavior at its branch — zero
posted operations with unchanged host state, zero selector-predicate consultations, unfaulted `false` completion
with an unchanged cancel count, exactly-once disposal of the exact installed control host / control / messenger
with no messenger-ready publication, exactly-once secondary reporting with the primary preserved by reference,
a cleared stored open task, and zero late cancel/focus invocations.

## Output Summary

Two test files changed, zero production files, zero project includes, zero new files, zero package, runsettings,
`coverage.config`, threshold, filter, or exclusion changes. All 28 pre-existing cases and every pre-existing
assertion are intact (198 insertions and 0 content deletions in N1; 261 insertions and a single replaced `using`
directive in N2). Exactly ten non-data-row deterministic cases were added, five per batch, mapped one-to-one onto
the nine allocated units, and neither changed file contains any prohibited timing, skip, or exclusion construct.
No contradiction was found; the correction may proceed to P5-T201.
