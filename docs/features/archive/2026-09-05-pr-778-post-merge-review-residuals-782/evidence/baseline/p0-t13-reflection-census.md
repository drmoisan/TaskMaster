# Baseline — Reflective `UiThread._dispatcher` Acquisition Census (P0-T13)

Timestamp: 2026-09-05T19-44

Command:

```powershell
$files = Get-ChildItem -Path . -Recurse -File -Filter '*.cs' |
    Where-Object { -not ($_.FullName.Contains('\obj\') -or $_.FullName.Contains('\bin\')) }
$files | ForEach-Object { Get-Content -LiteralPath $_.FullName | Select-String -SimpleMatch '"_dispatcher"' }
$files | ForEach-Object { Get-Content -LiteralPath $_.FullName | Select-String -SimpleMatch 'typeof(UiThread)' }
```

1614 source files were scanned. Build output under `\obj\` and `\bin\` is excluded by an exact
path-segment test rather than by a substring match; a substring test for `obj` matches the directory
name `OutlookObjects` case-insensitively and silently removes that whole subtree from the scan.

The conjunction `GetField("_dispatcher"` is deliberately **not** used as the search literal.
CSharpier wraps every acquisition so that `GetField(` and `"_dispatcher",` never share a line, and a
line-oriented search for the conjunction returns zero matches whatever the tree contains. The two
tokens are searched separately instead.

EXIT_CODE: 0

Output Summary:

## `"_dispatcher"` — exactly 6 lines

| File | Line | Matched text |
|---|---|---|
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 128 | `"_dispatcher",` |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 422 | `"_dispatcher",` |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 139 | `"_dispatcher",` |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 145 | `"_dispatcher",` |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 41 | `"_dispatcher",` |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 136 | `"_dispatcher",` |

All six file-and-line pairs match the acceptance condition exactly. **This is the before-figure for
the AC5 gate in P3-T10**, which requires the same search to return exactly two lines after the
migration.

## `typeof(UiThread)` — exactly 7 lines

| File | Line | Matched text |
|---|---|---|
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 127 | `return typeof(UiThread).GetField(` |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 421 | `var dispatcherField = typeof(UiThread).GetField(` |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 138 | `var dispatcherField = typeof(UiThread).GetField(` |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 144 | `return typeof(UiThread).GetField(` |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 40 | `typeof(UiThread).GetField(` |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 135 | `FieldInfo field = typeof(UiThread).GetField(` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | 469 | `var uiThreadType = typeof(UiThread);` |

The first six are the immediately preceding line of each of the six `"_dispatcher"` acquisitions, at
lines 127, 421, 138, 144, 40, and 135 respectively, matching the acceptance condition exactly.

The seventh, at `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` line 469, **targets
`"_uiSyncContext"`, not `"_dispatcher"`**, and is therefore outside the C12/C13 family. The
surrounding source confirms it: line 469 assigns `typeof(UiThread)` to a local, and line 470-473
call `GetField("_uiSyncContext", ...)` on that local. This delivery does not modify that file.
