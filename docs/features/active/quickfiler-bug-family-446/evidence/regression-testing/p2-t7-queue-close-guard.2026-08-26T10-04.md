# [P2-T7] Guard the Queue Close on the Empty-Batch Path (Issue #446, AC2)

Timestamp: 2026-08-26T10-04

Task: [P2-T7]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcHomeController.Iteration.cs` — the unconditional `else` around
`await QfcQueue.CompleteAddingAsync(Token, 10000);` inside `IterateQueueAsync` became
`else if (batch.Stop == QfcDequeueStop.SourceExhausted)`, with a comment recording that a
`DeadlineExpired` or `QuantitySatisfied` empty batch deliberately leaves the queue open for a later
iteration.

`QfcQueue.CompleteAddingAsync` reaches `BlockingCollection<T>.CompleteAdding()` at
`QuickFiler/Controllers/QfcQueue.cs:59`, which is irreversible for the rest of the session; that is
why the guard is on the close rather than on a recovery path.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcHomeController.Iteration.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcHomeController.Iteration.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding|FullyQualifiedName~IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce" "/Logger:trx;LogFileName=p2-t7.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t7"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t7/p2-t7.trx`

Counters: total 2, executed 2, **passed 2**, failed 0, error 0, timeout 0, aborted 0.

- `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` = **Passed**
  (was Failed at `[P1-T16]`).
- `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` = **still Passed**
  (`[P1-T17]` negative control; it forecloses the degenerate fix of never closing the queue).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test names and outcomes unchanged. No `danmoisan` or `megalodon4` match
anywhere under the feature folder.

## Output Summary

The consumer end of the stop reason is now wired: only `SourceExhausted` closes the UI queue. The
`[P1-T16]` `[expect-fail]` test transitions Failed -> Passed and its negative control stays Passed,
so both sides of the discrimination are pinned. Format EXIT_CODE 0, check EXIT_CODE 0, compile
EXIT_CODE 0, scoped run EXIT_CODE 0 with 2 of 2 Passed and 0 Failed.
`QfcHomeController.Iteration.cs` is 95 lines, within the 500-line cap.
