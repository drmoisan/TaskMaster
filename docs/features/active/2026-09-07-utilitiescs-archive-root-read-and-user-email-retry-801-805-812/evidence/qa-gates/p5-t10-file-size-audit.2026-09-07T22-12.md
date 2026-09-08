# Phase 5 — 500-Line File-Size Audit (P5-T10)

Timestamp: 2026-09-08T08-26

Command: `git diff --name-only origin/main...HEAD`

EXIT_CODE: 0

Command: `git status --porcelain --untracked-files=all`

EXIT_CODE: 0

Command: `pwsh -NoProfile -Command` measuring each audited path with `(@(Get-Content -LiteralPath <path>)).Count`

EXIT_CODE: 0

Output Summary:

The audited set is every `*.cs` path in the union of the two spans above. The name-listing diff is paired with the porcelain span because the diff enumerates committed changes only: `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` is modified by P5-T3 and still uncommitted at this point, so the diff alone would not measure it. Per the D14 amendment recorded in `spec.md`, the audit is executed over `*.cs` paths only; the two `.csproj` paths in the diff are build configuration and sit outside the cap's scope.

| Audited `*.cs` path | Lines | Cap | Within cap |
| --- | --- | --- | --- |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | 194 | 500 | yes |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs` | 394 | 500 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | 373 | 500 | yes |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` | 81 | 500 | yes |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 997 | 1002 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 305 | 500 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 184 | 500 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 399 | 500 | yes |

Every audited path except `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` has a line count less than or equal to 500. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` measures 997, which is less than or equal to its pre-change figure of 1002. That file was already roughly twice the 500-line cap before this change; AC6 requires only that this change not worsen it, and the count fell by 5.

`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs` measures 394 and did not require reduction, so no additional file was created and the Write Set was not enlarged.

Absence findings, both required by AC6:

- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` does **not** appear in the audited set. It appears in neither the three-dot diff nor the porcelain span.
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` does **not** appear in the audited set. It appears in neither span.

Note on the stale AC6 parenthetical. AC6 in `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md` carries a parenthetical figure of 1066 lines for `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`. That figure was measured before the merge of `origin/main` at `f63a2c4bc396ed43af2354a07891b8ef0bb205ed`, which brought commit `f7294d716313042112c291ef1a50408fa086e4c3` (PR #814). That commit added a single `[DoNotParallelize]` attribute line to the file, so it now measures 1067, for the reason P0-T13 records in full. AC6's operative condition over that file is its **absence from the audited set**, not its line count, so the stale parenthetical does not affect this check-off. No task of this plan modifies that file, and it is not in the Write Set.
