# Phase 1 (S7/J1) — J1 + OlTableExtensions_Tests (Cycle 7)

Timestamp: 2026-06-09T18-00

Resolved vstest.console: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

## Run 1 — full OlTableExtensions_Tests class in isolation

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:FullyQualifiedName~OlTableExtensions_Tests /InIsolation
EXIT_CODE: 0
Output Summary:
```
Total tests: 83
     Passed: 83
```
All 83 tests in the class pass, including the four GetTableInViewAsync reflection
call-site tests updated for the new signature (P1-T4) and the rewritten J1 (P1-T5).

## Run 2 — the four GetTableInViewAsync tests by name

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry,GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException,GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException,GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot /InIsolation
EXIT_CODE: 0
Output Summary:
```
  Passed GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException [303 ms]
  Passed GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry [26 ms]
  Passed GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException [1 ms]
  Passed GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot [1 ms]
Total tests: 4
     Passed: 4
```

J1 determinism confirmed:
- J1 now runs in ~26 ms vs ~280 ms at baseline (the 20 ms wall-clock block is gone).
- J1 passes with NO Thread.Sleep: the injected timeout-source factory returns a
  CancellationTokenSource cancelled synchronously inside the first GetTable call;
  result == mockTable.Object and callCount == 1 (both assertions preserved).
- The 303 ms on NullTableView is unrelated first-call Moq/Outlook interop init overhead
  (it throws before any timeout path); it carries no wait dependency.

The other OlTableExtensions_Tests tests remain green (83/83). No regression.
