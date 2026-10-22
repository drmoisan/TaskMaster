# [P3-T1] Three Injectable Seams on the Undo Consumer (Issue #448)

Timestamp: 2026-08-26T10-18

Task: [P3-T1]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcFormController.Actions.cs` — a partial declaration of
`QfcFormController`, so the seams land without editing the non-owned
`QuickFiler/Controllers/QfcFormController.cs` where `_undoQueue` and `_undoConsumerTask` are
declared.

1. `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;`
2. `internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);`
3. `internal Func<IMovedMailInfo, Task> UndoItemProcessor { get; set; }`, defaulted to the new
   private `ProcessUndoItemAsync(IMovedMailInfo item)` which holds the take-branch body verbatim
   (D-Plan-3). The default is resolved lazily through a backing field
   (`get => _undoItemProcessor ??= ProcessUndoItemAsync;`) because a C# instance property
   initializer cannot reference an instance method (CS0236); a constructor-based default would have
   required editing the non-owned `QfcFormController.cs`.
4. `_undoConsumerTask ??= Task.Run(UndoConsumer);` became
   `_undoConsumerTask ??= UndoConsumerStarter(UndoConsumer);`.
5. The take branch became `await UndoItemProcessor(item).ConfigureAwait(false);`.

The loop itself is **unchanged by this task**: `new Stopwatch()`, the `|| exit` disjunction, the
`sw.ElapsedMilliseconds > 10000` branch, `await Task.Delay(200)` and the conditional
`if (exit) { _undoConsumerTask = null; }` all remain exactly as they were. Production behaviour is
byte-for-byte unchanged because every seam defaults to the code it replaced.

`Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing` were already
referenced; no package was added.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcFormController.Actions.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcFormController.Actions.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcFormControllerSeamTests|FullyQualifiedName~QfcFormControllerTests" "/Logger:trx;LogFileName=p3-t1.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t1"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t1/p3-t1.trx`

Counters: total 53, executed 53, passed 53, **failed 0**, error 0, timeout 0, aborted 0.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, all test names and all outcomes unchanged. A case-insensitive search
for the account name and the machine name across the feature folder returns no match.

`QfcFormController.Actions.cs` is 343 lines after `csharpier format`, within the 500-line cap.

## Output Summary

The intra-phase compile exits 0 and the scoped `QfcFormControllerSeamTests` /
`QfcFormControllerTests` run records **0 failed** across 53 tests, so the seams are additive and
regress nothing.
