# Baseline Tests — CI Invocation Form (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

## Authoritative pass/fail — CI-equivalent `/EnableCodeCoverage` invocation

Command: `vstest.console.exe <all 7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
EXIT_CODE: 0

Output Summary: `Test Run Successful.` Total tests: 5141; Passed: 5141; Failed: 0.
Test assemblies: QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test,
UtilitiesCS.Test, VBFunctions.Test. Under the CI invocation `TaskMaster.Test` runs sequentially
(no `[assembly: Parallelize]`), so the shared-static race is not exposed on this path.

## Numeric coverage headline — reliable `dotnet-coverage collect` -> Cobertura path

Command: `dotnet-coverage collect --output <scratchpad>/baseline.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe <all 7 *.Test.dll> /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
EXIT_CODE: 0

Output Summary: Repository-wide (Cobertura root, all instrumented modules) line-rate **81.82%**
(lines-covered 121621 / lines-valid 148653); branch-rate 59.66% (14932 / 25028). All 5141 tests passed
on the collection run. This is the pre-fix baseline coverage headline used for the P2-T6 no-regression
delta. It is consistent with the cycle-1 figure (81.80-81.82%). The change under this cycle is
test-attribute-only, so no production line is added or removed.
