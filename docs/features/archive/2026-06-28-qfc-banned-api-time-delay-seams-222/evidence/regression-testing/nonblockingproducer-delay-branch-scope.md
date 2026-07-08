# Regression-Testing Scope Note — NonBlockingProducer 20 ms delay branch (P4-T3)

Timestamp: 2026-06-28T20-05

## Production site
`QuickFiler/Controllers/QfcHomeController.Metrics.cs`, method `NonBlockingProducer(string line, CancellationToken ct)`:

```
do {
    try { success = _metrics.TryAdd(line, 20, ct); }
    catch (OperationCanceledException) {
        if (ct.IsCancellationRequested) break;
        else await TimeProvider.Delay(TimeSpan.FromMilliseconds(20)); // <-- site 8
    }
} while (!success);
```

## Why the exact catch branch is not deterministically reachable
`BlockingCollection<T>.TryAdd(item, millisecondsTimeout, cancellationToken)` throws
`OperationCanceledException` **only** when its `cancellationToken` is canceled. When the OCE is
thrown, `ct.IsCancellationRequested` is therefore `true`, so control takes the `break` path. The
`else await TimeProvider.Delay(...)` branch (OCE raised while the token is NOT canceled) cannot
occur under BlockingCollection semantics; it is defensive code. Driving it would require either a
real cancellation race (non-deterministic, prohibited by the determinism/no-timing-hacks policy) or
modifying production code (out of scope, behavior-preservation mandate).

## What is tested instead (deterministic, no real waits)
`QfcHomeControllerMetricsTests.NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`
proves the controller's injected `TimeProvider` gates the exact production expression
`TimeProvider.Delay(TimeSpan.FromMilliseconds(20))`: with a `FakeTimeProvider`, the 20 ms delay does
not complete until `fake.Advance(TimeSpan.FromMilliseconds(20))`. Combined with the banned-API sweep
(P3-T7) confirming the production line now calls `TimeProvider.Delay` (not `Task.Delay`), this
establishes that, whenever the branch executes, the delay is honored via the injected seam rather
than wall-clock.

## Behavior preservation
Duration unchanged at 20 ms; production default seam is `TimeProvider.System`, so production timing
is identical to the prior `Task.Delay(20)`.
