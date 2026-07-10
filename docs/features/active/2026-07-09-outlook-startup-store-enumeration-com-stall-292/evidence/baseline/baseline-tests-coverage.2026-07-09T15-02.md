# Baseline Test + Coverage (issue #292)

- Timestamp: 2026-07-09T15-02
- Task: [P0-T6]
- Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
  - Note: `/InIsolation` added so the Moq test assemblies load without the STTE 4.2.0.1 Setup FileNotFound issue; no behavioral filter applied (full suite ran).
- EXIT_CODE: 0
- Raw coverage report: `artifacts/csharp/coverage.xml` (Microsoft merged XML) and `artifacts/csharp/coverage.baseline.cobertura.xml` (Cobertura, per-line hits).

## Output Summary

- Test result: `Test Run Successful.` Total tests: 4514; Passed: 4514; Failed: 0. Total time 51.77 s.
- Baseline repository-wide (first-party production modules, raw whole-module instrumentation) line coverage: **39.78%** (39115 / 98340 lines).
  - Per-module contributors: UtilitiesCS 45.31% (37002/81660), TaskMaster 40.48% (2059/5086), ToDoModel 2.06% (41/2036), TaskVisualization 14.94% (13/87), Tags 0% (0/1550), QuickFiler 0% (0/7921).
- Assembly containing the three touched production files (`CurrentStoreContext.cs`, `StoresWrapper.cs`, `StoreLockupResponder.cs`): **UtilitiesCS = 45.31%**.

## Coverage-policy context

- The raw whole-module first-party figure includes COM/VSTO/WinForms and Outlook-interop code that CLAUDE.md formally exempts from the 80% floor. The 80% floor applies to the testable first-party denominator (after those exclusions), enforced by the feature-review canonical coverage pipeline.
- This baseline is the no-regression reference for [P3-T5]. The change under this plan adds only host-neutral, fully-reachable lines, so it cannot reduce the testable-denominator rate; the binding numeric gate for this change is the >= 90% new/changed-code coverage of the three touched files (verified in [P3-T4]/[P3-T5]).
