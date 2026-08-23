# P5-T193 — Batch N1 scope and anti-masking ledger

Timestamp: 2026-07-22T17-07Z

Command: `git status --porcelain; git diff --numstat -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs; git diff -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs | grep -c '^-[^-]'; git diff --stat -- QuickFiler.Test/QuickFiler.Test.csproj coverage.config scripts/vscode/TaskMaster.cli.runsettings; grep -nE 'Thread\.Sleep|Task\.Delay|DateTime\.(Now|UtcNow)|Stopwatch|DoNotParallelize|\[Ignore\]|TestCategory|WaitOne\(|SpinWait|while \(true\)' QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs; grep -c '\[TestMethod\]' QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs; grep -c 'DataRow' QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs; wc -l QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`

EXIT_CODE: 0

## Proofs against the P5-T185 baseline

| Claim | Evidence |
|---|---|
| Exactly one file changed | `git status --porcelain` reports exactly one source modification: ` M QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` |
| Zero production C# files changed | No path under `QuickFiler/` appears in `git status`; all eight production SHA-256 values recorded in P5-T185 are unchanged |
| No `QuickFiler.Test.csproj` include changed | `git diff --stat` on the project file is empty; the 100-entry `Compile Include` inventory and file hash `06663711…` are unchanged |
| No package, runsettings, `coverage.config`, threshold, filter, or coverage/test exclusion changed | `git diff --stat` on `coverage.config` and `scripts/vscode/TaskMaster.cli.runsettings` is empty; hashes `b9cd8035…` and `98ef03a8…` are unchanged |
| File is at most 480 lines | Post-format physical line count is **341** |
| All ten pre-existing cases and every pre-existing assertion present with unchanged meaning | `git diff --numstat` reports `197 0` — **197 insertions and zero deletions**; the count of removed lines in the unified diff is `0`, so no pre-existing line was deleted, reordered, weakened, or made conditional |
| Exactly five cases added | The file now declares 10 `[TestMethod]` cases (5 pre-existing in this partial + 5 new); the class total moved from 10 to 15 as required |
| All cases are non-data-row | `grep -c 'DataRow'` = **0** |
| No masking constructs | The banned-pattern grep (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Stopwatch`, `[DoNotParallelize]`, `[Ignore]`, `TestCategory`, `WaitOne(`, `SpinWait`, `while (true)`) returned **no matches** (grep exit 1) |

## Added case → allocated unit → uncovered line mapping

| New case | Allocated unit | Uncovered line(s) closed |
|---|---|---|
| `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched` | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 99 |
| `HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate` | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 118 |
| `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 122 |
| `Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost` | `BreadcrumbDropDownOpenCoordinator.Reset()` | 133 |
| `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary` | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 224, 225, 226 |

Each case asserts behavior at the branch rather than merely executing the line: zero posted operations and an
unchanged host open state / close-reason list (99, 118, 133), zero selector-predicate consultations with the host
left in its exact pre-drain state and only the release operation still queued (122), and an unfaulted, uncanceled
`false` completion with `opening.Exception` null and an unchanged selector-cancel count while the sink observes the
open and rollback exceptions in order (224-226).

## Output Summary

Batch N1 changed exactly one file, `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`
(341 lines after `csharpier format`, 197 insertions, zero deletions). Zero production C# files, zero project
includes, zero packages, zero runsettings, zero `coverage.config`, zero thresholds, zero filters, and zero
coverage/test exclusions were changed. All ten pre-existing coordinator cases and every pre-existing assertion
survive verbatim, exactly five non-data-row deterministic cases were added, one per allocated unit, and none of the
touched content contains `Thread.Sleep`, `Task.Delay`, a wall-clock wait, a retry loop, a timing threshold,
`[DoNotParallelize]`, `[Ignore]`, or a category-based skip. No contradiction was found; the batch may proceed to
P5-T194.
