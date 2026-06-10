# Baseline — MSTest Coverage State (Issue #181)

Timestamp: 2026-06-08T12-27
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx
EXIT_CODE: 1

Output Summary:
- Test run completed with code coverage enabled. EXIT_CODE 1 (due to 4 pre-existing failing tests, not a coverage or collector failure).
- Totals: 4064 total tests; 4058 passed; 4 failed; 2 skipped. Total time ~41.3s.
- Repo-wide line coverage (raw, from merged Cobertura): line-rate 0.5888690448578188 = 58.89% (lines-covered 101554 of lines-valid 172456). branch-rate 1.0; complexity 9020.
- Coverage artifact (Cobertura) produced at: TestResults/baseline-coverage.cobertura.xml (converted from the .coverage attachment via dotnet-coverage merge -f cobertura).
- The 4 pre-existing failing tests (timing/timer-sensitive; unrelated to analyzer adoption; this plan changes no test or production code):
  - Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress
  - EmptyQueue_AfterSeveralIntervals_StopsTimer
  - Enqueue_InvokesBatchActionsOnTimerInterval
  - RequestTask_WithConfiguredTask_InvokesTaskAfterInterval

Notes on coverage figure:
- The 58.89% repo-wide raw figure is collected over ALL instrumented modules including vendored assemblies (SVGControl, UtilitiesSwordfish) and large COM/interop and auto-generated code that the CI coverage gate may scope out. The authoritative repo-wide coverage and the 80%/90% policy gates are evaluated by the PR GitHub Actions CI run, which applies the repo's coverage configuration.
- This baseline figure is the no-regression reference for changed-line coverage. This plan introduces no new compile-required production code (analyzer adoption + build-config + docs only), so no new-code 90% obligation is triggered unless a compile-required seam is later added (none in this plan as executed).
- The KNOWN ENVIRONMENT CAVEAT (Moq binding-redirect on System.Threading.Tasks.Extensions 4.2.0.1) did NOT block local coverage in this run; the collector ran successfully and produced a .coverage attachment.
