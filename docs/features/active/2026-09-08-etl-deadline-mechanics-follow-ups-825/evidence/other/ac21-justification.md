# AC21 — Justification for Removing DoNotParallelize from OlTableExtensions_Tests

Timestamp: 2026-09-09T17-11

RunsObserved: 0
Justification: item 2 change

## Why the attribute is no longer needed

The class has no shared mutable state. It declares no ClassInitialize, TestInitialize, ClassCleanup,
TestCleanup or AssemblyInitialize method, no mutable static field, no Console.SetOut and no captured
TextWriter, no static TimeProvider, and none of Thread.Sleep, Task.Delay, DateTime.Now or Stopwatch.
The usual argument for serialising a test class therefore never applied to it.

The actual hazard was a wall-clock deadline under thread-pool contention. Exactly one test in the
class, GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot, armed a real timed
CancellationTokenSource and then asserted that GetTable was called exactly once, while a genuine
2000 ms deadline governed the Task.Run at UtilitiesCS/Threading/TimeOutTask.cs line 63. Under
class-level parallelism with a saturated pool, a work item not dequeued within 2000 ms would cancel
the linked token and trigger the retry recursion, producing a call count of two and a load-dependent
failure.

The item 2 change removes that hazard rather than tolerating it. That test now supplies a
FakeTimeProvider through the new trailing parameter, and a fake clock that is never advanced never
fires a timer, so no wall-clock deadline governs any test in the class. The attribute has no
remaining justification and was removed.

## Why no run count appears above

RunsObserved is 0 deliberately. Repeated passing runs are not the justification for this removal and
were not used as one. Ten green runs would sample one machine and one suite composition, and the
failure probability of the removed hazard rises with unrelated future test additions and with slower
hardware, so a repeated-run exercise establishes nothing durable about it. Repository policy
separately forbids stabilising a test with a timing tolerance. The justification recorded here is
the code change: the deadline the attribute guarded against no longer exists on this path.

## Ordering

AC22 was checked off at P3-T21, before this attribute was removed at P7-T2, which is the order
spec.md Risks requires: the wall-clock hazard must be shown removed before the attribute that
guarded it is removed. AC21 is checked off at P7-T7, after both.
