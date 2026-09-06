# Baseline — Write Set Line Counts (P0-T8, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-59

## Why the earlier record is superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the record had to be
re-taken against the re-anchored base whether or not its figures moved.

The superseded record additionally used `(Get-Content -LiteralPath '<path>').Count`, which reads the
worktree. Two of the ten files now carry the Phase 1 edits, so a worktree read of them would be a
post-change figure and not a baseline. This re-record reads the content out of the `pre-782-base`
commit itself.

## Measurement method and measuring party

The four Phase 0 gate baselines re-recorded by P0-T3 through P0-T7 were measured by the
**orchestrator, not the executor**, at the re-anchored base commit `736c2cf2`, by the
temporary-restore method: the orchestrator restored the six Write Set source files Phase 1 has
changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

This task, P0-T8, is one of the two Phase 0 tasks that do run their own commands, because the counts
can be read out of the `pre-782-base` commit itself and therefore do not depend on the Phase 1
working tree. **The ten commands recorded below were run by the executor**, not by the orchestrator,
and the counts are the executor's own observations.

Command:

```powershell
@(git show 'pre-782-base:UtilitiesCS/Threading/UiThread.cs').Count
@(git show 'pre-782-base:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/Threading/UiThread_Tests.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs').Count
@(git show 'pre-782-base:UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs').Count
@(git show 'pre-782-base:QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs').Count
@(git show 'pre-782-base:QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs').Count
```

Each operand is quoted as a single argument. `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`
contains a space and would otherwise be split into two operands that do not resolve. The `@(...)`
array subexpression is required so the count is one element per line, matching what
`(Get-Content).Count` reports and keeping these figures comparable with the superseded ones.

EXIT_CODE: 0

Output Summary:

| File | Counting command | Baseline count |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | `@(git show 'pre-782-base:UtilitiesCS/Threading/UiThread.cs').Count` | 172 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | `@(git show 'pre-782-base:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs').Count` | 77 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/Threading/UiThread_Tests.cs').Count` | 179 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count` | 514 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs').Count` | 206 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs').Count` | 348 |
| `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs').Count` | 241 |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `@(git show 'pre-782-base:UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs').Count` | 201 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `@(git show 'pre-782-base:QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs').Count` | 320 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `@(git show 'pre-782-base:QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs').Count` | 393 |

**All ten counts are identical to the superseded record.** That is the expected outcome, not a sign
the re-record was skipped: the main advance changed none of the ten files, and the source tree at
`736c2cf2` is byte-identical to `origin/main` for every `*.cs` file. No deviation was observed, so
none is reported.

## Files deliberately outside this baseline

The three remaining production files in the Write Set — `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, and
`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` — are deliberately outside this baseline. The
edits P1-T6 and P1-T7 make to them are one-for-one line replacements that cannot change a line
count, so no size gate in Phases 2, 4, or 7 reads a baseline for them.
