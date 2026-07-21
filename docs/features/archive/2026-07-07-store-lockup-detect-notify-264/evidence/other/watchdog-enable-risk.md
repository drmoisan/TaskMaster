# Watchdog Enable — Risk & Rollback (P8-T1)

Timestamp: 2026-07-08T08-30

## Exact change

`TaskMaster/ThisAddIn.cs`, in `ThisAddIn_Startup`: the single caller of `UiThread.Init` was
changed from `UiThread.Init(monitorUiThread: false)` to:

```csharp
UiThread.Init(
    monitorUiThread: true,
    onLockupDetected: attribution => GetStoreLockupResponder()?.OnLockupDetected(attribution),
    timeProvider: TimeProvider.System
);
```

This enables the previously-dormant `ThreadMonitor` watchdog and wires the F4
`StoreLockupResponder.OnLockupDetected` callback and the production clock.

## Risk

This is the only change to existing startup behavior outside the store-processing paths. It
starts one additional background polling loop (a clock-driven timer via `TimeProvider.CreateTimer`)
for the add-in's lifetime. The loop posts a periodic no-op ping to the STA dispatcher and measures
UI responsiveness; the marginal cost is one background timer plus a periodic no-op — the cost F4's
detection depends on paying. The obsolete `Thread.Suspend`/`Thread.Resume` diagnostic stack-capture
path remains gated behind the small `delayThreshold` (Debug-level) and is NOT on the attribution
path, so its fragility cannot delay or prevent auto-disable/notify.

## Rollback

Revert the one call to `UiThread.Init(monitorUiThread: false)`. With the watchdog off, all F4
detection/attribution/notify code remains present but dormant (never invoked), restoring prior
startup behavior without removing any F4 code.
