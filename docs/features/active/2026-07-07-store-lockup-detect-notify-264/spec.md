# store-lockup-detect-notify (Spec)

- **Issue:** #264
- **Parent (epic):** #260 (store-lockup-resilience)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature
- **Wave / Dependencies:** wave 2; depends on F1 (#261) and F3 (#263)

## Overview

When a single Outlook `Store` repeatedly locks up the UI thread (transient store-not-ready
HRESULTs, a failing Exchange logon, or expensive per-store COM reads), TaskMaster today has no
mechanism that ties a detected UI stall to a specific store, disables that store to restore
responsiveness, or informs the user. The existing `ThreadMonitor` watchdog can observe UI-thread
stalls and the #211 per-store timing probes localize blocking cost, but neither attributes a stall
to a store nor acts on it.

F4 delivers detection, attribution, immediate auto-disable, and a modeless three-button
notification. It is a pure consumer of two sibling features:

- **F1 (#261)** owns the disable/enable service, exposed on `IApplicationGlobals` as the member
  `StoreDisable` (type `IStoreDisableService`) and the `StoreIdentity` value type.
- **F3 (#263)** owns the runtime rehook. F4 does **not** call F3 directly; F1's `ReenableAsync`
  orchestrates the F3 rehook internally.

F4 authors no F1 or F3 files. It calls only the following F1 service members:

- `DisableSessionOnly(StoreIdentity)` — the auto-disable action and the first notification button.
- `DisableForFutureSessions(StoreIdentity)` — the second notification button (F1 persists).
- `ReenableAsync(StoreIdentity)` — the third notification button (F1 orchestrates the F3 rehook).
- `IsDisabled(StoreIdentity)` — the already-disabled guard (read-only, in-memory).

## Scope

- Extend the existing `UtilitiesCS/Threading/ThreadMonitor.cs` watchdog in place so it can detect
  an extended UI-thread lockup on an injected clock and a configurable, injected attribution
  threshold, and raise an injected callback when that threshold is crossed.
- Enable the watchdog in production by changing `UiThread.Init(monitorUiThread: false)` to
  `true` at `TaskMaster/ThisAddIn.cs:28` (the only caller of `UiThread.Init`).
- Introduce a single-writer/single-reader static volatile current-store context that the STA
  thread sets/clears at the three existing per-store COM entry points and the watchdog thread
  reads on stall.
- On attribution, call F1's `DisableSessionOnly(StoreIdentity)` immediately (to restore
  responsiveness on the next pass), then show a modeless notification.
- Add a modeless composition path to `MyBox` (or a sibling internal helper in
  `UtilitiesCS.Dialogs`), dispatched via `IUiDispatcher.BeginInvoke`, with three buttons wired to
  F1's service.
- Emit one `[store-lockup]` line at WARN so it lands in the existing JSON important-logs
  appender.

## Non-Scope

- No new watchdog class. Detection extends the existing `ThreadMonitor`; a parallel watchdog is
  explicitly rejected (research §2.1).
- No changes to F1's disable service or F3's rehook. F4 consumes both; F1 sequences F3.
- No use of `AsyncLocal` for the store context (research §3.1 — it does not flow to the watchdog's
  independent background thread).
- No new expensive or blocking COM reads on the UI thread. Attribution uses only cheap, already
  cached identity (`DisplayName` via F1's `StoreIdentity`).
- No modal `ShowDialog` for the notification, and no reuse of the existing `MyBox` modal
  `DialogInvoker` seam for the modeless path.
- No new log appender or `log4net.config` change; the existing WARN-filtered `important_logs_file`
  appender already captures a `logger.Warn(...)` line.
- No aborting of a COM call already in flight on the STA. Disabling a store prevents recurrence on
  the next per-store pass; it does not cancel a synchronous COM call already started.
- No modification of files outside this feature's change list. F1/F3 files are consumed only.

## Detection Design (ThreadMonitor extension)

`ThreadMonitor` is currently dormant (`ThisAddIn.cs:28` passes `monitorUiThread: false`), untested,
and drives its polling loop with `Thread.Sleep`, reading no injected clock. This feature necessarily
touches it, so its stall-timing decision moves onto a deterministic, injectable seam.

- **Injected clock.** Inject `System.TimeProvider` (production `TimeProvider.System`; tests
  `FakeTimeProvider`). The production type `System.TimeProvider` is supplied by
  `Microsoft.Bcl.TimeProvider` 10.0.7, already referenced by both `UtilitiesCS` and
  `UtilitiesCS.Test`, so no production dependency changes. `FakeTimeProvider` is supplied by
  `Microsoft.Extensions.TimeProvider.Testing`, which is currently referenced only by
  `QuickFiler.Test`; this feature therefore ADDS `Microsoft.Extensions.TimeProvider.Testing` (9.0.0)
  to `UtilitiesCS.Test`, mirroring the existing `QuickFiler.Test` wiring. This is an already-approved
  in-repo test dependency, not a new external dependency. Elapsed time is computed through the
  provider, not by reading wall-clock time directly, replacing the `Thread.Sleep`-based
  polling/backoff timing with clock-driven elapsed-time checks. This is also what makes
  `ThreadMonitor` unit-testable for the first time.
- **Configurable attribution threshold.** Add a second, larger, constructor-injected
  `lockupAttributionThresholdMs`, distinct from the existing small `delayThreshold` used for the
  diagnostic stack-trace cadence. No hardcoded literal; the default is a planning decision. The
  existing diagnostic stack-capture path (`Thread.Suspend`/`Thread.Resume`, obsolete APIs) stays
  gated behind the original `delayThreshold` and is not on the attribution path, so its fragility
  cannot delay or prevent auto-disable/notify.
- **Injected callback.** Add an `Action<LockupAttribution> onLockupDetected` callback invoked when
  `lockupAttributionThresholdMs` is crossed. `LockupAttribution` carries the stall duration and the
  store identity read from the current-store context (below). `ThreadMonitor` does not perform the
  disable/notify logic itself; it only detects and measures.
- **Pure decider.** Extract the elapsed-ms-in / stall-confirmed-out decision into a small pure
  helper (`LockupStallDecider`), following the repository's `StartupLifetimeStopDecider` split, so
  the threshold logic is unit-testable without a live `Dispatcher` or thread. The live polling loop
  (`Task.Run`, `Dispatcher`, `Thread`) stays a thin, host-bound shell.
- **Callback thread.** The callback runs on the watchdog's own background `Task.Run` thread — never
  the STA — so invoking it adds no new blocking on the UI thread.

## Attribution Mechanism (static volatile current-store context)

Because all per-store COM work runs synchronously, in sequence, on the single STA/UI thread, and
the watchdog reads from an unrelated background thread, the attribution mechanism is a
single-writer/single-reader static holder with volatile visibility — **not** `AsyncLocal`, which
cannot flow to the watchdog thread.

- **Shape.** A new `UtilitiesCS/Threading/CurrentStoreContext.cs`: a static class backed by a
  `private static volatile string _current`, exposing `Current` (read) and
  `Begin(string storeIdentity)` returning an `IDisposable` that restores the previous value on
  `Dispose`. Host-neutral; no COM dependency.
- **Single writer, single reader.** The STA thread is the only writer; the watchdog background
  thread is the only reader. A `volatile` reference field provides the required cross-thread
  visibility for a single reference read/write without a lock.
- **Set/clear points.** The context is opened immediately after the existing cheap `DisplayName`
  read at each site and wraps only the subsequent, more expensive calls, using
  `using (CurrentStoreContext.Begin(displayName)) { ... }`:
  1. `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` — `Init()`: wrap the block from
     `GetRootFolder()` through `GetSmtpAddressFromStore()` (the post-`DisplayName` blocking chain).
  2. `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — `RewireOlObjectsAsync`: wrap the
     `storeWrapper.Init()` / `storeWrapper.Restore(store)` call in a scope keyed on the
     already-read `storeDisplayName`. The scope opens and closes within one loop iteration, after
     the loop's `await Task.Yield()`, so no value leaks across the yield into the next iteration.
  3. `TaskMaster/AppGlobals/AppOlObjects.cs` — the per-store attribution point in
     `EmitPerStoreInboxAttribution`: wrap the `getDefaultFolder()` call in a scope keyed on the
     already-computed `displayName`.
- **Reading on stall.** The `ThreadMonitor` callback reads `CurrentStoreContext.Current` at the
  moment the attribution threshold is confirmed crossed. This is a plain in-memory field read — no
  COM, no blocking.
- **Cheap identity only.** `CurrentStoreContext` only ever receives a `string` that the call site
  already computed for its own #211 diagnostics. No new COM property read is introduced.
- **File-size constraint (AppOlObjects.cs).** `AppOlObjects.cs` is 525 lines, already over the
  repository's 500-line ceiling independent of F4. The set/clear edit there is a minimal (1–3 line)
  in-place wrap, but it touches an already-over-limit file. The edit must respect the partial-file
  split introduced by F2/F3 (F2 adds `AppOlObjects.StoreLoading.cs`); if the atomic plan requires
  the file to return under the ceiling, extract an unrelated cohesive slice into a partial-class
  file rather than folding the wrap into a larger refactor.

## Auto-Disable-Then-Notify Sequence

A small orchestrator (`StoreLockupResponder`, host-neutral aside from its injected dependencies)
is invoked by the `ThreadMonitor` callback. Its dependencies are all interface/delegate seams
(F1's `IStoreDisableService`, an `IUiDispatcher`, the modeless-notify composition, and a logging
sink), so it is Moq/FluentAssertions-testable without Outlook. The confirmed order is disable
first, then notify:

1. The watchdog confirms the attribution threshold is crossed and reads
   `CurrentStoreContext.Current`.
2. **Guard — no context.** If `Current` is null/empty (or a normalized identity-unavailable value),
   do nothing: no disable, no notify, no attributed `[store-lockup]` WARN line. A lower-severity
   diagnostic distinct from the attributed line is optional.
3. **Guard — already disabled.** Call F1's `IsDisabled(StoreIdentity)`. If the store is already
   disabled, skip both the disable call and the notification (idempotency / no duplicate
   notifications).
4. **Auto-disable.** Call F1's `DisableSessionOnly(StoreIdentity)` synchronously on the watchdog's
   background thread. Per F1's contract this is a pure in-memory state change (no COM), so it is
   safe off the STA. This is the responsiveness-restoring step: it prevents the store from being
   retried on the next per-store pass.
5. **Log.** Emit one `[store-lockup]` line at WARN through the injected sink, carrying the store
   identity, stall duration, and auto-disable outcome, so it lands in the JSON `important_logs_file`
   appender with no config change.
6. **Notify.** Marshal a request to show the modeless message onto the STA via
   `IUiDispatcher.BeginInvoke` (fire-and-forget) — never `Invoke`. This does not block the watchdog
   thread and does not require the STA to be idle immediately; the queued item runs when the STA
   next services its dispatcher queue (i.e. when the UI becomes responsive again).

Responsiveness note: disabling a store stops it from being retried on subsequent passes; it does
not abort a synchronous COM call already in flight on the STA (no supported interop mechanism can
cancel one). "Restores responsiveness" therefore means "prevents recurrence on the next pass," and
user-facing copy is worded accordingly.

## Modeless Notification Composition and Three-Button Wiring

`MyBox` has no modeless path today: its convenience `ShowDialog` overloads wrap the viewer in a
`using` block and route through the modal `DialogInvoker` seam (production default
`viewer => viewer.ShowDialog()`), which would dispose the form immediately if repointed to a
non-blocking `Show()`. The `DialogInvoker` `AsyncLocal` seam is a test-only isolation for the modal
API, not a production modal/modeless switch, and is not reused here.

- **New modeless composition.** Add an internal, non-`using`-scoped modeless composition path
  inside the `UtilitiesCS` assembly (a new method on `MyBox` or a sibling internal helper in
  `UtilitiesCS.Dialogs`, because `MyBox.ReplaceButtons` and `AppendButtonInColumn` are
  `internal static` and `UtilitiesCS` does not grant `InternalsVisibleTo` to `TaskMaster`;
  `ActionButton` itself is `public` (`UtilitiesCS/Dialogs/ActionButton.cs:13`), but the composition
  must still live in-assembly because the button-wiring helpers are internal). The
  existing modal `ShowDialog`/`DialogInvoker` overloads are not modified.
- **Viewer lifetime.** Construct a `MyBoxViewer` directly (not via the `using` convenience
  overloads) and own its lifetime through a `FormClosed` handler (`viewer.Dispose()` on close), so
  the form stays on screen until the user clicks a button.
- **Injectable show seam.** Show the viewer through an injectable `Action<MyBoxViewer> showAction`
  defaulting to `viewer => viewer.Show()`, mirroring `EfcHomeController.ViewerShowAction`
  (`QuickFiler/Controllers/EfcHomeController.cs:294-297`). Tests substitute a non-displaying stub.
- **Dispatch.** The orchestrator marshals the show request through `IUiDispatcher.BeginInvoke`, so
  tests can supply a synchronous pass-through mock and assert the show action was invoked without a
  live STA message pump.
- **Button wiring.** Buttons use the same `ActionButton` building blocks `MyBox` uses internally;
  `ActionButton.Button_Click` invokes the supplied `Action` directly and is independent of
  modality. The three buttons map to F1's service:
  - "Disable This Session Only" → `DisableSessionOnly(StoreIdentity)`.
  - "Disable for Future Sessions" → `DisableForFutureSessions(StoreIdentity)`.
  - "Reenable" → `ReenableAsync(StoreIdentity)`. F4 makes a single call; F1's `ReenableAsync`
    clears the disablement and orchestrates F3's rehook. F4 does not call F3 directly.
- **Message text.** Use the cached identity string from the attribution directly (the same
  `DisplayName` already used by `StoreFilterAttribution.FormatLine`), for example: "The mailbox
  '{identity}' has stopped responding and has been disabled for this session." No additional COM
  read is performed to improve the display string.
- **Line formatter.** The `[store-lockup]` WARN line is produced by a pure formatter
  (`StoreLockupAttribution`, mirroring `StoreFilterAttribution.FormatLine`) with no
  COM/clock/Dispatcher dependency.

## Guardrails

- **No context → no attribution.** A stall with `CurrentStoreContext.Current == null` calls no
  disable and shows no notification. Satisfies "no false attribution when no store context is
  active."
- **No duplicate notification.** The `IsDisabled` guard before disable and notify prevents a second
  disable call and a duplicate notification for a store that keeps re-entering the per-store loop.
- **Identity unavailable.** Two of the three set/clear sites already guard `DisplayName` reads and
  yield `"<unavailable>"`/null. If `StoreWrapper.Init`'s unguarded `DisplayName` read throws, no
  scope opens and `Current` stays at its prior value (null), which is the same "no context" guard,
  not a watchdog crash. `CurrentStoreContext.Begin` normalizes a null or `"<unavailable>"` identity
  to "no context" for auto-disable/notify purposes; whether `"<unavailable>"` is itself a
  persistable identity is an F1 decision, not F4's.
- **Watchdog adds no new stall.** Every background-thread step (`IsDisabled`, `DisableSessionOnly`,
  the WARN log, `BeginInvoke`) is non-blocking/in-memory by construction; nothing in the
  orchestrator calls back into Outlook COM.

## Determinism and Testability

Consistent with the General and C# Unit Test Policies (MSTest + Moq + FluentAssertions; no live
Outlook; no temporary files; no `Thread.Sleep`/`Task.Delay`/real timers in tests) and the
repository's coverable-decision / thin-host split:

- **Clock and threshold.** Inject `TimeProvider` into the pure `LockupStallDecider`; assert
  threshold-crossing by advancing a `FakeTimeProvider` — no real waits. Assert the lockup callback
  fires exactly when `lockupAttributionThresholdMs` is exceeded and not before.
- **Attribution.** Unit-test `CurrentStoreContext.Begin`/`Current` directly (value set inside the
  scope, reverted after `Dispose`, tolerant of nested/sequential scopes) with no COM or threading
  dependency.
- **Orchestrator (`StoreLockupResponder`).** Moq the `IStoreDisableService`, the `IUiDispatcher`
  (or the notify delegate), and the logging sink. Assert per scenario:
  - Threshold crossed + valid identity + not disabled → `DisableSessionOnly` called once, then the
    notify delegate invoked once, in that order (Moq `MockSequence` or captured call order).
  - No identity in context → neither `DisableSessionOnly` nor the notify delegate is called.
  - Already-disabled identity (`IsDisabled` returns true) → neither call happens a second time.
  - Each of the three notification buttons invokes the correct F1 call
    (`DisableSessionOnly` / `DisableForFutureSessions` / `ReenableAsync`) — construct the button
    action set directly and invoke each `Action` to assert the downstream mock call, without a real
    `Show()`.
  - The notify path is dispatched through `BeginInvoke` / the injected show-action seam, not
    `Invoke`, asserted on the seam without a live STA pump.
- **Modeless show.** Assert the injectable `showAction` seam is invoked (non-displaying stub); the
  real `viewer.Show()` is never called in a test.
- **Logging.** Assert the `[store-lockup]` line format via the pure `StoreLockupAttribution`
  formatter (string assertion, no log4net dependency), and separately assert at the orchestrator
  level that the sink is invoked with WARN-equivalent semantics.
- **No external dependencies.** None of the above requires a live `Application`, `Store`, or
  `Dispatcher`; `IUiDispatcher` is mocked, `CurrentStoreContext` is a plain static, and
  `TimeProvider`/`FakeTimeProvider` replace all timing.

## Acceptance Criteria

These refine the early-draft criteria in `issue.md` into numbered, testable statements.

- [ ] **AC1 — Detection on an injected clock and threshold.** `ThreadMonitor` computes elapsed
      stall time through an injected `TimeProvider` and raises an injected
      `Action<LockupAttribution> onLockupDetected` callback exactly when the injected, configurable
      `lockupAttributionThresholdMs` is exceeded and not before, verified by a deterministic MSTest
      advancing a `FakeTimeProvider`. The existing diagnostic stack-capture path remains gated
      behind the unchanged `delayThreshold` and is not on the attribution path.
- [ ] **AC2 — Watchdog enabled in production.** `TaskMaster/ThisAddIn.cs` calls
      `UiThread.Init(monitorUiThread: true)`, and `StoreLockupResponder`'s dependencies (F1's
      `StoreDisable` from `IApplicationGlobals`, an `IUiDispatcher`, the notify composition) are
      wired at startup.
- [ ] **AC3 — Attribution via static volatile context.** `CurrentStoreContext` is a
      single-writer/single-reader static holder; `Begin(identity)` sets the value inside an
      `IDisposable` scope and restores the previous value on `Dispose`. The three set/clear points
      (`StoreWrapper.Init`, `StoresWrapper.RewireOlObjectsAsync`, `AppOlObjects` per-store
      attribution) wrap only the post-`DisplayName` blocking calls using the already-cached
      `DisplayName`. Verified by a deterministic MSTest on `CurrentStoreContext` (no COM, no
      threads).
- [ ] **AC4 — No new expensive/blocking COM calls.** Attribution and identification introduce no
      new COM property reads on the UI thread; `CurrentStoreContext` receives only strings already
      computed by existing #211 diagnostics. The existing `[Startup timing]` / `[store-filter]` /
      `[loadinboxes]` lines are unchanged (additive, behavior-preserving wraps).
- [ ] **AC5 — Auto-disable immediately, then notify.** On attribution with a valid identity for a
      not-already-disabled store, `StoreLockupResponder` calls F1's `DisableSessionOnly(identity)`
      once and then shows the modeless notification once, in that order, verified by a deterministic
      MSTest asserting call order.
- [ ] **AC6 — Modeless three-button notification.** The notification is composed without a `using`
      block, owns its viewer lifetime via `FormClosed`, is shown through an injectable
      `showAction` seam defaulting to `viewer => viewer.Show()`, and is dispatched via
      `IUiDispatcher.BeginInvoke` (never `Invoke`, never modal `ShowDialog`). Its three buttons
      invoke F1's `DisableSessionOnly`, `DisableForFutureSessions`, and `ReenableAsync`
      respectively, verified by invoking each button `Action` against Moq without a real `Show()`.
      F4 makes no direct F3 call.
- [ ] **AC7 — Guard: no context.** A stall with `CurrentStoreContext.Current` null/empty (including
      a normalized identity-unavailable value) triggers no disable and no notification, verified by
      a deterministic MSTest.
- [ ] **AC8 — Guard: already disabled.** A stall attributed to a store for which F1's `IsDisabled`
      returns true triggers no second disable and no duplicate notification, verified by a
      deterministic MSTest.
- [ ] **AC9 — WARN logging.** One `[store-lockup]` line is emitted at WARN with store identity,
      stall duration, and auto-disable outcome through the injected sink, so it lands in the
      existing JSON `important_logs_file` appender with no config change. The line format is
      produced by the pure `StoreLockupAttribution` formatter and asserted by a deterministic
      MSTest with no log4net dependency.
- [ ] **AC10 — Determinism and toolchain.** All new/extended code is covered by deterministic
      MSTest (Moq + FluentAssertions) with injected clock/threshold and mocked
      watchdog/service/dispatcher/notify seams; no live Outlook, no temporary files, no real
      waits/timers. The full C# toolchain passes in order (CSharpier → analyzers →
      nullable/TreatWarningsAsErrors → MSTest with coverage, `TestCategory!=LiveOutlook`); new code
      meets the coverage policy with no repository-wide regression; all touched/new files are
      <= 500 lines (the `AppOlObjects.cs` over-cap constraint handled per the partial-file split).
