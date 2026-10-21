# Issue 633 status update mirror (P8-T25)

Timestamp: 2026-09-01T11-21
Task: [P8-T25]
Issue: https://github.com/drmoisan/TaskMaster/issues/633
PostedAs: comment
Comment URL: https://github.com/drmoisan/TaskMaster/issues/633#issuecomment-5496227321

Posted with `gh issue comment 633 --body-file <path>`. The issue was open at the time of posting; its
body was not modified, so no mirror into `FEATURE/issue.md` is required — that mirroring obligation
applies only to `PostedAs: body`.

## Exact text posted

## Fix delivered on `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`

The unsynchronized undo handoff is closed. The batch-move path now expresses its ordering dependency as
a control-flow property rather than an assumption.

### What changed

**`QuickFiler/Controllers/FilerQueue.cs`**

- Added `public Task WhenDrainedAsync()`, a counted, per-batch, awaitable quiesce. It returns an
  already-completed task when nothing is outstanding, and otherwise a task that completes when the
  outstanding-work count next reaches zero. It is idempotent and safe to await repeatedly or
  concurrently, and it completes rather than faulting, so a logged item failure is not converted into an
  unhandled exception on the batch-move path.
- **Repaired the producer/consumer handshake.** The `ThreadSafeSingleShotGuard` start gate is replaced
  by a start/stop decision taken under a single monitor. The consumer-running flag is now cleared in the
  same critical section in which `TryTake` fails, which closes the orphaned-item window: previously a
  producer whose `Queue.Add` landed between the worker's loop exit and its guard reinstall read the
  already-tripped guard, started no worker, and left its item stranded. This repair is a precondition
  for a sound barrier, not an opportunistic refactor — a barrier over the old handshake would have
  reported "drained" while an item was stranded, or never completed at all.
- The outstanding-work counter is decremented in a `finally`, so a throwing item still decrements and
  the drain cannot hang. The existing per-item `catch`, its `item.Helpers.First()` diagnostic, and its
  `logger.Error` call are preserved unchanged.
- Added an `internal Func<FilerQueueItem, Task> ItemProcessor` seam whose production default preserves
  the existing call. It exists so the queue can be driven deterministically from a unit test; the real
  `EmailFiler.SortAsync` is non-virtual and casts to a COM folder.
- `Consumer` is retained with its type, accessibility and `Task.CompletedTask` default. The change is
  additive on the public surface.

**`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`**

- `BackGroundMoveAsync` now awaits `_parent.FilerQueue.WhenDrainedAsync()` between the batch move and
  the `WriteMetrics` dispatch. There is no longer any control-flow path from a completed batch move to
  `WriteMetrics` or `CleanupBackground` that does not pass through the barrier. Metrics-before-cleanup
  order is unchanged.
- Added a `_parent` null check to the method's early-return guard, required because the barrier
  dereferences `_parent` and cleanup sets it to null.
- Deleted the two now-subsumed `await _parent.FilerQueue.Consumer;` statements. Both were strictly
  subsumed: each was immediately preceded by an await of the same `BackGroundMoveAsync` task, and the
  barrier waits on the whole outstanding count rather than on one worker task.

### Verification

The defect carries a genuine fail-before / pass-after pair. Both
`BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` and
`...DoesNotDispatchCleanupBeforeDrain` failed against the pre-fix tree — with one item parked behind a
closed gate, the metrics recorder count was deterministically 1 by the time an equal-priority dispatcher
probe completed — and both pass after the fix. Determinism comes from dispatcher enqueue order and
`TaskCompletionSource` gates, never from a sleep, delay, poll, or timeout.

Twelve tests were added: seven queue-level cases covering the drain contract, the orphaned-item
regression, and a throwing processor; and five ordering cases covering both barrier paths, the
metrics-then-cleanup order, and both guard branches.

Toolchain, in one uninterrupted pass: `csharpier check` reports 0 unformatted files; both
`msbuild /t:Rebuild` gates exit 0 with zero `Skipping target "CoreCompile"` occurrences in their logs;
6924 tests run, 6924 passed, 0 failed. Repository-wide line coverage moved from 85.32 percent to 85.39
percent over the same filtered denominator. `FilerQueue.cs` reaches a per-file rate of 1.00, and zero of
the 138 changed production lines are uncovered.

The production diff touches only the two files named above.

### Follow-up raised separately

`UtilitiesCS.Test/.../DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` redirects
process-global `Console.Out` and has no `[DoNotParallelize]` attribute, so a sibling class's
`Console.SetOut` can clobber its redirect under the class-level parallel scope. It failed once during
this work and passed in isolation and on re-run. The identical hazard is already mitigated on
`PrettyPrint_Tests`, which carries `[DoNotParallelize]` with an explanatory comment. This is unrelated
to #633 and outside its authorized scope, so it was not fixed here and should be raised as its own
issue.
