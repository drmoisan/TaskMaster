# [P4-T5] 500-line file-size audit across the Write Set source files

Timestamp: 2026-09-08T02-33

Command: `(Get-Content -LiteralPath <path>).Count`, the pinned line-count idiom, for each of the ten audited source files. The idiom `Get-Content -LiteralPath <path> | Measure-Object -Line` is rejected by name and was not substituted, because it counts non-blank lines only and would understate a file by its blank-line count.

Measured after the last formatter run of Phase 4, which is the second pass recorded in `p4-t2-format-and-builds.md`.

EXIT_CODE: 0

## Post-change counts, with the recorded baseline beside each pre-existing file

| Path | `POSTCHANGE_LINES` | `BASELINE_LINES` from [P0-T16] |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 293 | 195 |
| `UtilitiesCS/Threading/IUiCaptureSource.cs` | 50 | created by this delivery |
| `UtilitiesCS/Threading/SyncContextForm.cs` | 50 | 50 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 458 | 215 |
| `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | 460 | created by this delivery |
| `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` | 209 | created by this delivery |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | 394 | 360 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | 1067 | 1066 |
| `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs` | 157 | 156 |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs` | 273 | 272 |

Seven of the ten files pre-existed; the three without a baseline are the files this delivery creates.

## Classification

PRE_EXISTING_FILES_OVER_500: 1

The set is every audited file whose `BASELINE_LINES` value already exceeds 500. It contains exactly one member:

- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`, baseline 1066.

The figure quoted in the plan for that file is 1066 and the value [P0-T16] recorded is 1066, so the two agree and the classification is not disturbed. This delivery changes that file by exactly the single `[DoNotParallelize]` attribute line [P2-T7] adds, taking it from 1066 to 1067. It is over the limit both before and after, and the delivery increases the overrun by one line.

FILES_OVER_500_INTRODUCED: 0

No audited file whose post-change count exceeds 500 lies outside the pre-existing set. The largest file this delivery creates or grows below that set is `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` at 460 lines, 40 under the limit; `UtilitiesCS.Test/Threading/UiThread_Tests.cs` is 458, and `UtilitiesCS/Threading/UiThread.cs` is 293, 207 under the limit and comfortably inside the roughly 305 lines of headroom research R8 recorded.

The two `.csproj` files are excluded from this audit, because the 500-line limit governs production code, test code and reusable scripts, and a project file is none of those.

## Budget reconciliation

`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` measured 468 lines when this task first ran, 8 above the 460-line whole-file authoring budget [P2-T1] states. The overshoot came from the [P3-T6] correction, which replaced the ambient-apartment arrangement in two methods with a dedicated-MTA-thread arrangement. Documentation comments in that file were trimmed and the toolchain restarted; the file now measures exactly 460, so the authoring budget is met. The trim changed no executable statement and no assertion, and every acceptance token of [P2-T1] through [P2-T5] still returns its required count.

`UtilitiesCS.Test/Threading/UiThread_Tests.cs` finishes at 458 against the 215-line baseline, so 243 lines were added against the 275-line budget [P2-T8] states, and it sits 37 lines below the 495-line ceiling [P2-T9] gates.

## Outcome

`FILES_OVER_500_INTRODUCED:` is `0`, so no remediation-required outcome arises from this audit and no file outside the Write Set was created. The one pre-existing overrun is carried to [P6-T13] as a follow-up candidate; it is not a remediation-required outcome of this delivery.
