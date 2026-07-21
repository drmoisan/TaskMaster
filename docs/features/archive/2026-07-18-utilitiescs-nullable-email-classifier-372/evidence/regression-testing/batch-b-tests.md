# Batch B — Test Run with Coverage

Timestamp: 2026-07-19T01-30

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/evidence/regression-testing/batch-b-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702. Passed: 5702. Failed: 0. Test Run Successful.
- Line coverage: 83.79% (line-rate 0.837859) vs baseline 0.837795 — no regression.
- Branch coverage: 76.34% (branch-rate 0.763446) vs baseline 0.763329 — no regression.
- No test regression on the corpus/count classes; changed-line coverage does not regress versus baseline (AC3, AC4).

Operational note: an initial run reported 1 failure — `TaskTree.Test.TaskTreeControllerMoveLogicTests.MoveObjectsToSibling_RootTarget_RemovesFromRootsAndReseeds` throwing `System.Configuration.ConfigurationErrorsException: The configuration file has been changed by another program` (per-user user.config, via `ToDoModel.IDList.GetNextToDoID` -> `ApplicationSettingsBase.Save()`). This was a test-infrastructure contention artifact caused by a detached leftover test pipeline (a `pwsh` Invoke-MSTest runner surviving a prior bash-tool timeout) writing the shared `user.config` concurrently. It is in an assembly unrelated to Batch B's Corpus changes. After killing the leftover runner and re-running on a verified clean slate, all 5702 tests passed. The failure is confirmed environmental, not a regression.
