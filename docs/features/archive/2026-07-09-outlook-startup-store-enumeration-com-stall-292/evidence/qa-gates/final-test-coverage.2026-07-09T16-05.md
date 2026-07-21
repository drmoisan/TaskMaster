# Final Test + Coverage (issue #292, remediation cycle 1) — POST-FIX

- Timestamp: 2026-07-09T16-05
- Task: [P3-T4]

## Authoritative pass/fail — CI-equivalent `/EnableCodeCoverage` invocation (the required CI gate path)

- Command: `vstest.console.exe <all 7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` Total tests: 5141; Passed: 5141; **Failed: 0**.
- This is the authoritative P3-T4 result. Combined with P2-T13 (passes A–D + the 7 earlier passes), the CI gate path is green and the 10 formerly-failing #292 tests pass in every pass.

## Coverage headline — reliable `dotnet-coverage collect` -> Cobertura path

- Command: `dotnet-coverage collect --output <scratchpad>/postchange.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> <all 7 *.Test.dll> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- Repository-wide (Cobertura root, all instrumented modules) line-rate: **81.80%** (121602 / 148653) [second measurement 81.82%]; branch-rate 59.66%.
- Per production package line-rate: UtilitiesCS **88.33%**, QuickFiler 72.52%, TaskMaster 67.43%, Tags 67.28%, ToDoModel 53.67%, TaskVisualization 18.31%, VBFunctions 100.00%.
- No regression versus the P0-T6 pre-fix baseline (ROOT 81.80% / UtilitiesCS 88.36%). The `[DoNotParallelize]` attributes add no production code, so coverage is structurally unchanged.

## Out-of-scope observation on the coverage-measurement path (NOT the CI gate)

Under the `dotnet-coverage collect` path (used only for the coverage NUMBER, not the CI pass/fail gate),
2 `TaskMaster.Test` tests flaked (`StoresWrapperEnumerationScopeTests.Init_HealthyMultiStore_...ClearsContextAfterReturn`
and `Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull`). This is a pre-existing, INTRA-`TaskMaster.Test`
instance of the same #292 race, OUTSIDE the plan's `UtilitiesCS.Test` scope, documented in
`evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md`. Root cause of the exposure:
`TaskMaster.Test` has NO `[assembly: Parallelize]`, so it runs sequentially (race-free) under the CI
`/EnableCodeCoverage` invocation; the flake appears only because the coverage-measurement
`TaskMaster.cli.runsettings` force-imposes `ClassLevel` parallelization. It is therefore NOT a CI-gate risk
under the current configuration, and it does not affect the coverage number (the tests still execute their
production lines before the assertion). Per the execution directive, it was reported rather than fixed.
