# Baseline — Test + Coverage (P0-T6)

- **Timestamp:** 2026-07-11T13-00
- **Command:** `dotnet-coverage collect -o baseline.cobertura.xml -f cobertura -- vstest.console.exe <8 bin/Debug *.Test.dll> /InIsolation /Logger:trx /TestCaseFilter:TestCategory!=LiveOutlook` (CI-faithful invocation from ci.yml; dotnet-coverage 18.5.2 wraps the vstest run to emit a Cobertura line-rate in one pass. `/EnableCodeCoverage` is omitted because dotnet-coverage performs the collection.)
- **EXIT_CODE:** 1 (22 pre-existing environmental test failures; see below)
- **Output Summary:**
  - **Total tests:** 5358 — **Passed:** 5336 — **Failed:** 22 — Total time 55.8s.
  - **Repo-wide raw line coverage:** 68.93% (`line-rate="0.6893051840769782"`, lines-covered 128086 / lines-valid 185819). Branch-rate 38.19%.
  - This raw figure includes vendored/third-party packages and untestable COM/VSTO/WinForms code; it is NOT the testable-denominator figure the 80% floor applies to. It is captured verbatim for an apples-to-apples baseline-vs-final delta (P5-T5).

## Test assemblies (8, matching CI filter)

QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test.

## Pre-existing failures (22, all environmental — unrelated to F5 / Swordfish)

All 22 failures are Deedle / email-DataFrame tests failing with `Failed loading language 'eng'` (an NLP/tokenizer language-model that does not load in this environment). None touch Swordfish collections or any F5 deletion target. Because this is the pre-change baseline, these failures are pre-existing and will recur identically in the post-change run (P5-T4), cancelling out in the delta. Failing tests:

DeedleDoodles, DfToListEntries_WithProjectCategories_ParsesProjectAndProgramNames, DropFirstN_DropsFirstNRows, Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame, Exclude_EmptyOtherFrame_ReturnsSameRowCount, Exclude_NonEmptyOtherFrame_RemovesMatchingRows, FilterToProjectIDs_WithNullAndMixedRowKeys_ReturnsOnlyFourCharacterRows, FromArray2D_EmailLikeArray_ReturnsExpectedRowCountAndColumnLayout, FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows, FromDefaultFolder_EmptyStores_ReturnsEmptyFrame, FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame, FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame, FromDefaultFolder_StoresWithOneStoreThatHasNoData_ReturnsEmptyFrame, GetColumnEid_WithStringValues_ReturnsOrdinalSeries, GetDuplicateEntriesByColumn_ReturnsDuplicateValues, GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields, GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame, GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform, InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop, InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing, InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker, PrintToLog_WithPopulatedFrame_LogsWithoutThrowing.

## Per-package line-rates at baseline (for first-party regression comparison at P5-T5)

| Package | line-rate |
|---|---|
| Swordfish.NET.General (VENDORED — F5 deletes) | 0.0761 |
| UtilitiesCS | 0.8832 |
| UtilitiesCS.Test | 0.9763 |
| QuickFiler | 0.7252 |
| QuickFiler.Test | 0.9467 |
| TaskMaster | 0.6743 |
| TaskMaster.Test | 0.9467 |
| ToDoModel | 0.5491 |
| ToDoModel.Test | 0.7998 |
| Tags | 0.9263 |
| Tags.Test | 0.9702 |
| TaskTree | 0.9548 |
| TaskTree.Test | 1.0000 |
| TaskVisualization | 0.8972 |
| TaskVisualization.Test | 0.9682 |
| VBFunctions | 1.0000 |
| VBFunctions.Test | 1.0000 |
| SVGControl (vendored) | 0.1622 |
| Deedle / FSharp.Core / others (third-party) | ~0 |

The vendored `Swordfish.NET.General` package (7.61% covered) is in the current denominator and is removed wholesale by F5 (WI-3). Removing it drops both its covered and uncovered lines, so the raw repo-wide rate is expected to rise slightly post-change; no surviving first-party production package loses coverage.
