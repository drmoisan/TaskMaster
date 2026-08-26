# [P3-T6] Every Exit Path Clears `_undoConsumerTask` (Issue #448)

Timestamp: 2026-08-26T10-38

Task: [P3-T6]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — added
`UndoConsumer_OnExit_ResetsUndoConsumerTask`, covering both exit paths in one test:

1. **Idle exit.** Empty queue, `FakeTimeProvider` advanced eleven seconds past the threshold. After
   awaiting the consumer, `_undoConsumerTask` must be null.
2. **Exception exit.** One queued item and a `UndoItemProcessor` that throws
   `InvalidOperationException`, which stands in for the exception path that disposing `_undoQueue`
   mid-take (`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:216`) can produce. After the
   throw propagates, `_undoConsumerTask` must be null as well.

Both halves plant `Task.CompletedTask` into `_undoConsumerTask` as a sentinel **before** the
consumer runs. Without the sentinel the field is null from construction and the assertion would pass
vacuously. With it, a path that fails to clear the field leaves the sentinel behind and the
assertion fails — which is precisely the pre-`[P3-T3]` behaviour on the exception path, where
`_undoConsumerTask = null;` sat inside `if (exit)` and `exit` was never set when the loop threw.

The consequence being pinned: a stale non-null `_undoConsumerTask` makes the `??=` in `UndoDialog()`
a no-op, so the user's next undo silently never starts a consumer.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~UndoConsumer_OnExit_ResetsUndoConsumerTask" "/Logger:trx;LogFileName=p3-t6.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t6"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t6/p3-t6.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `UndoConsumer_OnExit_ResetsUndoConsumerTask` = **Passed** in 315 ms.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

Line-count note: `QfcFormControllerSeamTests.cs` stands at 576 lines after this task, above the
500-line cap. `[P3-T7]` is the designated task for bringing it back under.

## Output Summary

Format EXIT_CODE 0, check EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1
Passed and 0 Failed.
