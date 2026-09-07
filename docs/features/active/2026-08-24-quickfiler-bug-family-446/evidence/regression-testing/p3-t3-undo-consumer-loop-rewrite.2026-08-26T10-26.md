# [P3-T3] Rewrite the Undo-Consumer Loop (Issue #448)

Timestamp: 2026-08-26T10-26

Task: [P3-T3]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcFormController.Actions.cs` — `UndoConsumer` rewritten:

- Added `private static readonly TimeSpan UndoConsumerIdleTimeout = TimeSpan.FromSeconds(10);`,
  preserving the previous ten-second threshold value.
- `long start = TimeProvider.GetTimestamp();` before the loop, replacing `new Stopwatch()` /
  `sw.Start()`.
- Loop condition is now `while (!_undoQueue.IsCompleted)`. The `|| exit` disjunction and the `exit`
  flag are gone entirely. The old condition `!_undoQueue.IsCompleted || exit` was the termination
  defect: setting `exit = true` made the disjunction **true**, so the loop it was meant to end
  spun instead.
- On a successful take: `await UndoItemProcessor(item)` then `start = TimeProvider.GetTimestamp();`
  so the threshold measures idle time rather than total session time.
- `else if (TimeProvider.GetElapsedTime(start) > UndoConsumerIdleTimeout) { break; }`.
- `else { await TimeProvider.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false); }`,
  replacing the ambient `await Task.Delay(200)`.
- The whole loop is wrapped in `try`/`finally` with `_undoConsumerTask = null;` moved into the
  `finally`, so it runs unconditionally including on the exception path that disposing `_undoQueue`
  mid-take (`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:216`) can produce.

Every branch of the loop body either awaits or breaks, so no branch reaches the loop head without
yielding.

Noted residue, not changed: `using System.Diagnostics;` at line 4 of the file is now unused, since
`Stopwatch` was the only `System.Diagnostics` type this file referenced. It is left in place because
the plan task does not authorize removing it. It is inert for every gate in this plan: no
`IDE0005` severity is configured in `.editorconfig` or `.globalconfig`, and `QuickFiler.csproj` sets
no `GenerateDocumentationFile`, which the compiler requires before it reports `IDE0005` during
build. Recorded here so a reviewer can decide rather than discover it.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcFormController.Actions.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcFormController.Actions.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay" "/Logger:trx;LogFileName=p3-t3.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t3"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t3/p3-t3.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` = **Passed**
  (was Failed at `[P3-T2]` with `found 0` delay requests).

`QfcFormController.Actions.cs` is 360 lines after `csharpier format`, within the 500-line cap.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

## Output Summary

The `[P3-T2]` `[expect-fail]` test transitions Failed -> Passed. Format EXIT_CODE 0, check
EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1 Passed and 0 Failed.
