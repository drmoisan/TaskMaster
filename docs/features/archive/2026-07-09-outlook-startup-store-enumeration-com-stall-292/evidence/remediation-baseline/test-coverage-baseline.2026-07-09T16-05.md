# Test + Coverage Baseline (issue #292, remediation cycle 1) — PRE-FIX

- Timestamp: 2026-07-09T16-05
- Task: [P0-T6]

## Authoritative pass/fail (CI-equivalent `/EnableCodeCoverage` invocation)

- Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 1

### Output Summary (pass/fail)

- `Test Run Failed.` Total tests: 5141; Passed: 5131; **Failed: 10**; total time 1.01 min.
- This reproduces CI run 29046195330 (`Total 5141 / Failed 10`) on the FIRST full-suite pass. The failure is the #292 process-global-static race and is the expected pre-fix signal.
- The 10 failing tests (all in `UtilitiesCS.Test`), extracted from the trx:
  1. `CurrentStoreContextTests.Begin_SetsCurrent_ReadableInsideScope`
  2. `CurrentStoreContextTests.Dispose_RestoresPreviousValue`
  3. `CurrentStoreContextTests.NestedScopes_RestoreInnerThenOuter`
  4. `CurrentStoreContextTests.SequentialScopes_EachRestoreToNull`
  5. `CurrentStoreContextTests.Begin_NormalizedInnerScope_RestoresRealOuterValue`
  6. `CurrentStoreContextTests.Begin_NormalizesUnavailableIdentity_ToNoContext (null)`
  7. `CurrentStoreContextTests.Begin_NormalizesUnavailableIdentity_ToNoContext ("")`
  8. `CurrentStoreContextTests.Begin_NormalizesUnavailableIdentity_ToNoContext ("   ")`
  9. `CurrentStoreContextTests.Begin_NormalizesUnavailableIdentity_ToNoContext ("<unavailable>")`
  10. `ThreadMonitorTests.EvaluatePoll_NoContext_CarriesNullIdentity`

## Coverage (reliable path: `dotnet-coverage collect` -> Cobertura)

- Command: `dotnet-coverage collect --output <scratchpad>/baseline.cov.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> <all 7 *.Test.dll> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 1 (this instrumented pass showed Total 5141 / Passed 5130 / **Failed 11** — the race count fluctuates by schedule; both the 10- and 11-failure observations confirm a real, schedule-dependent race, not a fixed set).
- Note: the `/EnableCodeCoverage` `.coverage` binary is not offline-convertible in this environment; the repo-standard reliable coverage path is `dotnet-coverage collect` producing Cobertura directly (consistent with the `Invoke-MSTestWithCoverage.ps1` helper). This same method is used for the final coverage (P3-T4/P3-T5) so the comparison is apples-to-apples.

### Output Summary (coverage headline)

- Repository-wide (Cobertura root, all instrumented modules incl. test assemblies) line-rate: **81.80%** (lines-covered 121602 / lines-valid 148653); branch-rate 59.66%.
- Per production package line-rate (full CI-equivalent 7-assembly set):
  - UtilitiesCS: 88.36% (assembly containing the touched production files)
  - QuickFiler: 72.56%
  - TaskMaster: 67.43%
  - Tags: 67.28%
  - ToDoModel: 53.67%
  - TaskVisualization: 18.31%
  - VBFunctions: 100.00%
- These full-set figures differ from the T15-02 two-assembly baseline (which showed QuickFiler/Tags at 0% because their test assemblies were not run); the full CI-equivalent set exercises every test assembly, so this is the representative repository-wide reference for the P3-T5 no-regression comparison.

## No-regression reference

This is the authoritative pre-fix coverage reference for [P3-T5]. The remediation changes only add `[DoNotParallelize]` attributes to existing test classes (zero production `*.cs` change, no new module), so the production line-coverage denominator and covered-line counts cannot decrease; the repository-wide line-rate must remain at or above 81.80% under the same measurement method.
