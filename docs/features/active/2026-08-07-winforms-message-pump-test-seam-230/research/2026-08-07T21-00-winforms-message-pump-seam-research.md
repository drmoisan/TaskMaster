# Research — WinForms `Application.Run()` Message-Pump Test Seam (Issue #230)

- **Timestamp:** 2026-08-07T21-00
- **Issue:** #230 (winforms-message-pump-test-seam)
- **Feature folder:** `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/`
- **Author:** task-researcher agent
- **Status:** Research complete; no implementation performed.

All findings below were verified by reading the cited files in this worktree unless explicitly
marked as framework-behavior knowledge; framework claims are cross-checked against in-repo test
evidence where such evidence exists.

---

## 1. Current State Analysis

### 1.1 The existing WPF seam (the pattern to mirror)

Two proven WPF `Dispatcher.Run()`-on-a-background-thread implementations exist:

**`StaDispatcherHost`** (`UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:161-208`):

- **Thread creation:** private `Thread` whose body captures `Dispatcher.CurrentDispatcher` and
  `Thread.CurrentThread.ManagedThreadId` into host properties.
- **Apartment state:** `_thread.SetApartmentState(ApartmentState.STA)` before `Start()`.
- **Readiness signalling:** `AutoResetEvent _ready`; the thread body sets it after capturing the
  dispatcher; the host constructor blocks on `_ready.WaitOne()` so the host is fully usable when
  the constructor returns.
- **Pump start:** `Dispatcher.Run()` inside `try`, with a
  `TaskCompletionSource<bool> _stopped (TaskCreationOptions.RunContinuationsAsynchronously)`
  completed in `finally`.
- **Shutdown:** `StopAsync()` calls `Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send)`,
  awaits `_stopped.Task`, then `_thread.Join()`, then throws `InvalidOperationException` if the
  thread is still alive, then disposes the readiness event.
- **Usage discipline:** every test wraps usage in `try { ... } finally { await host.StopAsync(); }`.

**`StartRunningDispatcher()`** (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:297-326`):

- Same shape, reduced: `ManualResetEventSlim` readiness handshake, `IsBackground = true`, a
  descriptive thread `Name`, STA, `Dispatcher.Run()`. Shutdown is `ShutdownDispatcher(dispatcher)`
  → `dispatcher?.InvokeShutdown()` (cross-thread), called from the test's `finally`.
- Consumed by `QfcItemController_ViewerSetupTests.AssignControlsAsync_DispatchesAssignThroughViewerDispatcher`
  and `PopulateControlsAsync_...` (`QfcItemController.ViewerSetupTests.cs:199-226, 300-335`):
  per-test instance, `try/finally` shutdown, completion observed by awaiting the dispatched task —
  no polling, no sleeping.

Neither host uses `Thread.Sleep`, `Task.Delay`, or wall-clock polling; all waits are event- or
task-based handshakes signalled deterministically. This is the established in-repo determinism
pattern the WinForms seam must replicate.

### 1.2 What `itemViewer.UiSyncContext` is and why awaiting it hangs

- `ItemViewer()` constructor (`QuickFiler/Viewers/ItemViewer.cs:23-30`):
  `InitializeComponent(); _context = SynchronizationContext.Current; _uiScheduler =
  TaskScheduler.FromCurrentSynchronizationContext(); _uiDispatcher = Dispatcher.CurrentDispatcher;`.
  `UiSyncContext` (line 60) is a plain getter over `_context`.
- WinForms `Control` construction auto-installs a `WindowsFormsSynchronizationContext` when the
  ambient context is null or the exact base `SynchronizationContext` type
  (`WindowsFormsSynchronizationContext.AutoInstall` defaults to `true`). This is proven in-repo:
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:192-220` installs a base
  `SynchronizationContext`, constructs the control, and asserts
  `viewer.UiSyncContext.Should().BeOfType<WindowsFormsSynchronizationContext>()` — a passing test.
  Therefore a headless `ItemViewer` always captures a `WindowsFormsSynchronizationContext` bound
  to the **constructing thread**.
- `await someControl.UiSyncContext` compiles via the extension awaiter
  `UtilitiesCS/Threading/UiThread.cs:85-111`:
  - `IsCompleted => _context == SynchronizationContext.Current` (reference equality — completes
    synchronously only when already running with that exact context instance current);
  - otherwise `OnCompleted` does `_context.Post(...)`. For a `WindowsFormsSynchronizationContext`,
    `Post` marshals through the thread's hidden WinForms marshaling (parking) window via
    `Control.BeginInvoke`/`PostMessage`; the posted continuation executes **only when the owning
    thread runs a message loop**.
- On a thread-pool MSTest async-test thread there is no message loop, so the continuation is never
  drained — the documented indefinite-hang hazard
  (`UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs:369-378`).

### 1.3 How #227 cycle 5 achieved headless `ItemViewer` construction

`QfcItemController.ViewerSetupTests.cs:378-405` (`ResolveControlGroups_WithHeadlessItemViewer_...`):
save `SynchronizationContext.Current`; `SetSynchronizationContext(new SynchronizationContext())`;
`new QuickFiler.ItemViewer()`; exercise the synchronous member; restore the prior context in
`finally`. No handle is created, no message loop is needed, because the member under test never
posts to the captured context. The structural precedent is `ProgressPane_Tests.cs` /
`ProgressViewer_Tests.cs`. This works for synchronous members only; any member that
`await`s `UiSyncContext` (or dispatches through `UiDispatcher`) needs the pump this research
specifies.

### 1.4 What must be true for the 9 affected members to be exercised

All of the following, simultaneously:

1. A **real `ItemViewer` constructed on the pump thread** (every affected member casts
   `_itemViewer` to concrete `ItemViewer` and/or walks its Designer control tree), so that
   `UiSyncContext`, `UiScheduler`, and `UiDispatcher` are all bound to the pumped thread.
2. The pump thread must be **running a message loop** whenever an `await *.UiSyncContext`
   continuation, a `Control.BeginInvoke`, or a WPF `Dispatcher` operation from these members is
   pending, and must keep running until the member's returned `Task` completes.
3. Controller wiring per the established harness pattern (`HarnessController` +
   `QfcItemControllerTestSupport.SetField`, `QfcItemController.TestSupport.cs:25-59`): injected
   `_itemViewer`, `_globals` (with `Ol.DarkMode`, `QfSettings`), `Token`/`_tokenSource`,
   `_uiDispatcher`, a **mocked `IWebViewCoreInitializer`** (`_webViewInitializer`), and mocked
   factories (`_conversationResolverFactory`, folder-predictor factories) as each path requires.
   For the factory members see the seam gap in §7.3.
4. `QfcThemeHelper.SetupThemes` must run headless. The ratified boundary
   (`exemption-boundary.2026-07-02T17-00.md`, row `Initialize(bool async)`) already records that
   the barrier for these members is "the unbuilt WinForms message-pump seam ... not headless
   construction", i.e. cycle-5 analysis judged `SetupThemes` headless-safe. This should be
   re-confirmed by the first spike test, but no contrary evidence was found.
5. The real `Microsoft.Web.WebView2.WinForms.WebView2` child controls in the Designer tree are
   constructed but **never initialized** (`EnsureCoreWebView2Async` is reached only through the
   mocked `IWebViewCoreInitializer`); cycle-5 tests already prove bare construction is safe.

---

## 2. WinForms Pump Mechanics (Question 2)

### 2.1 `Application.Run(ApplicationContext)` vs `Application.Run()`

Both run a standard message loop on the current thread without showing any window and both
terminate when `Application.ExitThread()` executes **on that thread**. Recommendation:
`Application.Run(new ApplicationContext())`, for parity with the issue's proposal and because the
retained `ApplicationContext` gives an explicit, documented exit handle
(`context.ExitThread()` raises `ThreadExit` → ends the loop) in addition to
`Application.ExitThread()`. No `Form` is created; the only window involved is the hidden
message-only marshaling (parking) window WinForms creates for cross-thread marshaling, so the
seam is fully headless.

### 2.2 Establishing and capturing the `WindowsFormsSynchronizationContext`

- On the pump thread, before signalling readiness, install the context **explicitly**:
  `SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext())`.
  Constructing it eagerly creates the thread's marshaling window, so `Post` from any thread is
  valid immediately.
- Do **not** rely on `WindowsFormsSynchronizationContext.AutoInstall` for the host itself
  (auto-install happens as a side effect of `Control` construction; explicit installation makes
  the capture deterministic and independent of what work runs first). Leave the static
  `AutoInstall` property untouched (process-global; mutating it would bleed into parallel test
  classes).
- Because the WinForms context is already installed when `ItemViewer` is later constructed on the
  pump thread, `Control`'s auto-install logic leaves it in place (it only replaces a null or
  exact-base-type context), so `itemViewer.UiSyncContext` captures the host's context instance —
  and the awaiter's reference-equality `IsCompleted` check then short-circuits for code already
  running on the pump.

### 2.3 Readiness handshake

Mirror `StaDispatcherHost`: the thread body (a) installs the context, (b) copies
`SynchronizationContext.Current` and `ManagedThreadId` into host fields, (c) signals a
`ManualResetEventSlim`, (d) enters `Application.Run(_context)`. The host constructor blocks on the
event, so when it returns the sync context is valid. Work posted between (c) and (d) is not lost:
`WindowsFormsSynchronizationContext.Post` is `PostMessage` to the already-created marshaling
window, and those messages queue until the loop starts. Any exception thrown during (a)-(b) must
be stored in an `_initializationError` field **before** the readiness signal (signal in
`finally`), and the constructor must rethrow it after the wait — otherwise a broken host presents
as a hang.

### 2.4 Clean shutdown

1. `StopAsync()` posts `Application.ExitThread` onto the pump via the captured context
   (`SyncContext.Post(_ => Application.ExitThread(), null)`) — `ExitThread` only affects the
   calling thread's loop, so it must execute on the pump thread, exactly as
   `Dispatcher.BeginInvokeShutdown` is marshaled in the WPF host.
2. Optionally first call `System.Windows.Threading.Dispatcher.FromThread(_thread)?.InvokeShutdown()`
   to retire any WPF dispatcher that `ItemViewer`'s constructor lazily created on the pump thread
   (hardening; pending WPF operations are otherwise silently dropped when the thread exits).
3. `Application.Run` returns; a `finally` around it completes a
   `TaskCompletionSource<bool>(RunContinuationsAsynchronously)` `_stopped`.
4. `StopAsync()` awaits `_stopped.Task`, then `_thread.Join()`, then verifies `!_thread.IsAlive`
   (throw `InvalidOperationException` otherwise — the `StaDispatcherHost` precedent), then
   surfaces any recorded pump-thread exceptions (§3), then disposes the readiness event.
5. `Dispose()` is the idempotent synchronous bridge (`StopAsync().GetAwaiter().GetResult()`),
   safe on the test thread because the test thread has no context bound to the pump. The thread is
   additionally `IsBackground = true` and named
   (e.g. `"QuickFiler.Test.WinFormsPumpHost"`) so a defective test cannot keep the vstest process
   alive (the `StartRunningDispatcher` precedent).

An exited WinForms loop also tears down the thread's `Application` thread-context and its parking
window — cleanup the WPF `Dispatcher.Run` host cannot provide for WinForms state. This is one of
the two reasons `Application.Run` is preferred over reusing the WPF pump (§6).

---

## 3. Determinism and Hang-Avoidance (Question 3)

Rules of `.claude/rules/general-unit-test.md` (Determinism Infrastructure) applied:

- **No `Thread.Sleep` / `Task.Delay` / polling anywhere.** All coordination uses the same
  primitives as the accepted precedents: `ManualResetEventSlim` (readiness),
  `TaskCompletionSource<T>` with `RunContinuationsAsynchronously` (completion/stopped), and
  awaiting the member's own returned `Task` (progress). The existing seams' unbounded
  `ready.Wait()` / `thread.Join()` after a deterministic signal are established in-repo practice
  (`WpfUiDispatcherTests.cs:187,198`; `TestSupport.cs:314`).
- **Exception marshalling — three channels:**
  1. *Async members under test:* the async state machine captures exceptions into the returned
     `Task` regardless of which thread ran the faulting segment; the test simply awaits
     (`await controller.InitializeAsync()` throws on the MSTest thread). This is the primary
     channel and requires nothing from the host.
  2. *Host-posted delegates:* `InvokeAsync`/`RunAsync` wrap every delegate in `try/catch` →
     `tcs.TrySetException(ex)`, mirroring how `WpfUiDispatcher` faults propagate in
     `WpfUiDispatcherTests.InjectedDispatcher_PropagatesOriginalFaultsFromBothAsyncOverloads`.
  3. *Stray pump-loop exceptions* (e.g. a fire-and-forget production post that throws inside a
     marshaled callback): the thread body subscribes `Application.ThreadException` (a
     thread-bound event) and records exceptions into a host list; `StopAsync()` rethrows the
     first recorded exception (as an `AggregateException` if several). This converts what would
     otherwise be a swallowed dialog/quiet failure into a test failure at the disposal point.
- **A failure surfaces as a test failure, not a CI timeout**, because: construction failures
  rethrow from the constructor (§2.3); posted-work failures fault the awaited task; and the pump
  cannot silently die while work is pending (`Application.Run`'s exit path completes `_stopped`,
  and `StopAsync` reports recorded faults). The only residual hang class is a genuine deadlock in
  the code under test (pump thread blocked forever); the recommended belt for the new test files
  is MSTest `[Timeout(...)]`, which has in-repo precedent
  (`TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:29,67`) and is a harness bound, not a
  banned wall-clock wait in test logic.
- **Never bridge the two threads synchronously in the hazardous direction:** test code must not
  block the pump thread on the test thread (no `Invoke`-style synchronous member is exposed on
  the host at all — only `Task`-returning members), which structurally prevents the classic
  A-waits-B-waits-A deadlock.

---

## 4. Recommended Approach

### 4.1 Summary

Build a small, self-contained `WinFormsPumpHost` test-support class in `QuickFiler.Test` that runs
`Application.Run(ApplicationContext)` on a dedicated named STA background thread with an
explicitly installed `WindowsFormsSynchronizationContext`, a readiness handshake, `Task`-based
posting members, `Application.ThreadException` capture, and deterministic
post-`ExitThread`/await/join shutdown — the exact `StaDispatcherHost` shape transposed from WPF to
WinForms. Tests construct the real `ItemViewer` on the pump via the host, run the affected
`QfcItemController` members from the MSTest thread, and await their `Task`s.

### 4.2 Concrete API shape (net481-safe; see §8)

```csharp
namespace QuickFiler.Controllers.Tests   // or QuickFiler.Test.TestSupport
{
    /// <summary>
    /// Runs a real WinForms message pump (Application.Run) on a dedicated STA background
    /// thread so tests can deterministically await continuations captured by a
    /// WindowsFormsSynchronizationContext. WinForms analogue of StaDispatcherHost
    /// (UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs).
    /// </summary>
    internal sealed class WinFormsPumpHost : IDisposable
    {
        internal WinFormsPumpHost();                          // starts the pump; blocks until ready; rethrows init failure

        internal SynchronizationContext SyncContext { get; }  // the pump's WindowsFormsSynchronizationContext
        internal int ThreadId { get; }                        // pump thread's ManagedThreadId

        internal Task InvokeAsync(Action action);             // run sync work on the pump (e.g. assertions on controls)
        internal Task<T> InvokeAsync<T>(Func<T> factory);     // e.g. host.InvokeAsync(() => new ItemViewer())
        internal Task RunAsync(Func<Task> asyncWork);         // start async work ON the pump; unwrapped completion
        internal Task<T> RunAsync<T>(Func<Task<T>> asyncWork);

        internal Task StopAsync();                            // post ExitThread, await stopped, join, surface pump faults
        public void Dispose();                                // idempotent synchronous bridge to StopAsync
    }
}
```

Disposal contract: `StopAsync`/`Dispose` are idempotent; after stop, posting members fault their
returned task with `ObjectDisposedException` (fail fast rather than silently queueing to a dead
loop); the constructor never returns a half-initialized host. Usage contract in tests: one host
per test (or per test class via `[ClassInitialize]`/`[ClassCleanup]` where several tests share
one `ItemViewer`), always released in `finally`/`Dispose`.

Canonical usage for the affected members:

```csharp
using (var host = new WinFormsPumpHost())
{
    var viewer = await host.InvokeAsync(() => new QuickFiler.ItemViewer());
    var controller = new HarnessController();
    QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer);
    // ... inject _globals / token / _webViewInitializer mock / factories per member ...

    await controller.InitializeSequentialAsync();   // continuations drain through the live pump

    // assert against viewer/controller state; control reads via host.InvokeAsync if handle-bound
}
```

Note the `ItemViewer` is constructed **via the host**, so no `SynchronizationContext` is ever
installed or mutated on the MSTest thread — strictly cleaner isolation than the cycle-5
save/restore pattern.

### 4.3 Rejected alternatives (brief)

1. **Reuse the existing WPF `StartRunningDispatcher()` thread and construct `ItemViewer` there.**
   `Dispatcher.Run`'s inner loop does dispatch all thread window messages, so it would likely also
   drain WinForms marshaling posts; rejected because (a) it leaves WinForms per-thread state
   (`Application` thread context, parking window) undisposed at `InvokeShutdown` — state leak
   across the suite; (b) it is lower-fidelity to production (Outlook/VSTO pumps a Win32 loop
   through WinForms, not a WPF dispatcher frame); (c) issue #230 and the ratified maintainer
   decision explicitly scope the `Application.Run` analogue. Kept as a documented fallback if an
   unforeseen `Application.Run` blocker appears.
2. **Custom single-threaded queue context (AsyncPump-style) with
   `WindowsFormsSynchronizationContext.AutoInstall = false`.** Rejected: `AutoInstall` is
   process-global static state (bleed across parallel classes, violating test independence);
   `Control.BeginInvoke`/`InvokeRequired`/handle-bound paths bypass `SynchronizationContext`
   entirely, so fidelity is wrong for exactly the members under test.
3. **Refactor production so the 9 members no longer await `UiSyncContext` (route through
   `IUiDispatcher`).** Rejected: a behavior-risk production rewrite of the initialization
   orchestration, contrary to #230's charter (test infrastructure) and to the ratified #227
   boundary rationale, which framed the gap as missing *test* infrastructure.

---

## 5. Behavior Semantics of the Seam

- **Success:** constructor returns ⇔ pump live and context captured; a posted/awaited work item's
  task completes ⇔ the work ran to completion on the pump thread; `StopAsync` returns ⇔ loop
  exited, thread joined, no recorded pump faults.
- **Failure:** init failure → constructor throws; work failure → its task faults with the original
  exception; stray marshaled-callback failure → recorded and rethrown by `StopAsync`; pump thread
  fails to terminate → `InvalidOperationException` from `StopAsync` (precedent
  `WpfUiDispatcherTests.cs:199-204`).
- **Ordering:** posts through one `WindowsFormsSynchronizationContext` execute in post order
  (single message queue, single thread); work posted before the loop starts is queued, not lost.
- **Edge cases:** double `Dispose` is a no-op; post-after-stop faults fast; nested
  `await UiSyncContext` on the pump thread completes synchronously (awaiter reference-equality);
  host never shows a window (no `Form` created).

---

## 6. Placement (Question 5)

**Facts verified:**

- No shared test-support project exists in `TaskMaster.sln` (no `*TestSupport*`/`*TestCommon*`
  project; sln grep negative). The nine test projects are all legacy non-SDK
  `TargetFrameworkVersion v4.8.1` / `packages.config` projects.
- `QuickFiler.Test.csproj` references `QuickFiler`, `UtilitiesCS`, `TaskVisualization`
  (lines 414-425) — everything the seam and its consumers need (WinForms, `System.Windows.Threading`,
  `UiThread.GetAwaiter` from `UtilitiesCS`).
- `UtilitiesCS.Test.csproj` references `TaskMaster` and `UtilitiesCS` (lines 908-915). A
  `QuickFiler.Test → UtilitiesCS.Test` project reference would drag the `TaskMaster` VSTO project
  into `QuickFiler.Test`'s dependency closure; the reverse direction is not needed today.
- All 9 affected members live in `QuickFiler`, and all their existing tests live in
  `QuickFiler.Test`. `UtilitiesCS.Test` has only *potential* future use (e.g. the deliberately
  avoided `ProgressPane` async-path test in `AsyncSerialization_Tests.cs`).

**Recommendation:** place the host in `QuickFiler.Test` as a new file
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (plus `WinFormsPumpHostTests.cs` beside the
other test files), the only project with a committed consumer. Do not create a new shared
test-support csproj now: a new non-SDK project (sln entry, `packages.config`, MSTest adapter
wiring, analyzer wiring) for one ~150-200-line class contradicts the simplicity-first policy, and
a test-to-test reference in either direction creates undesirable dependency-closure coupling
(above). If `UtilitiesCS.Test` later gains a real consumer, promote the file to a dedicated
shared test-support project in that change. This mirrors how `StartRunningDispatcher` lives in
`QuickFiler.Test` today while `StaDispatcherHost` lives separately in `UtilitiesCS.Test` — the
repo already tolerates per-project pump helpers; consolidation is a follow-up, not a prerequisite.

---

## 7. Coverage Impact (Question 7)

### 7.1 Exact attribute sites for the 9 affected members (current worktree)

| # | Member | Attribute site | Removable with the pump seam? |
|---|---|---|---|
| 1 | `Initialize` (9-arg private) | `QfcItemController.Initialization.cs:138` | Yes (reflection-invoke via `InvokeNonPublic`, funnels into #2) |
| 2 | `Initialize(bool async)` | `QfcItemController.Initialization.cs:168` | Yes (final `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)` is fire-and-forget; see §7.4 note on the WPF-dispatcher-on-WinForms-pump interop smoke test) |
| 3 | `InitializeAsync` | `QfcItemController.Initialization.cs:200` | Yes, with caveat: its last statement `await InitializeWebViewAsync()` will fault at the mocked web-view seam; the test asserts the controlled fault; all preceding lines execute |
| 4 | `InitializeGraphicsAsync` | `QfcItemController.Initialization.cs:260` | Yes (fire-and-forget web-view init) |
| 5 | `InitializeSequentialAsync` | `QfcItemController.Initialization.cs:291` | Yes (fire-and-forget web-view init) |
| 6 | `CreateAsync` | `QfcItemController.Initialization.cs:403` | Yes, but requires the factory seam gap fix in §7.3 |
| 7 | `CreateSequentialAsync` | `QfcItemController.Initialization.cs:436` | Yes, same §7.3 dependency |
| 8 | `InitializeWebViewAsync` | `QfcItemController.ViewerSetup.cs:38` | **No — remains exempt** (§7.2) |
| 9 | `ResolveControlGroupsAsync(ItemViewer)` | `QfcItemController.ViewerSetup.cs:253` | Yes (pure pump case: `QfcTipsDetails.CreateAsync` posts + `await itemViewer.UiSyncContext` drain on the pump) |

Ratified-boundary cross-reference: these 9 are exactly "category 1" of
`docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
(rows at lines 30-38), ratified in `maintainer-decision.2026-07-02.md`, which also pre-authorizes
#230 as the follow-up for precisely this bucket.

### 7.2 Why `InitializeWebViewAsync` cannot be fully de-exempted by the pump alone

The pump removes its `await _itemViewer.UiSyncContext` barrier (`ViewerSetup.cs:55`), but lines
76-117 dereference `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`, which is null unless
the real WebView2 runtime initialized the control — a genuine external-process dependency barred
by the unit-test policy (same class of barrier as the already-exempt `WebView2CoreInitializer`,
boundary category 5). With the mocked `IWebViewCoreInitializer`, execution reaches the
`EnsureCoreWebView2Async` seam call and must stop there (controlled fault). Partial line coverage
of the first ~20 lines is attainable, but the member cannot be meaningfully covered end-to-end.
Recommendation: keep the attribute with an updated justification comment (pump barrier resolved;
residual barrier = CoreWebView2/WebView2 runtime), and record a possible follow-up refactor
(extract the post-init `CoreWebView2` wiring behind an adapter) as a separate issue if desired.
Expected boundary effect: **19 → 11 members** (8 of 9 de-exempted).

### 7.3 Seam gap discovered: `CreateAsync`/`CreateSequentialAsync` cannot inject mocks

Both factories (`Initialization.cs:404-431, 437-464`) internally do
`new QfcItemController(); controller.SaveParameters(...); await controller.Initialize*Async();`.
`SaveParameters` (`Initialization.cs:380-397`) applies **production defaults** via `??=` —
including `_webViewInitializer ??= new WebView2CoreInitializer()` (the real WebView2 adapter) —
and there is no injection point between `SaveParameters` and the awaited init. A test driving the
factories as-written would therefore invoke the real WebView2 runtime (external dependency,
nondeterministic on CI). **De-exempting #6/#7 requires a minimal additive production change:**
optional seam parameters on the two static factories (at minimum
`IUiDispatcher uiDispatcher = null, IWebViewCoreInitializer webViewInitializer = null,
Func<MailItem, ConversationResolver> conversationResolverFactory = null`), assigned to the
controller's fields before `SaveParameters`, exactly mirroring the primary constructor's existing
optional-seam pattern (`Initialization.cs:29-64`). Non-breaking (defaults preserve behavior),
consistent with repo precedent, and small. If the planner prefers zero production change, #6/#7
stay exempt and the target becomes 19 → 13; the recommended scope includes the factory seams.

### 7.4 Additional census notes

- The controller partials currently contain **19** `[ExcludeFromCodeCoverage]` sites, but only 18
  belong to the ratified boundary: `EnsureBreadcrumbPipeline`
  (`QfcItemController.ViewerSetup.cs:132`) was added by issue #351 **after** ratification and is
  not in the ratified 19 (whose 19th member is `WebView2CoreInitializer`, a non-controller file).
  The implementation phase must re-baseline the census and either justify or address that site in
  its evidence — it is out of #230's 9-member scope but will show up in any grep-based count.
- `ItemViewer` itself is type-level `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`), so
  constructing it on the pump adds nothing to the coverage denominator.
- Removing the 8 attributes adds those members to the denominator; the tests must genuinely cover
  them (repo floor per `CLAUDE.md`; no coverage regression on changed lines). The one member whose
  final `await` faults by design (`InitializeAsync`) still executes every line up to and including
  the faulting await; the plan's coverage gate evidence should confirm per-member line coverage
  after the change.
- `Initialize(bool)`'s tail dispatches `InitializeWebViewAsync` through the **WPF**
  `Dispatcher` captured by `ItemViewer`'s constructor on the pump thread. WPF dispatcher
  operations are serviced by any Win32 message loop on that thread via the dispatcher's
  message-only window; this is standard interop behavior but has no in-repo proof yet, so the
  host's own test file must include a smoke test asserting both marshal routes
  (`await SyncContext` and `Dispatcher.FromThread(pump).InvokeAsync`) execute on the pump thread
  before any controller test relies on it.

---

## 8. net48 Constraints (Question 6)

- `QuickFiler.Test.csproj` and `UtilitiesCS.Test.csproj` both target
  `TargetFrameworkVersion v4.8.1` (non-SDK, `packages.config`; MSTest 4.3.3 adapter/platform).
  `UtilitiesCS.Test` sets `LangVersion Latest`; `QuickFiler.Test` sets no `LangVersion`.
- Per repository memory (confirmed by the `ResourceTimingRow` precedent): net48x has no
  `IsExternalInit`, so `init` accessors, `record`, and `record struct` fail with CS0518. The
  recommended API (§4.2) therefore uses a plain `sealed class`, get-only properties backed by
  fields assigned in the constructor/thread body, and no `init`/`record`/required members. Nothing
  in the design needs them.

---

## 9. Risks and Mitigations (Question 8)

| Risk | Mitigation |
|---|---|
| Pump-thread leak across tests | `IsBackground = true` + named thread (process can always exit); `Dispose` in `finally`/`using` per test; `StopAsync` joins and asserts thread death (precedent `WpfUiDispatcherTests.cs:198-204`). |
| Hung pump converts failure into CI timeout | Posted-`ExitThread` + `_stopped` TCS + join sequence mirrors the accepted WPF host; no synchronous host API that can block the pump on the test thread; `[Timeout]` belt on the new test files (in-repo precedent `NonBlockingDelayTests.cs`). |
| `SynchronizationContext` bleed into sibling tests | The host never touches the MSTest thread's context; all installation happens on the pump thread; `ItemViewer` is constructed via `host.InvokeAsync`, removing even the cycle-5 save/restore need. `AutoInstall` is never mutated. |
| MSTest parallelization interaction | `TaskMaster.runsettings` enables `Workers=0 / ClassLevel` for the whole run (and `UtilitiesCS.Test` also sets assembly-level `Parallelize`); therefore no static/shared host — one host instance per test (or per class), no static mutable state in the host. The existing static `EnsureUiThreadDispatcher`/parked-dispatcher infrastructure (`TestSupport.cs:238-285`) is left untouched. |
| STA requirement vs MSTest default apartment | MSTest test threads are MTA; the seam does not require the test thread's apartment to change — only the pump thread is STA, set via `SetApartmentState(ApartmentState.STA)` before `Start()` (identical to both existing hosts). |
| WebView2 initialization on a pumped thread | Never initiate real WebView2 init: `IWebViewCoreInitializer` is always mocked; bare `WebView2` control construction inside `InitializeComponent` is already proven safe headless (cycle-5 tests). `InitializeWebViewAsync` stays exempt (§7.2). |
| Fire-and-forget faults (`_ = InitializeWebViewAsync()`) tearing the pump or leaking unobserved exceptions | Mocked web-view seam faults fast and deterministically; async-method faults land in the (discarded) `Task`, not the loop; any non-Task marshaled throw is caught by the host's `Application.ThreadException` recorder and surfaced at `StopAsync`. Pending posts at shutdown are dropped with the queue — no test asserts on the discarded path. |
| WPF dispatcher assumptions on the pump thread | §7.4 smoke test proves the WPF-dispatcher-on-WinForms-loop interop in this environment before controller tests depend on it; `StopAsync` optionally `InvokeShutdown`s the thread's WPF dispatcher. |
| Coverage floor regression when removing attributes | Attributes are removed member-by-member only in the same change that adds the covering test; final coverage evidence re-run per the C# toolchain gate. |

---

## 10. Testing Implications (strategy only, no test code)

1. **Seam self-tests first** (`WinFormsPumpHostTests.cs`): executes-on-pump-thread (thread-id
   assertions for `InvokeAsync`, `RunAsync`, and `await host.SyncContext` via `UiThread.GetAwaiter`);
   WPF-dispatcher interop smoke test (§7.4); fault propagation (sync throw, async fault); stop
   semantics (post-after-stop faults; double-dispose no-op; recorded `ThreadException` surfaced).
   Scenario completeness per UT2 (positive, negative, edge, error).
2. **Member tests** in the existing per-cluster files (`QfcItemController.InitializationTests.cs`
   pattern): per member, arrange the harness (§1.4), act by awaiting the member, assert observable
   controller/viewer state (e.g. `_listTipsDetails` populated for `ResolveControlGroupsAsync`,
   themes dictionary set, controls populated, events wired) — never assert on timing.
3. **Framework/libraries:** MSTest + Moq + FluentAssertions only (CUT1/CUT2). No temporary files.
   Host is test code (excluded from coverage denominator by project type).
4. **Order of implementation:** host + self-tests → `ResolveControlGroupsAsync` (smallest pump-only
   member) → `InitializeSequentialAsync`/`InitializeGraphicsAsync` → `Initialize(bool)` + 9-arg
   overload → factory seam change + `CreateAsync`/`CreateSequentialAsync` → `InitializeAsync` →
   attribute removals + boundary/evidence re-baseline.

---

## Automation Feasibility

**Assessment: fully automatable in unattended CI. No manual human interaction is required at any
point in building or running this seam.** Justification, item by item:

- **No live Outlook process:** every Outlook dependency in the affected paths is an Interop
  *interface* mocked with Moq (`Mock<MailItem>` et al.), the established pattern in
  `QfcItemController.ViewerSetupTests.BuildMailItemMock` — already passing without Outlook.
- **No UI click / no visible UI:** `Application.Run(ApplicationContext)` with no `Form` shows
  nothing; the only windows are hidden message-only marshaling windows. The repo's CI already
  constructs real WinForms controls headlessly (cycle-5 `ItemViewer` tests, `ProgressPane`/
  `ProgressViewer` tests pass in unattended runs), which demonstrates window creation works in the
  CI session.
- **No interactive dialog:** the host's `Application.ThreadException` subscription prevents the
  WinForms unhandled-exception dialog path from ever engaging on the pump thread.
- **No portal login / network / external service:** WebView2 runtime is never initialized
  (mocked seam); the one member that genuinely needs it remains `[ExcludeFromCodeCoverage]`
  (§7.2) precisely to preserve unattended determinism.
- **Human involvement of a governance (not execution) nature:** the resulting boundary change
  (19 → 11) touches a maintainer-ratified exemption boundary; per the #227 precedent, the reduced
  boundary evidence should be re-ratified by the maintainer. That is a review/approval step in the
  PR lifecycle, not a manual step in building or running the tests.

---

## References

- `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` (StaDispatcherHost pattern)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (harness, StartRunningDispatcher, parked dispatcher)
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` (headless ItemViewer + running-dispatcher usage)
- `QuickFiler/Viewers/ItemViewer.cs`, `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`
- `QuickFiler/Controllers/QfcItemController.Initialization.cs`, `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `UtilitiesCS/Threading/UiThread.cs` (SynchronizationContextAwaiter)
- `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`, `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`
- `TaskMaster.runsettings`, `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md` (read before forming recommendations)
