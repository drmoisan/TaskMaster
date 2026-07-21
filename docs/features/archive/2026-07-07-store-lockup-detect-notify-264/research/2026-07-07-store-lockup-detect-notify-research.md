# Research: store-lockup-detect-notify (Issue #264)

- Date: 2026-07-07
- Epic: store-lockup-resilience (#260), feature F4, wave 2, depends on F1 (#261) and F3 (#263)
- Scope: detection of an extended, error-driven UI-thread lockup; attribution to the store
  currently being processed using only cheap cached identity; immediate session-scope
  auto-disable; modeless three-button notification. Read-only research; no source changes made.

## 1. Current State Analysis

### 1.1 Detection primitive: `ThreadMonitor`

`UtilitiesCS\Threading\ThreadMonitor.cs` is the only existing UI-stall watchdog.

- Constructed with `(Thread thread, int pollingFrequency = 500, int delayThreshold = 100, int
  stackTraceIterations = 4)` (`ThreadMonitor.cs:24-35`).
- `Run()` (`:37-76`) starts a `Task.Run` loop that never exits: `Thread.Sleep(pollingFrequency)`,
  then posts a no-op `dispatcher.InvokeAsync(() => { })` to the monitored thread's `Dispatcher`.
  It then polls up to `stackTraceIterations` times, `Thread.Sleep(delayThreshold)` each time,
  checking `task.Status`. If the no-op never completes within `pollingFrequency +
  stackTraceIterations * delayThreshold` (default 500 + 4*100 = 900 ms), it captures a stack
  trace of the target thread via `Thread.Suspend()`/`Thread.Resume()` (`:79-118`, obsolete APIs,
  `#pragma warning disable 0618`) and logs it at `Debug`, not `Warn`.
- Wired from `UiThread.Init(monitorUiThread:)` (`UiThread.cs:46-50`), which only constructs and
  runs it when `monitorUiThread == true`. `TaskMaster\ThisAddIn.cs:28` calls
  `UiThread.Init(monitorUiThread: false)` today — the watchdog is **not currently running** in
  production.
- No unit tests exist for `ThreadMonitor` (`UtilitiesCS.Test\Threading\` glob for
  `ThreadMonitor*Test*.cs` returned no files) — it is presently untested and, given its
  `Thread.Sleep`-driven polling loop and its live `Dispatcher`/thread-suspend dependencies, is not
  testable in its current form.
- The polling loop uses `Thread.Sleep` directly (a banned API per `csharp.md`, currently at
  `suggestion` severity so it does not fail the build) and reads no injected clock. Per the C#
  toolchain guidance, new/touched time-dependent code should inject a clock; `ThreadMonitor` is
  necessarily touched by this feature, so its stall-timing decision must move onto an injectable,
  deterministic seam (see §2).

### 1.2 Per-store cheap-identity read points (attribution surface)

Three call sites read a store's `DisplayName` cheaply as (or near) the first COM read in a
per-store loop, then proceed to progressively more expensive/blocking COM calls for that same
store, all synchronously on the STA/UI thread:

1. **`StoreWrapper.Init()`** (`UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:27-75`):
   `DisplayName = InnerStore.DisplayName` (`:36`) is the very first read, timed by its own
   Stopwatch. It is immediately followed by `GetRootFolder()` (`:42`), conditionally
   `GetDefaultFolder(olFolderInbox)` (`:52`), and `GetSmtpAddressFromStore()` (`:60`, itself a
   chain of `CurrentUser` → `AddressEntry` → `GetExchangeUser()` → `PrimarySmtpAddress`,
   `:146-184`). A slow/failing Exchange logon manifests as a long stall inside this chain, all
   attributable to the `DisplayName` read at `:36`.
2. **`StoresWrapper.RewireOlObjectsAsync`** (`UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs:83-127`):
   reads `store.DisplayName` (`:102`) before calling `storeWrapper.Init()` or
   `storeWrapper.Restore(store)` (`:108/114`), inside a `foreach` over `GetFilteredStores()` with
   an `await Task.Yield()` between iterations (`:96-99`).
3. **`AppOlObjects.LoadInboxes()` / `EmitPerStoreInboxAttribution`** (`TaskMaster\AppGlobals\AppOlObjects.cs:124-240`):
   `readDisplayName()` (a guarded `store.DisplayName` read, `:144-152`, folded into
   `EmitPerStoreInboxAttribution` at `:211`) runs before `getDefaultFolder()` (`:229`, i.e.
   `store.GetDefaultFolder(olFolderInbox)`).

All three sites already exist purely to support the #211 per-store timing/attribution diagnosis
(`StoreFilterAttribution`, `StartupInboxAttributionProbe`, `StoreWrapperInitProbe`) — they read
`DisplayName` first specifically because it is cheap and rarely throws, and everything after it is
the expensive/blocking part. This is the existing, already-proven pattern F4 should extend rather
than replace.

Because all three loops execute on the single STA/UI thread (COM objects are apartment-affine;
`RewireOlObjectsAsync`'s `await Task.Yield()` resumes via the captured `UiSyncContext`, so it stays
on the same thread), there is at most one store "in flight" at any instant, and it is always the
UI thread doing the work. `IStoreDisableService` (F1) operates purely on an in-memory identity list
and is not itself COM-bound, so it is safe to invoke off that thread.

### 1.3 Modeless notification primitive: `MyBox`

`UtilitiesCS\Dialogs\MyBox.cs` has **no existing modeless code path**. Every `ShowDialog` overload
(the `Dictionary<string, Action>` overload at `:139-149` is the one referenced by the issue)
ultimately calls `DialogInvoker(viewer)` (`:28-43`), whose production default is
`viewer => viewer.ShowDialog()` — a **modal**, blocking call. The `AsyncLocal<Func<MyBoxViewer,
DialogResult>>` seam exists solely so `UtilitiesCS.Test` can inject a non-blocking stub during
tests (`UtilitiesCS.Test\Dialogs\MyBox_ShowDialog_Tests.cs`); it is not a production
modal/modeless switch. Additionally, the convenience overload wraps the viewer in a `using`
block (`MyBox.cs:146`: `using MyBoxViewer viewer = new();`) — if `DialogInvoker` were repointed to
a non-blocking `viewer.Show()` for production use, the `using` block would `Dispose()` the form
immediately after `Show()` returns, while it is still on screen. **`MyBox.ShowDialog(...)` must
not be reused as-is for F4's modeless requirement**; see §4 for the recommended approach.

Button wiring is decoupled from modality: `ActionButton.Button_Click` (`ActionButton.cs:181-184`)
invokes the supplied `Action` directly from the WinForms `Click` event, independent of whether the
form was shown via `ShowDialog()` or `Show()`. This means the three-button wiring mechanism itself
is reusable for a modeless viewer; only the *invocation* of the viewer needs a different, injectable
path.

The existing modeless pattern in this repository is `EfcHomeController`'s injectable show-action
seam (`QuickFiler\Controllers\EfcHomeController.cs:294-297`):

```csharp
internal Action<EfcViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();
internal Func<EfcViewer, Task> ViewerShowAsyncAction { get; set; } =
    async viewer => await UiThread.Dispatcher.InvokeAsync(() => viewer.Show());
```

This is the pattern F4 should mirror: a small, purpose-built `Action<MyBoxViewer>`/`Func<...>`
seam defaulting to `viewer => viewer.Show()`, not the shared `DialogInvoker` (which is a test-only
seam for the *modal* API and is process-global/`AsyncLocal`, not owned by the notification call
site).

### 1.4 UI-thread marshalling

`UtilitiesCS\Threading\IUiDispatcher.cs` / `WpfUiDispatcher.cs` provide the narrow, mockable
dispatch seam already used elsewhere: `Invoke` (blocking), `InvokeAsync` (awaitable), `BeginInvoke`
(fire-and-forget, non-blocking), `InvokeAsync<TResult>`. `WpfUiDispatcher.BeginInvoke` forwards to
`UiThread.Dispatcher.BeginInvoke(action).Task` (`WpfUiDispatcher.cs:32-33`). `BeginInvoke` is the
correct primitive for F4's notify step: it enqueues work on the STA thread's dispatcher queue
without blocking the caller (the watchdog's background thread), and it does not require the STA
thread to be idle *right now* — the queued item runs as soon as the dispatcher can service it,
which is exactly the moment the UI thread becomes free again (whether that is "immediately" for a
merely-slow call or "after a long stall" for a genuinely blocked one).

`QuickFiler\Helper Classes\EmailMoveMonitor.cs:29-40` shows the repo's established
injectable-delegate seam for marshalling to STA: `private readonly Action<System.Action>
_marshalToSta;` defaulting to `action => UiThread.Dispatcher.Invoke(action)`, with tests
supplying `a => a()`. F4's disable+notify orchestrator should use the same
constructor-default-delegate style, but for the *notify* half it must default to a **non-blocking**
dispatch (`BeginInvoke`/`InvokeAsync`, not `Invoke`), since the whole point is not to add a second
blocking hop on top of an already-stalled UI thread.

### 1.5 Logging

`TaskMaster\log4net.config:53-70`: the `important_logs_file` appender has a
`LevelRangeFilter` with `LevelMin=WARN` and a JSON `SerializedLayout`. A single `logger.Warn(...)`
call at the point of attribution/auto-disable lands in that file automatically; no new appender or
config change is needed. The repo's existing structured-line convention (`[store-filter]`,
`[loadinboxes]`, `[ui-heartbeat]`, `StoreFilterAttribution.FormatLine`) should be followed for the
new `[store-lockup]` line: a pure formatter class with no COM/clock/Dispatcher dependency,
consistent with `StoreFilterAttribution` and `StartupDiagnosticsProbe`.

### 1.6 F1 / F3 contracts assumed (both specs are still Draft; only the method/behavior list is
fixed, not final signatures)

- F1 (`docs/features/active/2026-07-07-store-disable-service-261/spec.md`): `IStoreDisableService`
  exposed on `IApplicationGlobals`, with `DisableSessionOnly(identity)`,
  `DisableForFutureSessions(identity)`, `Reenable(identity)`, `IsDisabled(identity)`,
  `GetDisabledStores()`. Identity is `DisplayName` with a documented fallback. `IApplicationGlobals`
  (`UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs`) does not yet declare this member; F4
  cannot compile against it until F1 lands, confirming the epic's wave-2 dependency ordering.
- F3 (`docs/features/active/2026-07-07-store-runtime-reenable-263/spec.md`): a runtime rehook
  operation "exposed behind an interface the disable service (F1), the notification (F4), and the
  settings UI (F5) can call." No concrete interface/method name is fixed yet; this research refers
  to it generically as **F3's rehook operation** (e.g. `IStoreRehookService.RehookAsync(identity)`)
  and flags the exact name as an F3 integration detail to confirm at F4 implementation time, not a
  fact verified in this codebase today.

## 2. Detection Design

### 2.1 Candidate approaches

**A. Extend `ThreadMonitor` in place.** Add an injected clock and a second, larger
"lockup-attribution" threshold alongside the existing diagnostic `delayThreshold`, and call an
injected `Action<string, TimeSpan>` (identity, stall duration) callback when the larger threshold
is crossed, instead of only logging a stack trace.

**B. Build a new, separate watchdog class purpose-built for attribution** (e.g.
`StoreLockupWatchdog`), leaving `ThreadMonitor` untouched, and run it alongside (or instead of)
`ThreadMonitor`.

**Recommendation: A, with the stall-duration decision extracted into a small pure/coverable
class**, following the repo's established split (`StartupLifetimeStopDecider` / `StartupDiagnosticsProbe`
pattern, `ApplicationGlobals.cs:126-161` + `TaskMaster\AppGlobals\StartupDiagnosticsProbe.cs`):
the live polling loop (Task.Run, Dispatcher, Thread) stays a thin, `[ExcludeFromCodeCoverage]`-eligible
or COM/host-bound host, and a new pure decider (e.g. `LockupStallDecider`) takes elapsed-ms inputs
and returns "stall confirmed" so the decision itself is unit-testable without a live Dispatcher or
thread. Building a second independent watchdog (B) would duplicate the ping-and-measure mechanism
that already exists and increase the number of places a "how long has the UI been unresponsive"
policy has to be kept consistent; extending `ThreadMonitor` reuses the one existing, wired-up
polling primitive and lets the current diagnostic stack-trace-on-stall behavior remain (now backed
by the same measurement).

### 2.2 Concrete extension shape

- Replace the `Thread.Sleep`-based polling/backoff with an injected clock abstraction
  (`System.TimeProvider`, per `csharp.md`'s time-seam guidance — `Microsoft.Bcl.TimeProvider` is
  already referenced by `UtilitiesCS.Test.csproj`) so elapsed time is computed from
  `TimeProvider.GetUtcNow()`/a `Stopwatch`-equivalent obtained through the provider, not read
  directly. Tests then use `FakeTimeProvider` to advance time deterministically instead of a real
  `Thread.Sleep` loop — this is also required to make `ThreadMonitor` unit-testable at all, since
  today it has zero test coverage.
- Add a second, larger, configurable threshold — e.g. `lockupAttributionThresholdMs` (constructor
  parameter, no hardcoded literal) — distinct from the existing `delayThreshold` used for the
  stack-trace-capture cadence. The issue's "extended" lockup is materially longer than the existing
  ~900 ms diagnostic window; the exact default should be a configuration decision made during
  planning, not invented here, but it must be injectable/configurable per the acceptance criteria
  ("configurable/injected threshold and clock").
- On crossing `lockupAttributionThresholdMs` (as decided by the new pure decider), invoke an
  injected callback, e.g. `Action<LockupAttribution> onLockupDetected`, where `LockupAttribution`
  carries the stall duration and the store identity read from the ambient context (§3). Do **not**
  perform the disable/notify logic inside `ThreadMonitor` itself — keep `ThreadMonitor` focused on
  detection/measurement (single responsibility) and let a separate orchestrator (§3.3) own the
  disable-then-notify sequence. This mirrors the existing separation between `ThreadMonitor`
  (detection) and `StoreFilterAttribution`/`StartupInboxAttributionProbe` (pure decision/formatting).
- The callback executes on the watchdog's own background `Task.Run` thread — the same thread that
  already does the polling — never on the STA thread, so invoking it introduces no new blocking on
  the UI thread.

### 2.3 Should `monitorUiThread` be enabled?

Today `ThisAddIn.cs:28` passes `monitorUiThread: false`, so `ThreadMonitor` does not run at all in
production. Enabling it is a prerequisite for this feature's detection acceptance criterion. The
risk/tradeoff, grounded in the code read above:

- **Cost:** the watchdog's own polling loop runs continuously for the add-in's lifetime (a
  `Task.Run` with a `while (true)`), consuming one ThreadPool/background thread and periodically
  posting a no-op `InvokeAsync` to the STA dispatcher. This is a small, constant background cost,
  already accepted as a design in the existing (currently dormant) code.
  Since F4's design also uses that same polling cadence to drive the lockup-attribution threshold,
  the marginal cost of turning it on is the cost this feature depends on paying.
- **Risk — accuracy under this feature's new higher threshold:** the existing `GetStackTrace`
  method uses `Thread.Suspend()`/`Thread.Resume()` (obsolete, deprecated .NET APIs that can produce
  inconsistent/aborted state if the suspended thread is mid-GC or mid-JIT). F4 does not need this
  stack-capture path for attribution (identity comes from the ambient context in §3, not from the
  stack trace), so the risk is containable: gate the existing `GetStackTrace` call behind the
  original small `delayThreshold` (unchanged, diagnostic-only) and drive the new
  disable/notify callback off the separate, larger `lockupAttributionThresholdMs` decision, so a
  failure or slowness in the (already fragile) stack-capture path cannot delay or prevent the
  auto-disable/notify path.
- **Recommendation:** enable `monitorUiThread: true` in `ThisAddIn.cs` as part of this feature
  (it is currently the only caller of `UiThread.Init`), since F4's acceptance criteria require a
  live watchdog; document the two independent thresholds (diagnostic stack-trace threshold vs.
  lockup-attribution threshold) so a future contributor does not conflate them.

## 3. Attribution Design

### 3.1 Rejected: `AsyncLocal`

The delegation prompt anchors suggest an "AsyncLocal or an explicit scope." `AsyncLocal<T>`
flows a value along one logical call/await chain; it is not a cross-thread signal to an
independent, unrelated `Task.Run` loop that never awaited anything from within the STA thread's
execution. `ThreadMonitor`'s polling loop runs on its own background thread that has no
async/await relationship to the STA thread's synchronous COM-call sequence — it would not observe
an `AsyncLocal` value set by the STA thread. (This is also why `MyBox`'s existing `AsyncLocal`
seam is scoped to same-flow test-parallelism isolation, not cross-thread production signaling —
confirmed by its doc comment in `MyBox.cs:24-27`.) `AsyncLocal` is therefore not the correct
mechanism for this feature and should not be used for the ambient store-identity context.

### 3.2 Recommended: an explicit, thread-visible ambient scope

Because all per-store COM work happens synchronously, in sequence, on the single STA/UI thread
(§1.2), a plain static holder with volatile/interlocked semantics is sufficient and simplest:

```csharp
// UtilitiesCS/Threading/CurrentStoreContext.cs (new, host-neutral, no COM dependency)
public static class CurrentStoreContext
{
    private static volatile string _current;

    public static string Current => _current;

    // Returns an IDisposable that restores the previous value on Dispose, so nested/adjacent
    // scopes (defensive, not expected given the single-STA-thread invariant) do not corrupt state.
    public static IDisposable Begin(string storeIdentity) { /* set _current, capture previous */ }
}
```

- **Single writer, single reader:** the STA/UI thread is the only writer (it is the only thread
  that ever performs the per-store COM sequence); the watchdog's background thread is the only
  reader. A `volatile string` field gives the required cross-thread visibility for a single
  reference read/write without a lock.
- **Set/clear locations** (extending the three sites identified in §1.2), immediately after the
  existing cheap `DisplayName` read and wrapping only the subsequent, more expensive calls:
  - `StoreWrapper.Init()` (`StoreWrapper.cs:36`): wrap the block from `RootFolder = ...` (`:42`)
    through the end of `GetSmtpAddressFromStore()` (`:60-63`) in
    `using (CurrentStoreContext.Begin(DisplayName)) { ... }`.
  - `StoresWrapper.RewireOlObjectsAsync` (`StoresWrapper.cs:102-115`): wrap the
    `storeWrapper.Init()` / `storeWrapper.Restore(store)` call in a scope keyed on the already-read
    `storeDisplayName` (`:102`). Because the loop's `await Task.Yield()` (`:98`) happens *before*
    the scope opens for the next iteration (scope is opened and closed within one iteration, not
    held across the yield), there is no risk of the ambient value leaking into the next iteration's
    unrelated await point.
  - `AppOlObjects.LoadInboxes()` / `EmitPerStoreInboxAttribution` (`AppOlObjects.cs:211-239`): wrap
    the `getDefaultFolder()` call (`:229`) in a scope keyed on the already-computed `displayName`
    (`:211`). Since `EmitPerStoreInboxAttribution` is `internal static` and COM-free (identity and
    the folder come through injected delegates), the scope can be opened either inside this method
    around the `getDefaultFolder()` invocation, or in the caller `LoadInboxes()` around each loop
    iteration — the former is preferred because it keeps the scope tightly bound to the one
    blocking call it protects.
- **No expensive reads added:** `CurrentStoreContext` only ever receives a `string` that a call
  site already computed for its own diagnostic purposes; it introduces no new COM property reads.

### 3.3 Reading the context from the watchdog

`ThreadMonitor`'s new lockup callback (§2.2) reads `CurrentStoreContext.Current` at the moment the
threshold is confirmed crossed. This is a plain in-memory field read — no COM, no blocking, safe to
call from the background thread.

## 4. Auto-Disable-Then-Notify Sequence

### 4.1 Orchestrator

Introduce a small orchestrator class (e.g. `StoreLockupResponder`, host-neutral aside from its
constructor dependencies) that `ThreadMonitor`'s lockup callback invokes. Constructor dependencies,
all interface/delegate seams so the class is Moq- and FluentAssertions-testable without Outlook:

- `IStoreDisableService` (F1) — for `IsDisabled`/`DisableSessionOnly`/`DisableForFutureSessions`.
- F3's rehook operation (interface TBD; see §1.6) — for the "Reenable" button.
- A modeless-show delegate, `Action<StoreLockupNotification> showNotification` (or equivalent),
  defaulting to the production modeless-`MyBox` composition described in §4.3.
- A logging sink (`Action<string>`, defaulting to `logger.Warn`, matching the
  `StartupDiagnosticsProbe`/`EmailMoveMonitor` injected-sink style) for the `[store-lockup]` WARN
  line.

### 4.2 Sequence (confirmed decision: disable first, then notify)

1. `ThreadMonitor` confirms the stall threshold is crossed and reads `CurrentStoreContext.Current`.
2. **Guard — no context:** if `Current` is `null`/empty, do nothing (no disable, no notify, no
   log-as-attributed-lockup). Optionally log a lower-severity diagnostic distinct from the
   attributed `[store-lockup]` WARN line, since this is "stall with unknown cause," not "stall
   attributed to store X."
3. **Guard — already disabled:** call `IStoreDisableService.IsDisabled(identity)`. If already
   disabled, skip both the disable call and the notification (idempotency / no duplicate
   notifications — see §5).
4. **Auto-disable:** call `IStoreDisableService.DisableSessionOnly(identity)` synchronously, on the
   watchdog's background thread (pure in-memory state change per F1's spec — no COM).
5. **Log:** emit one `[store-lockup]` line at `Warn` with identity, stall duration, and the
   auto-disable outcome, through the injected sink, so it lands in the JSON important-logs
   appender (`log4net.config:53-70`) without any new appender configuration.
6. **Notify:** marshal a request to show the modeless message onto the UI thread via
   `IUiDispatcher.BeginInvoke` (or `WpfUiDispatcher`'s production default), never `Invoke` — this
   step must not block the calling (watchdog) thread, and must not require the STA thread to be
   idle right now; it is queued and runs when the STA thread next services its dispatcher queue.

Steps 4–6 occur in exactly this order to satisfy the confirmed "auto-disable immediately, then
notify" decision. Note the caveat established in §1.2/§4.4: disabling a store stops it from being
retried on subsequent passes; it does not abort a COM call already in flight on the STA thread for
that store (there is no supported interop mechanism to cancel a synchronous COM call once started).
"Restores responsiveness" therefore means "prevents recurrence on the next pass," not "aborts the
current call" — this should be stated in the feature's user-facing copy expectations and is a
constraint worth flagging back to product/epic owners if not already understood that way.

### 4.3 Modeless notification construction

Given §1.3's finding that `MyBox.ShowDialog(...)` cannot be reused unmodified (its `using`-scoped
viewer disposal is incompatible with true non-blocking `Show()`), the recommended composition is:

- Construct a `MyBoxViewer` directly (not via the `using` convenience overloads).
- Wire buttons using the same building blocks `MyBox` already uses internally
  (`ActionButton`, `MyBox.ReplaceButtons`/`AppendButtonInColumn` — both `internal`, so this
  composition must live inside the `UtilitiesCS` assembly, e.g. as a new method on `MyBox` itself
  or a sibling internal helper in `UtilitiesCS.Dialogs`, not in `TaskMaster`, since `UtilitiesCS`
  does not grant `InternalsVisibleTo` to `TaskMaster` — only to `DynamicProxyGenAssembly2`,
  `UtilitiesCS.Test`, and `ToDoModel.Test`, per `UtilitiesCS\Properties\AssemblyInfo.cs:16-18`).
- Own the viewer's lifetime via its own `FormClosed` handler (`viewer.Dispose()` on close) instead
  of a `using` block, so the form persists on screen until the user clicks a button.
- Show it through an injectable `Action<MyBoxViewer> showAction` seam defaulting to
  `viewer => viewer.Show()`, mirroring `EfcHomeController.ViewerShowAction`
  (`EfcHomeController.cs:294`) — not through `MyBox.DialogInvoker`, which is the modal seam and is
  process-global (`AsyncLocal`), shared with unrelated modal call sites and their tests.
- The three buttons map directly to the confirmed labels: "Disable This Session Only" →
  `IStoreDisableService.DisableSessionOnly(identity)`; "Disable for Future Sessions" →
  `IStoreDisableService.DisableForFutureSessions(identity)`; "Reenable" → F3's rehook operation
  followed by clearing the disablement (per F1's `Reenable(identity)`, which the epic manifest
  states "is invoked by F1's `Reenable` and by F4/F5 reenable actions" — i.e. F4's Reenable button
  should call `IStoreDisableService.Reenable(identity)`, which in turn is responsible for invoking
  F3's rehook, keeping F4's button handler a single call rather than duplicating F3's sequencing).
- **Store-identity display formatting:** use the cached identity string captured in
  `CurrentStoreContext`/passed through the attribution (the same `DisplayName` value already used
  by `StoreFilterAttribution.FormatLine` and `StartupInboxAttributionProbe`) directly in the
  message text (e.g. `"The mailbox '{identity}' has stopped responding and has been disabled for
  this session."`). Do not attempt any additional COM read to "improve" the display string — the
  epic's core constraint is cheap-cached-identity-only.

### 4.4 Marshalling correctness

- The orchestrator's notify step must go through `IUiDispatcher` (or the `WpfUiDispatcher`
  production default), not `UiThread.Dispatcher` directly, so tests can substitute a
  synchronous-pass-through mock (`a => a()`) and assert the show-action was invoked, per the
  existing `IUiDispatcher`/`EmailMoveMonitor` seam conventions.
- `BeginInvoke` (fire-and-forget) is preferred over `InvokeAsync`/`Invoke` for this call because
  the watchdog thread has no reason to wait for the show to complete or even to be scheduled; it
  only needs to hand off the request.

## 5. Guardrails

- **No context → no attribution:** covered by §4.2 step 2. A stall with `CurrentStoreContext.Current
  == null` must not call `DisableSessionOnly` for any store and must not show the three-button
  notification (there is nothing to attribute it to). This directly satisfies the acceptance
  criterion "no false attribution when no store context is active."
- **No duplicate notification for an already-disabled store:** covered by §4.2 step 3
  (`IStoreDisableService.IsDisabled(identity)` guard before both disable and notify). This also
  protects against a store that keeps re-entering the per-store loop after being disabled (e.g. if
  a later F1/F3 change causes a disabled store to still be enumerated somewhere) from re-triggering
  the UI.
- **Identity unavailable:** the three per-store call sites already guard `DisplayName` reads with
  `try/catch` in two of the three locations (`AppOlObjects.cs:144-152` returns `"<unavailable>"`
  ; `StoresWrapper.ShouldIncludeStoreInstrumented:147-152` swallows and leaves `displayName` null).
  `StoreWrapper.Init()` does **not** currently guard its `DisplayName` read (`StoreWrapper.cs:36`)
  — if this call itself throws, no scope is ever opened and `CurrentStoreContext.Current` stays at
  its prior value (or null), which is the same "no context" guard as above, not a crash in the
  watchdog path. The recommended `CurrentStoreContext.Begin` call sites should tolerate/normalize a
  null or `"<unavailable>"` identity by treating it the same as "no context" for the purposes of
  auto-disable/notify (F1's identity fallback, once specified, determines whether
  `"<unavailable>"` is itself a usable/persistable identity key — that is an F1 decision, not
  F4's).
- **Watchdog must not itself introduce a new stall:** every step in §4.2 that runs on the
  background thread (`IsDisabled`, `DisableSessionOnly`, the log call, `BeginInvoke`) is
  non-blocking/in-memory by construction; nothing in the orchestrator calls back into Outlook COM.

## 6. Test Strategy

Consistent with `UT4`/`CUT` (MSTest + Moq + FluentAssertions, no live Outlook, no temp files, no
real waits/timers) and the repo's established coverable-decision/thin-host split:

- **Clock/threshold determinism:** inject `TimeProvider` (production `TimeProvider.System`, tests
  `FakeTimeProvider`) into the extracted pure decider (e.g. `LockupStallDecider`), and assert
  threshold-crossing behavior by advancing the fake provider — no `Thread.Sleep`/`Task.Delay` in
  tests, matching the pattern already used for `StartupLifetimeStopDecider` (constructor-injected
  numeric parameters, no live clock inside the decider itself).
- **Attribution:** unit-test `CurrentStoreContext.Begin`/`Current` directly (set inside scope,
  reverts after `Dispose`, tolerates nested/sequential scopes) without any COM or threading
  dependency — it is a plain static holder.
- **Orchestrator (`StoreLockupResponder`):** Moq the `IStoreDisableService`, F3's rehook interface,
  the `IUiDispatcher` (or the notify delegate), and the logging sink. Assert, per scenario:
  - Threshold-crossing + valid identity + not-disabled → `DisableSessionOnly` called once, then
    the notify delegate invoked once, in that order (verify via Moq `MockSequence` or call-order
    capture) — covers the confirmed "disable immediately, then notify" sequencing.
  - No identity in context → neither `DisableSessionOnly` nor the notify delegate is called.
  - Already-disabled identity (`IsDisabled` returns true) → neither call happens a second time.
  - Each of the three notification buttons invokes the correct service call
    (`DisableSessionOnly`/`DisableForFutureSessions`/F3-rehook-then-`Reenable`) — construct the
    notification's button/action set directly (bypassing the real WinForms `Show()`) and invoke
    each `Action` to assert the correct downstream mock call, mirroring how
    `MyBox_ShowDialog_Tests` exercises `ActionButton` delegates without displaying a real window.
  - The notify path is dispatched through `BeginInvoke`/the injected show-action seam, not
    `Invoke`, so the test can assert on the seam call without needing a live STA message pump.
- **`ThreadMonitor` (extended):** since it currently has zero tests, plan for new deterministic
  tests once the clock is injectable: construct with a `FakeTimeProvider`, drive simulated ticks,
  and assert the lockup callback fires exactly when the injected threshold is exceeded and not
  before. The existing `Thread.Suspend`/stack-capture path should remain isolated behind the
  original diagnostic threshold and, given its use of obsolete APIs and a real `Thread`, may need
  to stay COM/host-bound-exempt (or itself be tested only via the callback/threshold contract, not
  by asserting on captured stack traces).
- **Logging:** assert the `[store-lockup]` line format via a pure formatter class (mirroring
  `StoreFilterAttribution.FormatLine`'s test style: no log4net dependency in the unit test, just a
  string-equality/contains assertion on the formatter's output), and separately assert (at the
  orchestrator level) that the sink delegate is invoked with `Warn`-equivalent semantics.
- **No Outlook, no temp files, no real waits:** none of the above requires a live `Application`,
  `Store`, or `Dispatcher`; `IUiDispatcher` is mocked, `CurrentStoreContext` is a plain static, and
  `TimeProvider`/`FakeTimeProvider` replace all timing.

## 7. File-by-File Change List

Production files (new or modified), all in `UtilitiesCS` or `TaskMaster` (both already
`InternalsVisibleTo`-scoped to their respective `.Test` projects):

| # | File | Change |
|---|------|--------|
| 1 | `UtilitiesCS\Threading\ThreadMonitor.cs` | Inject `TimeProvider`, replace `Thread.Sleep`-based polling/backoff with clock-driven elapsed-time checks, add a second configurable `lockupAttributionThresholdMs` and an `Action<...> onLockupDetected` callback invoked when it is crossed (reading `CurrentStoreContext.Current`). Existing diagnostic stack-capture path preserved behind the original threshold. |
| 2 | `UtilitiesCS\Threading\LockupStallDecider.cs` (new) | Pure, coverable decision helper (elapsed ms in, stall-confirmed bool out), following the `StartupLifetimeStopDecider` pattern, extracted out of `ThreadMonitor` so the threshold logic is unit-testable without a live Dispatcher/thread. |
| 3 | `UtilitiesCS\Threading\CurrentStoreContext.cs` (new) | Static, `volatile`-backed ambient scope (`Current`, `Begin(identity)` returning an `IDisposable`) — the cross-thread-visible attribution mechanism (§3.2). |
| 4 | `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs` | Wrap the post-`DisplayName` blocking calls in `Init()` (`GetRootFolder`, conditional `GetDefaultFolder(Inbox)`, `GetSmtpAddressFromStore()`) in `using (CurrentStoreContext.Begin(DisplayName))`. |
| 5 | `UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs` | Wrap `storeWrapper.Init()`/`storeWrapper.Restore(store)` in `RewireOlObjectsAsync` in a `CurrentStoreContext.Begin(storeDisplayName)` scope. |
| 6 | `TaskMaster\AppGlobals\AppOlObjects.cs` | Wrap the `getDefaultFolder()` call inside `EmitPerStoreInboxAttribution` in a `CurrentStoreContext.Begin(displayName)` scope. **Verified this file is already 525 lines**, over the repo's 500-line file-size ceiling (`general-code-change.md`), independent of F4. The wrap itself is a minimal (1-3 line) in-place edit, not a net-growth refactor, but it still touches an already-over-limit file. Follow the precedent already used for the sibling F3 feature (`AppEvents.ReadinessHookup.cs` extracted from `AppEvents.cs` for the same reason): if the atomic plan requires the file to shrink back under the ceiling as part of touching it, extract an unrelated cohesive slice into a partial class file rather than folding the `CurrentStoreContext` wrap into a larger refactor of this feature. |
| 7 | `UtilitiesCS\OutlookObjects\Store\StoreLockupAttribution.cs` (new) | Pure `[store-lockup]` line formatter (identity, stall duration, disable outcome), mirroring `StoreFilterAttribution.FormatLine`. |
| 8 | `UtilitiesCS\Threading\StoreLockupResponder.cs` (new) | Orchestrator: guards (§4.2/§5), calls `IStoreDisableService`, F3's rehook operation, and the modeless notify seam in the confirmed disable-then-notify order; owns the WARN log call. |
| 9 | `UtilitiesCS\Dialogs\MyBox.cs` (or a new sibling internal helper in `UtilitiesCS.Dialogs`) | Add the internal, non-`using`-scoped modeless composition path (owns `MyBoxViewer` lifetime via `FormClosed`, injectable `Action<MyBoxViewer> showAction` defaulting to `viewer => viewer.Show()`) described in §4.3. Does not modify the existing modal `ShowDialog`/`DialogInvoker` overloads. |
| 10 | `TaskMaster\ThisAddIn.cs` | Change `UiThread.Init(monitorUiThread: false)` to `true` (§2.3), and wire `StoreLockupResponder`'s dependencies (F1's `IStoreDisableService` from `IApplicationGlobals`, F3's rehook operation, `WpfUiDispatcher`) at startup alongside the existing globals construction. |

**Production file count: 10** (6 modified: `ThreadMonitor.cs`, `StoreWrapper.cs`,
`StoresWrapper.cs`, `AppOlObjects.cs`, `MyBox.cs`, `ThisAddIn.cs`; 4 new:
`LockupStallDecider.cs`, `CurrentStoreContext.cs`, `StoreLockupAttribution.cs`,
`StoreLockupResponder.cs`). This excludes F1/F3's own files (`IStoreDisableService` and its
implementation, F3's rehook interface/implementation), which are out of scope for F4 and are
consumed, not authored, here.

### Cross-feature impacts

- **F1 dependency:** F4 cannot compile the `StoreLockupResponder`/notification button wiring until
  `IStoreDisableService` exists on `IApplicationGlobals`. No F1 files are modified by F4; F4 is a
  pure consumer.
- **F3 dependency:** the "Reenable" button's downstream call depends on F3's rehook interface name
  and F1's `Reenable(identity)` sequencing it. No F3 files are modified by F4.
- **F5 (settings UI, wave 2, sibling):** F5 lists disabled stores and offers per-store reenable; it
  shares `IStoreDisableService` and F3's rehook operation with F4 but touches no files in common
  with this list — no direct file-level conflict expected, only shared-contract coordination
  (already captured in the epic manifest's "Shared Design Alignment" section).
- **#211 diagnostic instrumentation:** `StoreWrapper.Init`, `StoresWrapper.RewireOlObjectsAsync`,
  and `AppOlObjects.LoadInboxes`/`EmitPerStoreInboxAttribution` all carry existing #211
  Stopwatch/logging instrumentation. F4's `CurrentStoreContext.Begin` scopes must wrap *around*
  the existing instrumented blocks without altering their behavior, ordering, or emitted
  `[Startup timing]`/`[store-filter]`/`[loadinboxes]` lines — this is an additive, behavior-preserving
  change, consistent with how those diagnostics were themselves added.
- **`ThisAddIn.cs` startup behavior:** enabling `monitorUiThread: true` is the only behavior change
  to existing startup outside the store-processing paths; it starts one additional background
  polling loop for the add-in's lifetime, which was previously implemented but dormant.

## Rejected Alternatives

- **AsyncLocal for store-identity ambient context** — rejected; does not flow to the independent
  background thread that must read it (§3.1).
- **A brand-new, separate watchdog class instead of extending `ThreadMonitor`** — rejected in favor
  of extending the existing, already-wired polling primitive, to avoid duplicating the
  ping-and-measure mechanism and keeping one canonical "is the UI thread stalled" measurement
  (§2.1).
- **Reusing `MyBox.ShowDialog(string, string, BoxIcon, Dictionary<string, Action>)` unmodified for
  the modeless notification** — rejected; its `using`-scoped viewer disposal and its
  `DialogInvoker` seam are both built around a modal, blocking `ShowDialog()` call and would
  dispose the form immediately if repointed to a non-blocking `Show()` (§1.3, §4.3).
