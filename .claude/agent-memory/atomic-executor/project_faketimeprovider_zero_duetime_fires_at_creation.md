---
name: faketimeprovider-zero-duetime-fires-at-creation
description: FakeTimeProvider.CreateTimer with dueTime TimeSpan.Zero invokes the callback during creation, so a "not completed before Advance" pre-assertion on a zero-delay wait is false and no Advance is needed
metadata:
  type: project
---

`Microsoft.Extensions.Time.Testing.FakeTimeProvider.CreateTimer(cb, state, TimeSpan.Zero, Timeout.InfiniteTimeSpan)`
invokes `cb` **during the CreateTimer call**, not on the next `Advance`. A helper that completes a
`TaskCompletionSource` from that callback therefore returns an **already-completed** task, and an
assertion of the shape `waitTask.IsCompleted.Should().BeFalse(...)` placed between creation and
`Advance` fails deterministically. A non-zero due time behaves as expected: the timer stays pending
until `Advance` reaches it.

**Why:** #729 P2-T4. `spec.md` and research 1.4 had read the upstream `WakeWaiters` source and
concluded a due timer fires "on the next advance, not at creation", and pre-authorized only the
*opposite* correction — swapping `Advance(TimeSpan.Zero)` for `Advance(TimeSpan.FromTicks(1))` if the
comparison turned out strict. The observed behaviour was more eager than either branch anticipated,
so the plan's single authorized retry branch was scoped to a different assertion (`Status`, not
`IsCompleted`) and prescribed an edit to a line the failing test never reaches. `WaitAsync(30ms, fake)`
in the same file passed its identical pre-`Advance` assertion, which isolates the cause to the zero
due time rather than to the helper.

**How to apply:** when a plan asserts non-completion before `Advance`, that assertion is only sound
for a **strictly positive** due time. For a zero-delay scenario, assert completion directly after
creation, or drop the pre-assertion for that one test. At preflight, treat any
"FakeTimeProvider fires on the next advance" premise about a `TimeSpan.Zero` due time as unverified
until a run confirms it — reading `WakeWaiters` is not sufficient, because the immediate-fire path is
in `Change`/creation rather than in the advance path. See
[[project_preflight_selfderived_gate_thresholds_are_blind]] for the sibling case of a premise that
only an executed run can falsify.
