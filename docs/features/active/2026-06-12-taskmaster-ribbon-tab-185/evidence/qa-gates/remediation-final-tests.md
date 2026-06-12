# Phase 2 — Final QA: Tests with Coverage (Issue #185)

Timestamp: 2026-06-12T11-30

Command:
```
vstest.console.exe \
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll \
  Tags.Test\bin\Debug\Tags.Test.dll \
  TaskMaster.Test\bin\Debug\TaskMaster.Test.dll \
  TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll \
  ToDoModel.Test\bin\Debug\ToDoModel.Test.dll \
  UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll \
  VBFunctions.Test\bin\Debug\VBFunctions.Test.dll \
  /EnableCodeCoverage /InIsolation /ResultsDirectory:coverage-out-final
```
(Executed; NOT skipped. A Debug `-t:Build` was run first to restore the test assemblies that the P2-T3 forced nullable `-t:Rebuild` had cleaned — a mechanically-necessary build-state restore, not a source change.)

EXIT_CODE: 1

Output Summary:
- Total tests: 4068. Passed: 4067. Failed: 1.
- The single failure is `UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:219). Assertion: expected callCount 0 (action must not run when the UiThread Dispatcher is unavailable) but found 1.
- This failure is OUT OF SCOPE for #185 and is non-deterministic (flaky):
  - It is a WinForms/Dispatcher UI-thread timing test in `UtilitiesCS`, unrelated to the #185 in-scope files (`RibbonExplorer.xml` / `RibbonExplorerXmlTests.cs`).
  - The same test PASSED in the P1-T1 repo-wide run (4068/4068) at 2026-06-12T11-20.
  - Re-running the test in isolation passed: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` -> Test Run Successful, 1/1 passed (EXIT 0).
  - The in-scope production change is a non-compiled XML resource with no instrumentable IL, so it cannot affect this Dispatcher-timing test. The failure is a pre-existing Dispatcher-availability flake, not a regression from #185. Recorded honestly; not masked and not suppressed.

## Numeric coverage (post-change)
- Repository-wide C# line-rate: 58.94% (root Cobertura `line-rate=0.5894`, lines-covered 101852 / lines-valid 172813) per the canonical artifact `artifacts/csharp/coverage.xml` produced in P1-T2 from the equivalent seven-assembly run. Below the >= 80% policy threshold; reported honestly, no threshold weakened (cross-reference `repo-wide-coverage.md`).
- In-scope changed-file coverage (from the P2-T4 coverage attachment, cross-referencing P1-T3):
  - `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`: 168/170 lines = 98.82% (the 2 uncovered lines are in the compiler-generated lambda display class `<>c`, not authored source).
  - `TaskMaster/Ribbon/RibbonExplorer.xml`: not present in coverage (non-compiled XML resource, no instrumentable IL); no changed-line coverage regression possible.

Coverage attachment (P2-T4 run): `coverage-out-final/2a0e4c04-9b52-4195-8c18-d0d352dffdcf/DanMoisan_MEGALODON4_2026-06-12.11_30_08.coverage`
