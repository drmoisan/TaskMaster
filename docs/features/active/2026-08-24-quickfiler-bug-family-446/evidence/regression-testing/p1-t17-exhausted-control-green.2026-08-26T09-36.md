# [P1-T17] Source-Exhausted Empty Batch Closes the Queue (AC2 Negative Control)

Timestamp: 2026-08-26T09-36

Task: [P1-T17]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` — added
`IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce`, which arranges an empty
batch whose `Stop` is `QfcDequeueStop.SourceExhausted` through the `ArrangeIterate` `stop`
parameter and asserts `IQfcQueue.CompleteAddingAsync` was invoked `Times.Once`.

This is the negative control of AC2 and is green in both the pre-fix and post-fix states, so it is
NOT tagged `[expect-fail]`. AC2's failing-first obligation is carried by `[P1-T16]`. The control
matters because it forecloses a degenerate fix: a change that simply stopped calling
`CompleteAddingAsync` would turn `[P1-T16]` green while breaking this test.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce" "/Logger:trx;LogFileName=p1-t17.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t17"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t17/p1-t17.trx`

Total tests 1, **Passed 1**, Failed 0.

## Output Summary

AC2's negative control lands green as planned. Format EXIT_CODE 0, compile EXIT_CODE 0, scoped run
EXIT_CODE 0 with the single test recorded Passed. Paired with `[P1-T16]`, the two tests pin both
sides of the stop-reason discrimination: one stop reason must close the queue and the other must
not.
