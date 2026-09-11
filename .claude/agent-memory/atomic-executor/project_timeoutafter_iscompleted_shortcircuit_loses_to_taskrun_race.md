---
name: timeoutafter-iscompleted-shortcircuit-loses-to-taskrun-race
description: TimeoutAfter's IsCompleted short-circuit almost never fires for a Task.Run-started task, so plan prose claiming "no timer is armed" for a fast-completing work item is unreliable
metadata:
  type: project
---

`UtilitiesCS/Threading/TimeOutTask.cs` — `TimeoutAfter(this Task, int, TimeProvider?)` short-circuits
and returns the original task when `task.IsCompleted` is true, otherwise it arms a timer via
`(timeProvider ?? TimeProvider.System).CreateTimer(...)` and wires a `ContinueWith` that disposes
the timer and marshals results into the proxy.

A plan clause asserting that a *fast* work item "short-circuits and arms no timer" is wrong in the
common case: when the work is started with `Task.Run(...)`, the calling thread reaches `TimeoutAfter`
before the pool thread has run the delegate, so `IsCompleted` is false and a timer IS armed.

**Why:** the observable outcome is unchanged, which is what makes the false rationale easy to ship.
With a `FakeTimeProvider` the armed timer never fires unless the test advances the clock, and the
`ContinueWith` completes the proxy as soon as the work finishes. So the test still returns normally
with the expected invocation count, and there is no flake and no hang — only the stated reason is
wrong.

**How to apply:** treat this clause as rationale, not as an acceptance condition, and do not let a
test assert "no timer was armed". If a barrier wrapper counts `CreateTimer` forwardings, expect one
more arming than the prose predicts for the N=1 case, and make the barrier's completion source
tolerant of an unobserved signal (`TrySetResult`, not `SetResult`).

Related: [[project_faketimeprovider_zero_duetime_fires_at_creation]],
[[project_preflight_citation_match_propagates_false_fact]].
