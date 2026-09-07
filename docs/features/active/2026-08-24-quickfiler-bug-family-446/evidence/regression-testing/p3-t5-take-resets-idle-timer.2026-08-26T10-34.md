# [P3-T5] A Successful Take Resets the Idle Timer (Issue #448, AC9)

Timestamp: 2026-08-26T10-34

Task: [P3-T5]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — added
`UndoConsumer_SuccessfulTake_ResetsIdleTimer`. The shared `ArrangeUndoConsumer` helper gained an
optional `queuedItems` parameter so the seeding of `_undoQueue` is written once rather than in each
test (this is the helper extraction `[P3-T7]` prescribes).

Arrangement: a `CountingTimeProvider`, a fake `UndoItemProcessor` that records the item and advances
the clock six seconds, and three queued items. **No live Outlook COM call and no WinForms dispatcher
call occurs**, because the fake replaces the whole take-branch body extracted by `[P3-T1]`, which is
what `.claude/rules/general-unit-test.md` UT4 requires and why D-Plan-3 added that third seam.

Aggregate simulated time across the three takes is 18 s, past the ten-second threshold, while every
individual idle gap is 0 s.

## Why this test can fail

Because the consumer runs inline (`UndoConsumerStarter = body => body()`) and nothing else advances
the clock, the consumer is deterministically parked at the moment the starter returns. There is no
race in the observations:

- `processed` has 3 items — the consumer drained rather than exiting early.
- `consumer.IsCompleted` is `false` — it parked on an idle wait.
- `clock.DelayRequests` is `1` — it reached the idle branch, not the exit branch.

Against a session timer (the pre-`[P3-T3]` behaviour) the same arrangement produces
`IsCompleted == true` and `DelayRequests == 0`, because 18 s of accumulated session time exceeds the
threshold on the first empty take. The last two assertions are therefore genuine discriminators, not
restatements of the drain.

The test then advances 11 s — past the threshold measured from the last take — awaits the consumer
and asserts `TaskStatus.RanToCompletion`, so the fixed consumer is shown to terminate as well as to
persist. All elapsed time is simulated; `[Timeout(10000)]` bounds a regression that reintroduced a
non-terminating loop.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~UndoConsumer_SuccessfulTake_ResetsIdleTimer" "/Logger:trx;LogFileName=p3-t5.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t5"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t5/p3-t5.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `UndoConsumer_SuccessfulTake_ResetsIdleTimer` = **Passed** in 309 ms.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

Line-count note: `QfcFormControllerSeamTests.cs` stands at 534 lines after this task, above the
500-line cap. `[P3-T7]` is the designated task for bringing it back under and is executed after
`[P3-T6]` adds the last test.

## Output Summary

Format EXIT_CODE 0, check EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1
Passed and 0 Failed.
