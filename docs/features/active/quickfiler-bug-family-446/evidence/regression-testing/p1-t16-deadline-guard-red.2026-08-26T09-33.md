# [P1-T16] [expect-fail] Deadline-Expired Empty Batch Must Not Close the Queue

Timestamp: 2026-08-26T09-33

Task: [P1-T16] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` — added
`IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding`.

`ArrangeIterate` gained a `QfcDequeueStop stop = QfcDequeueStop.QuantitySatisfied` parameter, which
is the value the helper now puts on the `QfcDequeueBatch` returned by the mocked
`DequeueNextItemGroupWithOutcomeAsync`. The default preserves every existing call site unchanged.
The new test passes `stop: QfcDequeueStop.DeadlineExpired` with the default empty batch, drives
`IterateQueueAsync`, and asserts `IQfcQueue.CompleteAddingAsync` was invoked `Times.Never`.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding" "/Logger:trx;LogFileName=p1-t16.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t16"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t16/p1-t16.trx`

TRX counters: `total="1" executed="1" passed="0" failed="1"`.

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX (entity references expanded for readability):

```
Moq.MockException: a deadline-bounded empty batch must not close the queue
Expected invocation on the mock should never have been performed, but was 1 times: m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>())

Performed invocations:

   Mock<IQfcQueue:1> (m):

      IQfcQueue.CompleteAddingAsync(CancellationToken, 10000)
```

This is a Moq verification assertion failure raised by `Mock.Verify`, carrying the reason string the
test supplied. The TRX stack trace begins at `Moq.Mock.Verify(...)`, confirming the failure is the
assertion itself and not a build error or an unhandled production exception. The RED state is the
`else` branch of `QuickFiler/Controllers/QfcHomeController.Iteration.cs`, which
`[P1-T15]` deliberately left unconditional: any empty batch closes the queue today, whatever the
stop reason. `[P2-T7]` adds the `SourceExhausted` guard and turns this test green.

## Output Summary

Failing-first test for AC2 lands RED by Moq verification assertion. Format EXIT_CODE 0, compile
EXIT_CODE 0, scoped run EXIT_CODE 1 with 1 executed and 1 Failed. The observed invocation shows the
production path calling `CompleteAddingAsync(CancellationToken, 10000)` on a deadline-expired empty
batch, which is exactly the defect issue #446 reports.
