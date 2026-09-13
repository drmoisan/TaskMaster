# Orchestrator Mechanism Note: Dispatcher Gate Contention (Issue #743)

Timestamp: 2026-09-12T14-05
Collected by: orchestrator (preparation mode)
Method: Read and Grep against the worktree; static reading only, no test execution
EXIT_CODE: 0

This note records a mechanism that is consistent with every measured figure in the issue record. It is
offered as a strongly-supported hypothesis, not as a confirmed finding: nothing here was measured at
runtime, and preparation mode forbids running the suite. The plan must instrument it rather than assume
it.

## The gate

`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 32 declares:

`private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);`

`BeginTransactionAsync` at line 122 awaits `TransactionGate.WaitAsync()` with no timeout and no
cancellation token. The fixture's own doc comment at lines 117-121 states the two-phase shape is
deliberate, that consumers acquire the gate at fixture-build start "well before the install", and that
this preserves the issue #230 hold window.

## The hold window

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` lines 51-55 acquire the
transaction with the comment:

"Held until PumpHarness.Restore, so only one pump fixture owns the static UiThread.Dispatcher at a time
across all test classes in this assembly."

The work performed while the gate is held, in `BuildPumpHarnessCoreAsync` from line 68, includes
constructing a real `QuickFiler.ItemViewer` on the pump thread at line 74, a defensive handle read at
line 84, and the construction of the full mock graph and the production `SaveParameters` path. The
`ItemViewer` constructor runs `InitializeComponent`, which begin-inits and end-inits two WebView2
children, and that is the dominant fixture cost identified in the #729 research.

So the gate is held across the whole fixture build and onward until the harness is restored, not merely
across the reflection write it exists to serialize.

## The contention population is larger than the recorded lead states

The maintainer's lead, inherited from #592, describes contention between two test classes:
`QfcItemController.SeamFactoryTests` and `QfcItemController.InitializationTests`.

A repository-wide Grep for `BeginTransactionAsync` over `*.cs` returns call sites in six distinct test
files:

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (the shared pump-harness builder)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (eight call sites)
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (three call sites)
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`
- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (the declaration itself)

Every pump-hosted test that routes through `BuildPumpHarnessAsync` queues on the same single permit.

## Why this fits the measured signature

The MSTest `[Timeout(PumpTimeoutMs)]` clock is wall-clock from test start. A test blocked in
`TransactionGate.WaitAsync()` is spending its own timeout budget while waiting, even though it is doing
no work. With one permit, a serialized queue, and a per-holder cost dominated by a real WinForms control
tree with two WebView2 children, the waiting time for a test late in the queue scales with the number of
predecessors multiplied by the per-fixture cost.

Under CPU contention every holder's cost rises, so the queue tail crosses 60,000 ms. That predicts
exactly what the record reports, and predicts it better than the falsified handle attribution does:

- Expiry at the timeout bound rather than an exception, because waiting is not faulting.
- Several tests expiring together (the recorded genuine failure was seven expiries), because the whole
  queue tail crosses the bound at once rather than one test failing independently.
- Load sensitivity, because contention inflates the per-holder cost that the queue multiplies.
- Passing in class isolation and in the assembly alone but failing in the combined instrumented
  nine-assembly run, as recorded on #511 for 2026-08-08, because instrumentation overhead inflates the
  same per-holder cost.

## Counter-evidence that substantially weakens this hypothesis

Added 2026-09-12T14-35, after the sections above were written. It is recorded here rather than silently
removing the hypothesis, because the plan must weigh both.

A Grep for `Parallelize` over `QuickFiler.Test/**/*.cs` returns only two `[DoNotParallelize]` class
attributes, at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` line 11 and
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` line 24. There is no `[assembly: Parallelize]`,
which corroborates claim 3 of the #729 research: `QuickFiler.Test` runs serially under the MSTest default.

If tests in the assembly never run concurrently, then no second test is ever waiting at
`TransactionGate.WaitAsync()` while a first holds it, and the queue-wait mechanism above cannot arise in
the ordinary case. The gate would always be free at acquisition.

This does not reduce the hypothesis to zero, but it narrows it sharply. The residual ways it could still
bite are narrower and each must be measured rather than assumed:

- A transaction released late, after its test has formally completed, so the NEXT test's acquisition
  blocks on a predecessor the runner already considers finished. `BuildPumpHarnessAsync` holds until
  `PumpHarness.Restore`, so a path that fails to restore promptly produces exactly this.
- A transaction leaked entirely by a faulted or timed-out test, after which every subsequent acquirer
  blocks forever and expires at its own `[Timeout]`. This would produce a CLUSTER of expiries following
  one initial failure, which matches the recorded signature of seven expiries better than an independent
  per-test cost does.
- The `EnsureDispatcher` path at line 99 of the fixture, whose own doc comment states that disposing the
  returned scope is optional and that "a discarded scope leaks exactly as the pre-fix helper did".

The leaked-transaction variant is the most promising of the three and is the one worth instrumenting
first, because it explains clustering without requiring concurrency.

The competing explanation remains the one the #729 research favours: the real elapsed cost of building a
WinForms control tree with two WebView2 children, inflated by coverage instrumentation and by CPU
contention from OTHER assemblies running in parallel in the combined nine-assembly run. Cross-assembly
parallelism does not contend on this static gate, which lives only in `QuickFiler.Test`, but it does
contend for CPU.

Both explanations predict load sensitivity. They are distinguished by whether failures cluster after a
first failure (favouring the leak) or arrive independently (favouring elapsed cost). The recorded seven
simultaneous expiries is weak evidence for clustering, but it is a single observation.

## Relationship to the maintainer's lead

The lead is stale in its naming and understated in its scope, but it is not wrong in substance. Issue
**#493** did not remove the contention it describes; it centralized every swap behind one owner and, by
holding the gate across the full fixture build to preserve the #230 hold window, gave the serialized
region a longer critical section than the pre-#493 helper had.

That is a hypothesis about a change in degree and it must be measured. It is entirely possible that #493
reduced total contention by eliminating lock-ordering stalls even while lengthening the critical
section. Preparation mode cannot settle it.

**CORRECTION, 2026-09-12T15-05.** An earlier revision of this artifact attributed the replacement to
issue #648 throughout. The correct attribution is **#493**, per the fixture's own doc comment at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:12-13`. Issue #648 only added
`WpfUiDispatcherTests` as a consumer of the already-existing fixture. The orchestrator's original
attribution was inferred from a commit-title grep rather than read from source, which is the error class
this repository's evidence rules exist to prevent.

## Bearing on the fix

If this mechanism is confirmed, an injectable UI-marshalling seam on `ItemViewer` addresses the defect by
removing the need to construct a real `ItemViewer` inside the serialized region for the tests that do not
require one, which shortens the critical section and shrinks the queue. That is a different remedy from
replacing the real message pump, so it does not violate the inherited constraint that forbids a fake
`SynchronizationContext` substituting for the real pump, and it does not introduce a timing tolerance.

The plan must state which tests keep the real pump and which move to the seam, because the inherited
constraint exists precisely to stop the seam from deleting the coverage that the pump-hosted tests
provide.

## Output Summary

`TransactionGate` is a single-permit `SemaphoreSlim` awaited without timeout and held across the entire
pump-harness build and test body, and at least six test files acquire it rather than the two the
inherited lead names. However, `QuickFiler.Test` carries no `[assembly: Parallelize]` and runs serially,
so the simple queue-wait story cannot arise in the ordinary case. The surviving and most promising
variant is a LEAKED or late-released transaction, which would block every subsequent acquirer until its
own `[Timeout]` expires and would explain the recorded cluster of seven simultaneous expiries without
requiring concurrency. The competing explanation, favoured by the #729 research, is raw elapsed fixture
cost inflated by coverage instrumentation and by CPU contention from other assemblies. The two are
distinguished by whether failures cluster after a first failure or arrive independently. Nothing here was
measured at runtime; the plan must instrument to decide between them rather than adopt either.
