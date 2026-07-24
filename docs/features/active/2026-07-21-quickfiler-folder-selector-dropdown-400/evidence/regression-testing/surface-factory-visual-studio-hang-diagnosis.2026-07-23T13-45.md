# Surface factory Visual Studio hang diagnosis

- Timestamp: `2026-07-23T13-45Z`
- Command: `Add-Type -AssemblyName System.Windows.Forms; set SynchronizationContext.Current to null; construct System.Windows.Forms.Panel; inspect SynchronizationContext.Current; post one callback; inspect before Application.DoEvents(); call Application.DoEvents(); inspect after`
- EXIT_CODE: `0`
- Output Summary: `Panel construction installed WindowsFormsSynchronizationContext; the posted callback was false before pumping and true after Application.DoEvents.`

## Reported symptom

The user reported that
`SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp()` hangs indefinitely
when run in Visual Studio without coverage. The same case can pass in an isolated CLI
run, so a passing isolated result is not evidence that the Visual Studio execution path
is safe.

## Reproduction

The isolated WinForms probe returned:

```text
BEFORE_CONTEXT=<null>
AFTER_CONTEXT=System.Windows.Forms.WindowsFormsSynchronizationContext
CALLBACK_BEFORE_PUMP=False
CALLBACK_AFTER_PUMP=True
```

This proves that constructing the fixture's `TrackingControl : Panel` can install a
`WindowsFormsSynchronizationContext`, and that work posted to it remains pending until
a WinForms message pump runs.

## Exact hang sequence

1. `SurfaceFactoryFixture` constructs `TrackingControl : Panel`.
2. `NewCompletionSource` uses `TaskCreationOptions.RunContinuationsAsynchronously`.
3. The test calls `initialization.SetException(failure)`, scheduling the production
   failure path asynchronously.
4. The test immediately awaits `CaptureFailure`, and `CaptureFailure` itself awaits
   FluentAssertions without `ConfigureAwait(false)`.
5. If `creating` is incomplete at either await, the continuation captures the installed
   WinForms context.
6. Visual Studio waits for the returned test task, but the test does not pump WinForms
   messages. The continuation therefore remains queued indefinitely.

Coverage changes scheduling and can allow `creating` to fault before continuation
capture. That timing difference explains why coverage and isolated execution can pass;
it does not make the harness deterministic.

## False owner-thread evidence

`RecordingSynchronizationContext.Post` does not marshal to its creator thread. It sets
itself as the ambient context and executes the callback inline on whichever thread called
`Post`. `OperationRecorder` compares only the ambient context reference, so worker-thread
control access is recorded as on-boundary. The test therefore does not prove WinForms
owner-thread affinity.

`SurfaceFactoryFixture` state (`Context`, `Errors`, `Log`, `Control`, and `Messenger`) is
instance-owned. The six nearby static members belong to the outer test class and hold no
mutable static state. Their placement nevertheless obscures fixture ownership and will be
corrected by moving the operation, factory, failure-capture, and completion behavior under
each fixture instance.

## Production disposition

No production edit is indicated:

- `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` uses `ConfigureAwait(false)` for
  control creation, initialization observation, core access, navigation, and failure
  cleanup.
- `BreadcrumbPopupUiOperations.ObserveExternalAsync` uses `ConfigureAwait(false)`,
  reports the initialization failure once, and rethrows it.
- Failure cleanup suppresses the secondary cleanup exception so the original
  initialization exception remains authoritative.

The correction must be test-only: use a queued synchronization context created before
the `Panel`, explicitly drain it on its creator thread, and remove context-capturing async
test continuations. A lone `ConfigureAwait(false)`, `[DoNotParallelize]`, timeout, delay,
retry, test filter, or coverage exclusion would leave the false owner-thread assertion in
place and is prohibited.

