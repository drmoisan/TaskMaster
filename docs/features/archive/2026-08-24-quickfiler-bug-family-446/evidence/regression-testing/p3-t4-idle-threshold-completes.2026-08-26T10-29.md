# [P3-T4] Idle Consumer Past the Threshold Terminates (Issue #448)

Timestamp: 2026-08-26T10-29

Task: [P3-T4]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — added
`UndoConsumer_IdleBeyondThreshold_Completes`. It arranges the consumer through
`ArrangeUndoConsumer(clock)` with a `FakeTimeProvider` and an empty `_undoQueue`, starts it inline,
advances the clock eleven seconds (past the ten-second `UndoConsumerIdleTimeout`), awaits the
consumer task and asserts `Status == TaskStatus.RanToCompletion`.

Added after the loop rewrite deliberately: its pre-fix state is a hang, not an assertion failure, so
it cannot serve as a failing-first test (D5). `[P3-T2]`/`[P3-T3]` carry the failing-first obligation
for #448.

`[Timeout(10000)]` bounds the test so that a regression reintroducing the non-terminating loop is
recorded as a timeout rather than stalling the suite. No wall-clock wait occurs on the passing path:
all elapsed time is simulated by `FakeTimeProvider.Advance`, satisfying the determinism rules in
`.claude/rules/general-unit-test.md`.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~UndoConsumer_IdleBeyondThreshold_Completes" "/Logger:trx;LogFileName=p3-t4.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t4"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t4/p3-t4.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, **timeout 0**, aborted 0.

- `UndoConsumer_IdleBeyondThreshold_Completes` = **Passed** in 289 ms.

**No `[Timeout]` trip recorded**: the TRX `timeout` counter is `0` and the recorded duration
(289 ms) is far inside the 10 000 ms bound.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

## Output Summary

The rewritten loop terminates on the idle path. Format EXIT_CODE 0, check EXIT_CODE 0, compile
EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1 Passed, 0 Failed and 0 timed out.
`QfcFormControllerSeamTests.cs` is 484 lines, within the 500-line cap.
