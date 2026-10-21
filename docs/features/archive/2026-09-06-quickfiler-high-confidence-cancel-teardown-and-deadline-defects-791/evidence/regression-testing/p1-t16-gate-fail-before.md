# [P1-T16] [expect-fail] Gate and datamodel-projection tests, before the fix

Timestamp: 2026-09-06T14-46

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1-t16' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

ExpectedExitCode: 1
EXIT_CODE: 1

Output Summary: `Total tests: 46, Passed: 34, Failed: 12. Test Run Failed. Total time: 1.7412
Seconds.` A failing run is the expected and required outcome of this task: the production behaviour
these tests assert is supplied by Phase 2.

FAIL-BEFORE-COUNT: 12

## Failing tests, by fully qualified name and classification

Failure messages are reproduced from the TRX `ErrorInfo/Message` first line only. No raw TRX content
is pasted (R3); no message below carries a host path.

### NEW — the seven AC1 tests in `QfcStreamingDequeueConfidenceGateTests.Part4.cs` (6 of 7 red)

1. NEW `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`
   — `Expected batch.Accepted to contain a single item ... but the collection is empty.` This is the
   named fail-before evidence AC3 requires.
2. NEW `...DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted`
   — `Expected batch.Scanned to be 5 ... but found 2.`
3. NEW `...DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`
   — `Expected batch.Stop to be QfcDequeueStop.ScanCapReached {value: 3} ... but found QfcDequeueStop.SourceExhausted {value: 1}.`
4. NEW `...DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`
   — `Expected batch.Stop to be QfcDequeueStop.ScanCapReached {value: 3} ... but found QfcDequeueStop.DeadlineExpired {value: 2}.`
5. NEW `...DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts`
   — `Expected checkpoints to contain 3 item(s) ... but found 0: {empty}.`
6. NEW `...DequeueAsync_Launch_LogsCutoffQuantityAndBounds`
   — `Expected logs to contain a single item matching log.Contains("High-confidence dequeue launch"), but no such item was found.`

`...DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` is the seventh Part4 test and passes already.
That is correct and intended: it is the #608 regression pin, so it asserts behaviour this change must
*not* alter. A pin that were red before the change would be pinning the wrong thing.

### RETARGETED — the four tests in `...Part2.cs` (all 4 red)

7. RETARGETED `...DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier`
   — `Expected takeCount to be 51 ... but found 12 (difference of -39).` The 12 is the pre-change
   12-second bound at one second per score, which is exactly the superseded behaviour.
8. RETARGETED `...DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion`
   — `Expected takeCounter() to be 21 ... but found 3 (difference of -18).`
9. RETARGETED `...DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates`
   — `Expected batch.Stop to be QfcDequeueStop.ScanCapReached {value: 3} ... but found QfcDequeueStop.SourceExhausted {value: 1}.`
10. RETARGETED `...DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging`
    — `Expected checkpoints to contain 3 item(s) ... but found 0: {empty}.`

### RETARGETED — the two tests in `...Part3.cs` (1 of 2 red)

11. RETARGETED `...DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop`
    — `Expected batch.Stop to be QfcDequeueStop.ScanCapReached {value: 3} ... but found QfcDequeueStop.DeadlineExpired {value: 2}.`

`...DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` is the second Part3 retarget and
passes already. Its bound was changed from a 3-second deadline to an injected scan cap of 3, and at
one second per score both bounds admit the same three candidates, so its report sequence is
unchanged by construction. It is recorded here as retargeted-but-green rather than omitted.

### RETARGETED — the `QfcQueuePurePathsTests` projection test (red)

12. RETARGETED `QuickFiler.Controllers.Tests.QfcQueuePurePathsTests.DequeueNextItemGroupWithOutcomeAsync_ZeroAcceptanceCeilingGate_ReportsScanCapReachedStop`
    — `Expected batch.Stop to be QfcDequeueStop.ScanCapReached {value: 3} ... but found QfcDequeueStop.DeadlineExpired {value: 2}.`

## Relation to the [P0-T14] baseline

All seven tests [P0-T14] recorded as `BASELINE-PASS` were retargeted or superseded by Phase 1, and
six of the seven are now red under their new names. The seventh,
`DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`, kept its name and is green for
the reason stated above. No test outside the deliberately reddened set is failing in this run: the
34 passing tests include every unaffected gate test the plan's Citation table lists.
