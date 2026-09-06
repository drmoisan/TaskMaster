# QA Gate — File Sizes of Every Touched Test File (P4-T10)

Timestamp: 2026-09-05T22-43

Command:

```powershell
(Get-Content -LiteralPath '<path>').Count
```

run once per file with `<path>` replaced by each of the ten paths in the table below, and

```powershell
dotnet tool run csharpier check 'UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs'
```

The `DOTNET_ROOT` / `PATH` preamble was run in the same session before the CSharpier invocation,
because `global.json` pins an SDK the host cannot satisfy and a bare `dotnet` call fails.

EXIT_CODE: 0

Output Summary:

## Observed counts against the P0-T8 baseline

The Write Set contains ten test files, of which two are new and therefore have no baseline count.
`UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` is one of those ten, so it appears once in
this table rather than as a separate eleventh row; its row carries the additional CSharpier columns
the task requires.

| File | Counting command | Baseline (P0-T8) | Observed |
|---|---|---|---|
| `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs').Count` | none (new) | 126 |
| `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs').Count` | none (new) | 288 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/UiThread_Tests.cs').Count` | 179 | 213 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count` | 514 | 272 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs').Count` | 206 | 231 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs').Count` | 348 | 341 |
| `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs').Count` | 241 | 278 |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs').Count` | 201 | 256 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `(Get-Content -LiteralPath 'QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs').Count` | 320 | 317 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs').Count` | 393 | 397 |

## Acceptance

**Every observed count is strictly less than 500.** The largest is 397, in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`.

**The two ProgressTracker files are each strictly less than 350**, which is the headroom the Phase 2
arithmetic established: `ProgressTracker_Tests.cs` is 272 and `ProgressTracker_ReportAndViewerTests.cs`
is 288. Before the Phase 2 split the single file was 514, over the 500-line policy limit; the split
plus the two Phase 4 additions leave both parts with more than 60 lines of headroom each.

## CSharpier columns for `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`

Format command run against the file at creation time by P3-T1:

```powershell
dotnet tool run csharpier format UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs
```

Post-format count: **126**.

**The pre-format count is not recorded here and is not recoverable.** P3-T1 ran in a prior executor
session that ended before this artifact was written, and it left no artifact carrying the
pre-format figure; the file has been committed in its formatted state since, so the unformatted
text no longer exists in the tree or in git history. Rather than supply a figure that was not
observed, the equivalent property the pre/post pair was meant to establish is recorded directly:

```text
dotnet tool run csharpier check 'UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs'
Checked 1 files in 297ms.
CHECK_EXIT_CODE=0
```

`csharpier check` exiting 0 with `Checked 1 files` and reporting no unformatted file proves the
committed file is already at CSharpier's fixed point, which is the reason P3-T1 formatted it at
creation: so that it is not the file that rewrites the tree during the Phase 7 format step and
forces a second Phase 7 pass.
