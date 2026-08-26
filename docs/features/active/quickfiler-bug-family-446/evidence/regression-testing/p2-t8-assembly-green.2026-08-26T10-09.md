# [P2-T8] Whole-Assembly Green After the Phase 2 Change Set

Timestamp: 2026-08-26T10-09

Task: [P2-T8]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/Logger:trx;LogFileName=p2-t8.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t8"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t8/p2-t8.trx`

Counters: **total 952, executed 952, passed 952, failed 0**, error 0, timeout 0, aborted 0,
notExecuted 0.

## First run and the one invalidated pre-existing test

The first execution of this task returned total 952, passed 951, **failed 1**:
`QfcHomeControllerIterationTests.IterateQueueAsync_QueueEmpty`, with
`Moq.MockException: Expected invocation on the mock once, but was 0 times:
m => m.CompleteAddingAsync(...)`.

That failure is the intended consequence of `[P2-T7]`, not a defect. The test arranged an empty
batch through `ArrangeIterate()`, whose `stop` parameter defaults to
`QfcDequeueStop.QuantitySatisfied`, and asserted the queue was closed. Under issue #446 an empty
batch no longer closes the queue on its own; only `QfcDequeueStop.SourceExhausted` does. The test as
written encoded the defect being fixed.

Correction applied: `IterateQueueAsync_QueueEmpty` now arranges
`ArrangeIterate(stop: QfcDequeueStop.SourceExhausted)`, which is the stop reason its name and intent
("queue empty, source drained, so close") always described. **No assertion was weakened**: the test
still verifies `DequeueNextItemGroupWithOutcomeAsync` `Times.Once`, `CompleteAddingAsync`
`Times.Once` and `EnqueueAsync` `Times.Never`. A two-line comment records why the stop reason is now
stated explicitly.

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` is **497 lines** after the change
and after `csharpier format`, within the 500-line cap.

Toolchain rerun after that edit:

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

TRX hygiene: the recorded TRX is the second (green) run. It was scrubbed of the absolute worktree
path, the account name and the machine name, then re-parsed as XML; `<Counters .../>`, all test
names and all outcomes survive unchanged. No `danmoisan` or `megalodon4` match anywhere under the
feature folder. The empty `Deploy_*` scratch directories `vstest /InIsolation` leaves behind contain
no files and are therefore untracked by git.

## Output Summary

**Failed 0, total 952 (> 0).** All nine Phase 1 `[expect-fail]` tests that Phase 2 owns are green,
and the whole `QuickFiler.Test` assembly is green with them.
