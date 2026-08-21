# quickfiler-undoconsumer-nonterminating-loop (Issue #448)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-undoconsumer-nonterminating-loop/ (Issue #448)
- Found during: research for issue #435 (child F6 of epic #136, QuickFiler per-file coverage)

- Issue: #448
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/448
- Last Updated: 2026-08-08
## Summary

`QfcFormController.UndoConsumer()` contains a loop that never terminates. Once its 10-second
threshold is crossed the loop stops awaiting and busy-spins on a background thread for the remaining
lifetime of the process.

## Location

`QuickFiler/Controllers/QfcFormController.Actions.cs:253-292`

## Observed Behavior

The loop condition at line 258 is:

```csharp
while (!_undoQueue.IsCompleted || exit)
```

Two independent problems compound:

1. `_undoQueue` is a `BlockingCollection<IMovedMailInfo>` (declared at `QfcFormController.cs:90`) and
   no code path anywhere calls `CompleteAdding()` on it. `IsCompleted` is therefore permanently
   `false`, so `!_undoQueue.IsCompleted` alone holds the condition true regardless of `exit`.
2. `exit` is set `true` only in the `sw.ElapsedMilliseconds > 10000` branch at line 279. Because the
   condition is a disjunction (`|| exit`) rather than a conjunction, setting `exit` makes the
   condition *more* likely to hold, not less. After the threshold the `else if` branch is taken on
   every iteration, which re-sets `exit` and reaches no `await`, so the thread spins without yielding.

Before the 10-second mark the loop awaits `Task.Delay(200)` on the empty-queue path, so the spin only
begins after the threshold.

The consequence is that the post-loop cleanup at lines 288-291:

```csharp
if (exit)
{
    _undoConsumerTask = null;
}
```

is unreachable in any terminating execution. `_undoConsumerTask` is never reset, so the `??=` guard at
line 211 (`_undoConsumerTask ??= Task.Run(UndoConsumer);`) never restarts a consumer either.

## Expected Behavior

The loop should terminate when the queue is drained and the idle threshold has elapsed. The condition
appears to have been intended as a conjunction, for example `while (!_undoQueue.IsCompleted && !exit)`,
and the empty-queue path should continue to yield rather than spin.

## Impact

A QuickFiler session in which the user opens the undo dialog leaves a CPU-bound background thread
running until Outlook exits. Severity depends on how often `UndoDialog()` is reached in practice.

## Why This Is Filed Separately

Discovered during read-only research for the F6 coverage child. Correcting the loop condition is a
behavior change, and F6's acceptance criteria explicitly forbid behavior changes to observable
QuickFiler flows. F6 works around it by introducing an injectable start-delegate around
`Task.Run(UndoConsumer)` so that unit tests never start the real loop; that seam does not fix the
defect.

## Proposed Fix

1. Add a failing regression test that drives `UndoConsumer` past the idle threshold and asserts it
   completes, using an injected time provider and delay delegate so the test contains no wall-clock
   wait (`Thread.Sleep`, `Task.Delay`, and real waits are prohibited in tests by
   `.claude/rules/general-unit-test.md`).
2. Correct the loop condition and the idle-exit path.
3. Confirm `_undoConsumerTask` is reset so a later `UndoDialog()` can start a fresh consumer.

## Acceptance Criteria (early draft)

- [ ] A deterministic regression test fails before the fix and passes after, with no wall-clock wait.
- [ ] `UndoConsumer()` terminates once the queue is drained and the idle threshold elapses.
- [ ] `_undoConsumerTask` is reset on exit so a subsequent `UndoDialog()` starts a new consumer.
- [ ] No busy-spin: the empty-queue path always yields.
- [ ] Full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled vstest.

## Next Step

- [ ] Promote to GitHub issue (bug template)
