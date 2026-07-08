# SubjectMapSco_Orchestration_Tests Class After Fix — Finding C no-regression (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~SubjectMapSco_Orchestration_Tests"`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary:
- Total tests: 7. Passed: 7. Failed: 0.
- All `SubjectMapSco_Orchestration_Tests` methods PASS: `QueryOlFolders_WhenSelectedRelativePathIsConfigured_ExcludesSelectedNode`, `QueryMailTuples_WhenFoldersContainMixedItems_ReturnsOnlyMailItems`, `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`, `RebuildEntries_WhenFolderRemapExists_UsesMappedFolderPath`, `RepopulateSubjectMapEntries_WhenMailSequenceProvided_RebuildsAndEncodesMap`, `RebuildAsync_CallbackBody_WhenArchiveContainsMailItems_PopulatesMap`, `ShowSummaryMetrics_WhenEntriesExist_PopulatesSummaryMetricsAndShowsViewer`.
- The per-item `progress.Report` change in `Consume<T>` did not regress `RebuildEntries_WhenFolderRemapExists_UsesMappedFolderPath` (which asserts a report with `Value == 100`) or `RepopulateSubjectMapEntries_...` (which routes through `Consume` via `RepopulateSubjectMapEntries`).
