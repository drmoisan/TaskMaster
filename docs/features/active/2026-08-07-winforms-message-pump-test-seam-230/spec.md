# 2026-08-07-winforms-message-pump-test-seam — Spec

- **Issue:** #230
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T21-30
- **Status:** Draft
- **Version:** 1.0

## Overview

Across issue #227's remediation cycles, `QfcItemController`'s coverage-exemption boundary was
reduced from 103 to 19 members. The 9 residual controller members in that boundary are all blocked
by the same structural gap: they `await itemViewer.UiSyncContext` (or post continuations through
the ambient `WindowsFormsSynchronizationContext`), and this repository has no WinForms analogue of
the WPF `Dispatcher.Run()`-on-a-background-thread test seams it already uses
(`StaDispatcherHost` in `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs`;
`StartRunningDispatcher()` in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`).
Awaiting a `WindowsFormsSynchronizationContext` continuation on a thread-pool MSTest thread hangs
indefinitely because no message loop drains the posted continuation — documented in-repo at
`UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs:362-374`.

This feature adds a `WinFormsPumpHost` test-support seam that runs a real WinForms message pump
(`Application.Run(ApplicationContext)`) on a dedicated STA background thread, plus the tests and a
minimal additive production change needed to de-exempt 8 of the 9 residual members. The design of
record is the research artifact
`research/2026-08-07T21-00-winforms-message-pump-seam-research.md` in this feature folder.

## Behavior

### `WinFormsPumpHost` seam

A small, self-contained test-support class in `QuickFiler.Test` (new file
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`, with self-tests in
`QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` placed per the project's existing test-file
layout). No new shared test-support project is created; `QuickFiler.Test` is the only project with
a committed consumer, and its existing references (`QuickFiler`, `UtilitiesCS`,
`TaskVisualization`) cover everything the seam needs.

**Startup and readiness handshake.** The constructor starts a dedicated `Thread` that is:
`IsBackground = true`, named (for example `"QuickFiler.Test.WinFormsPumpHost"`), and STA via
`SetApartmentState(ApartmentState.STA)` before `Start()`. The thread body, in order:
(a) explicitly installs `new WindowsFormsSynchronizationContext()` via
`SynchronizationContext.SetSynchronizationContext` (never relying on, or mutating, the
process-global `WindowsFormsSynchronizationContext.AutoInstall`); (b) copies
`SynchronizationContext.Current` and `Thread.CurrentThread.ManagedThreadId` into host fields;
(c) signals a `ManualResetEventSlim` (signalled in `finally` so a broken host cannot present as a
hang); (d) enters `Application.Run(new ApplicationContext())`. The constructor blocks on the
readiness event and rethrows any exception recorded during (a)-(b), so it never returns a
half-initialized host. Work posted between (c) and (d) queues on the marshaling window and is not
lost.

**Pump semantics.** `Application.Run(ApplicationContext)` runs a standard message loop with no
`Form`; the only windows involved are hidden message-only marshaling windows, so the seam is fully
headless. Because the WinForms context is installed before any `ItemViewer` is constructed on the
pump thread, `Control`'s auto-install logic leaves it in place and `itemViewer.UiSyncContext`
captures the host's context instance; the `UiThread` awaiter's reference-equality `IsCompleted`
check then short-circuits for code already running on the pump.

**Posting members.** Only `Task`-returning members are exposed — no synchronous `Invoke`-style
member exists, which structurally prevents the test thread and pump thread from blocking on each
other. `InvokeAsync(Action)` / `InvokeAsync<T>(Func<T>)` run synchronous work on the pump;
`RunAsync(Func<Task>)` / `RunAsync<T>(Func<Task<T>>)` start async work on the pump with unwrapped
completion. Every posted delegate is wrapped in `try/catch` →
`TaskCompletionSource.TrySetException`, so a failure faults the returned task with the original
exception.

**Shutdown and disposal contract.** `StopAsync()`: optionally retires any WPF dispatcher the pump
thread lazily created (`Dispatcher.FromThread(_thread)?.InvokeShutdown()`), posts
`Application.ExitThread` onto the pump via the captured context, awaits a
`TaskCompletionSource<bool>(RunContinuationsAsynchronously)` completed in a `finally` around
`Application.Run`, then `_thread.Join()`, then throws `InvalidOperationException` if the thread is
still alive, then rethrows any recorded pump-thread exceptions, then disposes the readiness event.
`Dispose()` is the idempotent synchronous bridge to `StopAsync()`; double-`Dispose` is a no-op.
After stop, posting members fault their returned task with `ObjectDisposedException` (fail fast
rather than silently queueing to a dead loop).

**Exception marshalling — three channels.**
1. Async members under test: the async state machine captures exceptions into the returned `Task`;
   the test awaits and observes the fault on the MSTest thread.
2. Host-posted delegates: `try/catch` → `TrySetException` on the per-call task (above).
3. Stray pump-loop exceptions: the thread body subscribes `Application.ThreadException` and records
   exceptions into a host list; `StopAsync()` rethrows the first recorded exception (aggregated if
   several). A swallowed-dialog or quiet failure therefore becomes a test failure at disposal.

**Usage contract.** One host per test (or per test class where several tests share one
`ItemViewer`), always released in `finally`/`using`. The `ItemViewer` under test is constructed via
`host.InvokeAsync(() => new ItemViewer())`, so no `SynchronizationContext` is ever installed or
mutated on the MSTest thread.

### API shape (net481-safe)

```csharp
internal sealed class WinFormsPumpHost : IDisposable
{
    internal WinFormsPumpHost();                          // starts the pump; blocks until ready; rethrows init failure

    internal SynchronizationContext SyncContext { get; }  // the pump's WindowsFormsSynchronizationContext
    internal int ThreadId { get; }                        // pump thread's ManagedThreadId

    internal Task InvokeAsync(Action action);             // run sync work on the pump
    internal Task<T> InvokeAsync<T>(Func<T> factory);     // e.g. host.InvokeAsync(() => new ItemViewer())
    internal Task RunAsync(Func<Task> asyncWork);         // start async work on the pump; unwrapped completion
    internal Task<T> RunAsync<T>(Func<Task<T>> asyncWork);

    internal Task StopAsync();                            // post ExitThread, await stopped, join, surface pump faults
    public void Dispose();                                // idempotent synchronous bridge to StopAsync
}
```

### Additive production change: static-factory seam parameters

`CreateAsync` and `CreateSequentialAsync`
(`QuickFiler/Controllers/QfcItemController.Initialization.cs`) internally construct a
`QfcItemController`, call `SaveParameters(...)`, and await initialization. `SaveParameters` applies
production defaults via `??=`, including `_webViewInitializer ??= new WebView2CoreInitializer()`
(the real WebView2 adapter), and there is no injection point between `SaveParameters` and the
awaited init. Driving the factories as-written from a test would invoke the real WebView2 runtime —
an external dependency barred by the unit-test policy.

This spec therefore includes a minimal, non-breaking API extension: optional seam parameters on
both static factories (at minimum `IUiDispatcher uiDispatcher = null`,
`IWebViewCoreInitializer webViewInitializer = null`,
`Func<MailItem, ConversationResolver> conversationResolverFactory = null`), assigned to the
controller's fields before `SaveParameters`, mirroring the primary constructor's existing
optional-seam pattern. Defaults preserve current behavior; existing call sites compile and behave
unchanged. Without this change the achievable exemption reduction is 19 → 13 instead of 19 → 11.

## Inputs / Outputs

- Inputs: none at runtime; the seam is test-only infrastructure consumed by MSTest tests in
  `QuickFiler.Test`. The factory seam parameters are optional constructor-style inputs defaulting
  to `null` (production defaults preserved).
- Outputs (evidence artifacts, canonical locations per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`):
  - Pre-change exemption census and coverage baseline →
    `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/`
  - Post-change census, coverage, and toolchain evidence →
    `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/`
- Config keys and defaults: none.
- Versioning / backward compatibility: the factory signature change is additive with defaulted
  parameters — no breaking change to any public API.

## Data & State

- No persistence, caching, migration, or production data-flow changes. The seam owns exactly one
  pump thread, its `WindowsFormsSynchronizationContext`, and its recorded-exception list; all are
  created in the constructor and torn down deterministically in `StopAsync`/`Dispose`.
- Invariants: constructor returns ⇔ pump live and context captured; a posted work item's task
  completes ⇔ the work ran to completion on the pump thread; `StopAsync` returns ⇔ loop exited,
  thread joined, no recorded pump faults. Posts through one context execute in post order.
- Coverage denominator state change: removing 8 `[ExcludeFromCodeCoverage]` attributes adds those
  members to the coverage denominator; the covering tests land in the same change as each removal.

## Constraints & Risks

### Platform constraint: net481

`QuickFiler.Test` and the production projects target `TargetFrameworkVersion v4.8.1` (non-SDK,
`packages.config`). net48x has no `IsExternalInit` polyfill, so `init` accessors, `record`, and
`record struct` fail with CS0518. The host must be a plain `sealed class` with get-only properties
backed by fields assigned in the constructor/thread body. Nothing in the design requires newer
language features.

### Determinism requirements (`.claude/rules/general-unit-test.md`, Determinism Infrastructure)

- No `Thread.Sleep`, no `Task.Delay`, no wall-clock polling anywhere in the seam or its consuming
  tests. All coordination uses `ManualResetEventSlim` (readiness),
  `TaskCompletionSource<T>` with `RunContinuationsAsynchronously` (completion/stopped), and
  awaiting the member's own returned `Task` (progress) — the same primitives as the accepted
  in-repo precedents.
- All waits are bounded by deterministic signals; a failure inside pumped work must surface as a
  test failure (faulted awaited task, constructor rethrow, or `StopAsync` rethrow), never as a CI
  timeout. MSTest `[Timeout(...)]` on the new test files is a permitted harness bound (in-repo
  precedent `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`), not a wall-clock wait in test
  logic.
- No temporary files in any test (repository unit-test policy; no approved exceptions).

### Risks and mitigations (carried forward from the research)

| Risk | Mitigation |
|---|---|
| Pump-thread leak across tests | `IsBackground = true` + named thread; `Dispose` in `finally`/`using` per test; `StopAsync` joins and throws if the thread is still alive. |
| Hung pump converts a failure into a CI timeout | Posted-`ExitThread` + stopped-TCS + join sequence mirrors the accepted WPF host; no synchronous host API can block the pump on the test thread; `[Timeout]` belt on the new test files. |
| `SynchronizationContext` bleed into sibling tests | The host never touches the MSTest thread's context; all installation happens on the pump thread; `ItemViewer` is constructed via `host.InvokeAsync`; `AutoInstall` is never mutated. |
| MSTest parallelization interaction | `TaskMaster.runsettings` runs class-level parallelization; therefore no static or shared host — one host instance per test (or per class) and no static mutable state in the host. Existing static dispatcher infrastructure in `QfcItemController.TestSupport.cs` is left untouched. |
| STA apartment requirement vs MSTest default (MTA) | Only the pump thread is STA, set via `SetApartmentState(ApartmentState.STA)` before `Start()`; the test thread's apartment is never changed (identical to both existing hosts). |
| WebView2 initialization on a pumped thread | Real WebView2 init is never initiated: `IWebViewCoreInitializer` is always mocked; bare `WebView2` control construction inside `InitializeComponent` is already proven safe headless by #227 cycle-5 tests; `InitializeWebViewAsync` stays exempt. |
| Fire-and-forget faults (`_ = InitializeWebViewAsync()`) leaking unobserved exceptions | The mocked web-view seam faults fast and deterministically; async-method faults land in the discarded `Task`, not the loop; non-Task marshaled throws are caught by the `Application.ThreadException` recorder and surfaced at `StopAsync`. |
| WPF-dispatcher assumptions on the pump thread | `Initialize(bool)`'s tail dispatches through the WPF `Dispatcher` captured on the pump thread. This interop (WPF dispatcher serviced by a WinForms loop) has no in-repo proof yet; the host self-test file must include a smoke test proving both marshal routes before any controller test relies on it. `StopAsync` also retires the thread's WPF dispatcher. |
| Coverage floor regression when removing attributes | Attributes are removed member-by-member only in the same change that adds the covering test; coverage evidence is re-run per the C# toolchain gate against the captured baseline. |

## Implementation Strategy

- Implementation scope:
  - New test-support class `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` and self-test file
    `WinFormsPumpHostTests.cs` (MSTest + Moq + FluentAssertions only, per CUT1/CUT2).
  - New/extended member tests in the existing per-cluster `QfcItemController.*Tests.cs` files
    exercising the 8 target members through the pump host and the established harness pattern
    (`HarnessController` + `QfcItemControllerTestSupport.SetField`, mocked
    `IWebViewCoreInitializer`, mocked factories).
  - Additive optional seam parameters on `CreateAsync`/`CreateSequentialAsync` (production change,
    non-breaking).
  - Removal of 8 `[ExcludeFromCodeCoverage]` attributes; updated justification comment on
    `InitializeWebViewAsync`'s retained attribute.
  - Exemption-census re-baseline before and after the change (see Acceptance Criteria).
- Suggested implementation order (from the research): host + self-tests →
  `ResolveControlGroupsAsync` (smallest pump-only member) → `InitializeSequentialAsync` /
  `InitializeGraphicsAsync` → `Initialize(bool)` + 9-arg overload → factory seam change +
  `CreateAsync`/`CreateSequentialAsync` → `InitializeAsync` → attribute removals +
  boundary/evidence re-baseline.
- Dependency changes: none. No new packages; no new test-support project.
- Logging/telemetry: none (test infrastructure).
- Rollout: no flags; the change is test-side plus a defaulted-parameter production extension.
- Governance note: the resulting boundary change (19 → 11) touches a maintainer-ratified exemption
  boundary; per the #227 precedent, the reduced boundary evidence is re-ratified by the maintainer
  as a PR-lifecycle review step. This is an approval step, not a manual step in building or
  running the tests.

## Non-Goals

1. **De-exempting `InitializeWebViewAsync`.** The pump seam removes its
   `await _itemViewer.UiSyncContext` barrier, but its later lines dereference
   `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`, which is null unless the real WebView2
   runtime initialized the control — an external-process dependency barred by the unit-test
   policy. With the mocked `IWebViewCoreInitializer`, execution must stop at the seam call
   (controlled fault), so the member cannot be meaningfully covered end-to-end. It retains its
   `[ExcludeFromCodeCoverage]` attribute with an updated justification comment (pump barrier
   resolved; residual barrier = CoreWebView2/WebView2 runtime). Issue #230 itself tracks the
   residual concrete-`ItemViewer` accessor barrier separately. The maximum achievable outcome of
   this feature is 8 of the 9 residual members de-exempted (boundary 19 → 11).
2. **Addressing `EnsureBreadcrumbPipeline`** (`QfcItemController.ViewerSetup.cs:132`), added by
   issue #351 after the 2026-07-02 ratification. It is outside #230's 9-member scope; the census
   re-baseline documents it rather than changing it.
3. **Creating a shared test-support project.** The host lives in `QuickFiler.Test`; promotion to a
   shared project is a follow-up if `UtilitiesCS.Test` gains a real consumer.
4. **Refactoring production so the affected members no longer await `UiSyncContext`.** Rejected by
   the research as a behavior-risk rewrite contrary to #230's test-infrastructure charter.
5. **Reusing the WPF `StartRunningDispatcher()` thread for WinForms work.** Rejected (WinForms
   per-thread state leak, lower production fidelity); documented as fallback only.

## Acceptance Criteria

- [x] `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` exists as an `internal sealed class`
      implementing `IDisposable` with the API shape in this spec (readiness-handshake constructor,
      `SyncContext`, `ThreadId`, both `InvokeAsync` overloads, both `RunAsync` overloads,
      `StopAsync`, idempotent `Dispose`), compiles for net481, and uses no `init` accessors,
      `record`, or `record struct`.
      <br/>Evidence: `evidence/other/pump-host-selftests.2026-08-07T22-05.md` (P1-T7) — build
      EXIT_CODE 0 for net481; `Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread`
      passing.
- [x] The seam is unit-tested in its own right: `WinFormsPumpHostTests.cs` verifies work executes
      on the pump thread (thread-id assertions for `InvokeAsync`, `RunAsync`, and
      `await host.SyncContext`), fault propagation (synchronous throw and async fault surface on
      the awaited task), post-after-stop calls fault with `ObjectDisposedException`,
      double-`Dispose` is a no-op, and a recorded `Application.ThreadException` fault is rethrown
      by `StopAsync`.
      <br/>Evidence: `evidence/other/pump-host-selftests.2026-08-07T22-05.md` (P1-T7) — 13/13
      passing, covering every listed scenario.
- [x] Host self-test smoke assertion: a test in `WinFormsPumpHostTests.cs` proves both marshal
      routes — `await host.SyncContext` and WPF `Dispatcher.FromThread(<pump>).InvokeAsync` —
      execute on the pump thread, establishing the WPF-dispatcher-serviced-by-WinForms-loop
      interop before any controller test relies on it.
      <br/>Evidence: `evidence/other/pump-host-selftests.2026-08-07T22-05.md` (P1-T7) —
      `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` passing (49 ms).
- [x] Neither `WinFormsPumpHost` nor any test added by this feature uses `Thread.Sleep`,
      `Task.Delay`, wall-clock polling, or unbounded waits without a deterministic completion
      signal; the new test files carry MSTest `[Timeout]` attributes as the harness bound.
      <br/>Evidence: `evidence/qa-gates/determinism-audit.2026-08-07T23-35.md` (P7-T2) —
      `BANNED_HITS=0`, `SPIN_HITS=0`, and all 21 feature-added tests carry `[Timeout]`.
- [x] Pre-change census re-baseline: an evidence artifact under
      `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/`
      enumerates every `[ExcludeFromCodeCoverage]` site in
      `QuickFiler/Controllers/QfcItemController.*.cs` (expected: 19 sites), cross-references the
      2026-07-02 ratified boundary, and identifies `EnsureBreadcrumbPipeline`
      (`QfcItemController.ViewerSetup.cs:132`, added by #351) as post-ratification and outside
      #230's scope. A pre-change coverage baseline is captured in the same location.
      <br/>Evidence: `evidence/baseline/exclusion-census-pre.2026-08-07T21-50.md` (P0-T7 — all 19
      sites enumerated with file, line and member; ratified boundary cross-referenced;
      `EnsureBreadcrumbPipeline` identified as post-ratification) and
      `evidence/baseline/baseline-test-coverage.2026-08-07T21-52.md` plus
      `evidence/baseline/coverage-baseline.cobertura.xml` (P0-T6 — line-rate 0.856453,
      branch-rate 0.790039).
- [x] `CreateAsync` and `CreateSequentialAsync` gain optional seam parameters (defaulting to
      `null`) assigned to controller fields before `SaveParameters`, mirroring the primary
      constructor's optional-seam pattern; all existing call sites compile unchanged and default
      behavior is preserved (non-breaking, verified by the existing test suite passing).
      <br/>Evidence: `evidence/other/factory-seam-verification.2026-08-07T23-00.md` (P5-T2 —
      additive-only signature change; zero in-repo callers, so no call site could break) and
      `evidence/other/factory-tests.2026-08-07T23-15.md` (P5-T6 — build EXIT_CODE 0, 9/9 tests
      passing).
- [x] Each of the 8 target members — `Initialize` (9-arg), `Initialize(bool)`, `InitializeAsync`,
      `InitializeGraphicsAsync`, `InitializeSequentialAsync`, `CreateAsync`,
      `CreateSequentialAsync`, `ResolveControlGroupsAsync(ItemViewer)` — is exercised by at least
      one MSTest test that awaits the member through `WinFormsPumpHost` and asserts observable
      controller/viewer state, and each member's `[ExcludeFromCodeCoverage]` attribute is removed
      in the same change that adds its covering test(s).
      <br/>Evidence: `evidence/other/resolve-control-groups-tests.2026-08-07T22-15.md` (P2),
      `evidence/other/initialize-sequential-graphics-tests.2026-08-07T22-35.md` (P3),
      `evidence/other/initialize-overloads-tests.2026-08-07T22-55.md` (P4),
      `evidence/other/factory-tests.2026-08-07T23-15.md` (P5),
      `evidence/other/initialize-async-tests.2026-08-07T23-25.md` (P6), and
      `evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md` (P7-T1), whose table maps each
      of the 8 members to its removal phase and covering test.
- [x] `InitializeWebViewAsync` retains its `[ExcludeFromCodeCoverage]` attribute with an updated
      justification comment stating the pump barrier is resolved and the residual barrier is the
      CoreWebView2/WebView2 runtime dependency.
      <br/>Evidence: P6-T3 comment-only diff at
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (attribute retained, justification
      rewritten) and `evidence/other/initialize-async-tests.2026-08-07T23-25.md` (P6-T4).
- [x] Post-change census re-baseline: an evidence artifact under
      `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/`
      shows the 8 target attribute sites removed, `InitializeWebViewAsync` retained with updated
      justification, and `EnsureBreadcrumbPipeline` documented as out-of-scope, with the resulting
      site count recorded.
      <br/>Evidence: `evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md` (P7-T1) —
      `COUNT=11`, all 8 removals listed with their phase and covering test, boundary recorded as
      19 -> 11.
- [x] Repository line coverage does not regress relative to the pre-change baseline captured in
      `evidence/baseline/`, measured on the testable denominator per CLAUDE.md § UT2; post-change
      coverage evidence is recorded under `evidence/qa-gates/` and reports the per-member line
      coverage of the 8 newly de-exempted members.
      <br/>Evidence: `evidence/qa-gates/coverage-delta.2026-08-08T00-15.md` (P8-T6) — line-rate
      85.6453% -> 85.8333% raw (+0.1880 pts) and 85.8223% denominator-adjusted (+0.1769 pts);
      per-member figures 83.33%-100.00%, aggregate 92.98%, every member > 0%.
- [x] The full C# toolchain passes in a single clean final pass, in order: `csharpier .`; msbuild
      with `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true`; msbuild with
      `Nullable=enable` and `TreatWarningsAsErrors=true`; `vstest.console.exe` with
      `/EnableCodeCoverage`.
      <br/>Evidence: Phase 8 loop iteration 2, all four stages EXIT_CODE 0 —
      `evidence/qa-gates/final-format.2026-08-07T23-45.md` (P8-T1, `csharpier format`/`check`),
      `evidence/qa-gates/final-analyzers.2026-08-07T23-48.md` (P8-T3, 0 errors),
      `evidence/qa-gates/final-nullable.2026-08-07T23-50.md` (P8-T4, 0 errors),
      `evidence/qa-gates/final-test-coverage.2026-08-08T00-05.md` (P8-T5, 6293/6293 passing under
      `dotnet-coverage collect` wrapping `vstest.console.exe /InIsolation`, satisfying CUT3).
      Iteration 1 failed at P8-T5 and the loop was restarted from P8-T1; iteration 2 is the clean
      single pass.
- [x] No test added or modified by this feature creates or uses temporary files.
      <br/>Evidence: `evidence/qa-gates/determinism-audit.2026-08-07T23-35.md` (P7-T2) — zero
      `GetTempFileName`/`GetTempPath`/`Path.GetRandomFileName` hits, and the supplementary
      `File`/`Directory`/`Stream` scan returned only substring false positives (`autoFile.`,
      `capturedBlFile.`).
- [x] Every non-markdown file added or modified by this feature is at most 500 lines.
      <br/>Evidence: `evidence/qa-gates/file-size-audit.2026-08-07T23-46.md` (P8-T2, iteration-2
      section) — all 10 files measured after the final csharpier pass; largest is
      `QfcItemController.Initialization.cs` at 489.

## Seeded Test Conditions (from potential)

- [ ] Unit coverage areas: `WinFormsPumpHost` lifecycle (start/ready, post/run, stop/dispose,
      fault channels); the 8 target `QfcItemController` members via the pump host.
- [ ] Integration scenarios: none — the seam exists precisely to keep these unit tests free of
      external processes (no Outlook, no WebView2 runtime).
- [ ] CLI/API examples: canonical usage documented in this spec's Behavior section
      (`using (var host = new WinFormsPumpHost()) { var viewer = await host.InvokeAsync(() => new ItemViewer()); ... }`).
