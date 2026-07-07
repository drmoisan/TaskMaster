# Baseline — MSTest Coverage (QuickFiler.Test) (Issue #255)

Timestamp: 2026-07-07T13-15

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation

Note on flags/tooling:
- `/InIsolation` is required for this Moq-based test assembly (documented STTE 4.2.0.1 "Setup FileNotFound" otherwise).
- `/EnableCodeCoverage` produces a binary `.coverage` attachment. It was converted to Cobertura for numeric per-file extraction via `dotnet-coverage merge -o <out>.cobertura.xml -f cobertura <run>.coverage` (dotnet-coverage 18.5.2). The conversion is a mechanically-necessary micro-action to obtain the numeric coverage the evidence contract requires; it does not alter the test run.

EXIT_CODE: 0

Output Summary:
- Test results (plan command `/EnableCodeCoverage /InIsolation`): Total tests 488, Passed 488, Failed 0.
- Plain `vstest /InIsolation` (no coverage): 488/488 passed, confirming a clean baseline.
- Whole-run overall line coverage (all modules loaded during the QuickFiler.Test run, merged Cobertura): 20.23% (22150/109500 lines). This whole-solution denominator is low because QuickFiler.Test exercises only QuickFiler-adjacent code; it is recorded for provenance, not as the assembly gate.
- Fix-Scope file baseline line coverage (from converted Cobertura):
  - QuickFiler/Helper Classes/ConversationResolver.cs: 82.04% (274/334)
  - QuickFiler/Helper Classes/ConversationResolver.Loading.cs: 69.45% (291/419)
  - QuickFiler/Controllers/QfcItemController.Conversation.cs: 80.81% (160/198)

Known-flaky observation (not a regression): under `dotnet-coverage collect` instrumentation (an alternative collection path, not the plan command), 3 BackgroundWorker IsBusy-race tests (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`) flake due to a documented async-void IsBusy timing race. They pass under the plan's `/EnableCodeCoverage` collector and under plain vstest. They are unrelated to this fix.
