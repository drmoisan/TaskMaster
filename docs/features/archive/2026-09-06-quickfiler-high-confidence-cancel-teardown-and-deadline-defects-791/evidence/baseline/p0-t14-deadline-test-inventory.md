# [P0-T14] Pre-change status of the seven deadline-dependent tests (D2)

Timestamp: 2026-09-06T14-31

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t14' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines. The run uses the
same runsettings, isolation and blame switches as [P0-T10] and differs only in the class-scoping
`/TestCaseFilter`, so the seven statuses come from a run whose scope is exactly the affected set.

The `TestCategory!=LiveOutlook` clause is deliberately omitted from this filter rather than combined
with the two `FullyQualifiedName` clauses, because `&` binds tighter than `|` in a vstest filter
expression: the combined form `TestCategory!=LiveOutlook&A|B` would apply the category exclusion to
only the first clause. Neither class declares a `LiveOutlook` test, so omitting it changes no
selected test.

EXIT_CODE: 0

Output Summary: `Test Run Successful. Total tests: 39, Passed: 39, Total time: 1.6524 Seconds.`

## The seven deadline-dependent tests, from the `TestResults\791-p0-t14` TRX

BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcQueuePurePathsTests.DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop

BASELINE-PASS-COUNT: 7

## Cross-check against the [P0-T10] whole-assembly run

All seven names were also queried by outcome in the `TestResults\791-p0-t10` TRX (the
whole-assembly baseline that recorded 1339 passed, 0 failed) and all seven are recorded there as
`Passed`. The class-scoped run and the whole-assembly run therefore agree on every one of the seven,
so the class-scoping filter did not change any outcome.

## Reading

This is the set Phase 1 deliberately turns red and Phase 2 turns green again. All seven pass today,
which is what makes the Phase 2 no-newly-failing comparison in [P2-T15] meaningful: a test in this
set that is still red at the end of Phase 2 is a regression rather than a pre-existing failure.

Four of the seven live in `QfcStreamingDequeueConfidenceGateTests.Part2.cs` and are retargeted by
[P1-T8]; two live in `.Part3.cs` and are retargeted by [P1-T9]; one lives in
`QfcQueuePurePathsTests.cs` and is retargeted by [P1-T10]. Three of the seven are outside the four
retargeting obligations `spec.md` Test Strategy names, which is the D2 finding.
