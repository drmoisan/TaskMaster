# [P0-T16] Baseline line counts of the pre-existing Write Set source files

Timestamp: 2026-09-08T00-49

Command: `git ls-files -- <the seven paths>` followed by `(Get-Content -LiteralPath <path>).Count` for each of them.

EXIT_CODE: 0

## Line-count idiom (reproduced by name)

A file's line count is `(Get-Content -LiteralPath <path>).Count`, which counts physical lines. The idiom `Get-Content -LiteralPath <path> | Measure-Object -Line` is **rejected by name and was not substituted**: `Measure-Object -Line` splits its input with `RemoveEmptyEntries` and therefore counts non-blank lines only, understating a file by exactly its blank-line count. The 500-line limit in `.claude/rules/general-code-change.md` and the 495-line ceiling in [P2-T9] are physical-line limits.

All seven paths were confirmed tracked by `git ls-files`, which listed all seven.

## Output Summary

BASELINE_LINES UtilitiesCS/Threading/UiThread.cs 195
BASELINE_LINES UtilitiesCS/Threading/SyncContextForm.cs 50
BASELINE_LINES UtilitiesCS.Test/Threading/UiThread_Tests.cs 215
BASELINE_LINES UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs 1066
BASELINE_LINES UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs 156
BASELINE_LINES UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs 272
BASELINE_LINES QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs 360

## Divergence check

The three counts the plan states are observed exactly: `UtilitiesCS/Threading/UiThread.cs` 195, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` 215, and `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` 360. No `BASELINE_LINE_COUNT_DIVERGENCE:` line is recorded, because no observed count differs from the value the plan states.

Consequences that follow from the observed values:

- The [P2-T8] budget of 275 added lines is measured against the 215-line `UtilitiesCS.Test/Threading/UiThread_Tests.cs` baseline recorded here. 275 added lines on that baseline finish at 490, five lines under the 495-line ceiling [P2-T9] gates. The baseline did not diverge, so the permitted added-line count does not move.
- The [P2-T1] figure of 460 is a whole-file ceiling for `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, a file this delivery creates. No baseline is subtracted from it and it is not among the seven paths measured here.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` measures 1066 lines, matching the figure [P4-T5] quotes. It already exceeds the 500-line limit before this delivery touches it, and the only change this delivery makes to it is the single attribute line [P2-T7] adds, so [P4-T5] will classify it under `PRE_EXISTING_FILES_OVER_500:` rather than under `FILES_OVER_500_INTRODUCED:`.
