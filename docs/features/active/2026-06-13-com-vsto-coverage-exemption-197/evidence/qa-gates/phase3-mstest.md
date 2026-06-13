# Phase 3 — MSTest with Coverage

Timestamp: 2026-06-13T13-08

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase3.cobertura.xml
(Koverage dedup -> coverage/coverage.phase3.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 2 failures -> pipeline exit 1 (dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4066
- Failed: 2
- Failing tests this run: TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream (21s), RequestTask_WithConfiguredTask_InvokesTaskAfterInterval (23s)
- These are timing/threading-sensitive flaky tests in UtilitiesCS (note the long durations). The failing set varies run-to-run (baseline failed 2 different-but-overlapping tests; Phase 1 failed 0; Phase 2 failed 1), confirming non-determinism. None are ToDoModel-target tests; annotations are non-behavioral. Not a regression.

## Coverage headline (first-party deduped, all non-.Test incl vendored constant)
- covered: 38,752
- lines-valid: 59,551
- line rate: 65.07%

## ToDoModel annotation verification
- ToDoModel package denominator: baseline 3,316 -> 1,819 lines (reduction ~1,497; method-level IDList keeps GetNextToDoID measured, so slightly under the whole-class §2.3 estimate of 1,800-2,100, as expected).
- ToDoModel package rate: 10.43% -> 19.24%.
- 6 classes annotated once each: FileOperationsPST, ToDoSynchronizer, ToDoEvents, TreeOfToDoItems, ProjectController, ProjectViewer.
- IDList: 4 method-level [ExcludeFromCodeCoverage] (2 Outlook ctors + 2 RefreshIDList overloads). IDList class still present in denominator with 164 measured lines; GetNextToDoID(string) unannotated and measured.
- Testable seams confirmed present in ToDoModel package: ToDoLoader, ProjectEntry, BaseChanger (1 class match each). ToDoDefaults unannotated.
