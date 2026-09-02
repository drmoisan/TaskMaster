# 2026-08-28-qfc-initializewebviewasync-fault-is-unobserved (Spec)

- **Issue:** #670
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-40
- **Status:** Approved
- **Version:** 0.2

## Context
`QfcItemController.InitializeWebViewAsync` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:48`)
returns a `Task` that **three of its four production call sites discard**, so any exception it raises becomes an
unobserved task exception rather than a diagnostic anyone sees. The method is the sole entry point for WebView2
environment creation, core initialization, and — at `ViewerSetup.cs:112` — the call to `EnsureBreadcrumbPipeline()`.
Issue #488's D5 fix makes that path newly capable of throwing `ObjectDisposedException` when the pipeline is built
against a viewer whose teardown has begun, which converts a previously silent leak into a fault that is itself
silently swallowed.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime/toolchain: .NET Framework 4.8.1, VSTO / WinForms; MSBuild, CSharpier 1.2.6, MSTest + Moq + FluentAssertions
- Command/flags used: n/a — identified by source reading during issue #488 execution, discharging research §3.5
- Data source or fixture: `QuickFiler/Controllers/QfcItemController.Initialization.cs` call sites

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The severity comes from the failure mode rather than the likelihood. A WebView2 initialization failure — a missing
runtime, a locked cache directory, a disposed viewer — produces no diagnostic on three of four paths, so the
breadcrumb surface simply never appears and the cause is unavailable to anyone triaging it.


## Repro & Evidence
Steps to Reproduce:
1. Drive a `QfcItemController` through any of the three fire-and-forget initialization paths listed under
   "Suspected Cause / Notes".
2. Arrange for `InitializeWebViewAsync` to fault — for example by disposing the `ItemViewer` before the posted
   continuation reaches `EnsureBreadcrumbPipeline()`, which after #488's D5 fix throws `ObjectDisposedException`.
3. Observe that no exception surfaces to the caller, no log entry is written by the call site, and initialization
   silently completes as far as any observer can tell.

Expected:
A faulted `InitializeWebViewAsync` should be observed by its caller — awaited, `ContinueWith`-observed, or routed to
the repository's logging pattern — so that a failure during WebView2 initialization is diagnosable rather than
invisible.

Actual:
The task is discarded at three of the four call sites, so the fault is never observed:

| Call site | Form | Observed? |
| --- | --- | --- |
| `QfcItemController.Initialization.cs:192` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` | **no** — discarded, and additionally wrapped in a WPF `DispatcherOperation` |
| `QfcItemController.Initialization.cs:256` | `await InitializeWebViewAsync();` | yes — awaited into the enclosing async method's task |
| `QfcItemController.Initialization.cs:288` | `_ = InitializeWebViewAsync();` | **no** — discarded |
| `QfcItemController.Initialization.cs:324` | `_ = InitializeWebViewAsync();` | **no** — discarded |

On .NET Framework 4.5 and later an unobserved task exception no longer terminates the process by default, so the
fault is finalized away with no observable effect at all.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: no captured log — that is the defect. Identified by source reading, recorded in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/d5-faulted-task-observation.md`.

Authoritative design input:
`docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/research/initializewebviewasync-fault-observation.2026-08-31T20-30.md`.
That record settles the remediation shape; this spec carries its decisions forward as the specified design rather
than as open options. All line numbers cited below were re-derived against the current tree while authoring this
spec.


## Scope & Non-Goals

In scope:
- A new production partial file `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` containing a
  fault boundary (`InitializeWebViewGuardedAsync`) and an injectable observation policy
  (`WebViewInitializationErrorSink`).
- Exactly one added `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` for that new file.
- Exactly three call-expression substitutions in `QuickFiler/Controllers/QfcItemController.Initialization.cs`, at
  lines 192, 288 and 324.
- Three new MSTest tests added to the existing
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.

Out of scope / non-goals:
- **`QfcItemController.Initialization.cs:256` is deliberately left unchanged.** That site is
  `await InitializeWebViewAsync();` inside `public async Task InitializeAsync()` (declared at
  `QfcItemController.Initialization.cs:202`), so its fault is already observed by the enclosing task. Routing it
  through the guard would *swallow* the fault that the currently passing test
  `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`
  (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:245`) asserts. Leaving it alone is a
  deliberate decision, not an omission.
- **`InitializeWebViewAsync` itself is not edited.** It carries
  `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:47`, so a guard placed inside it would add zero covered
  lines and zero measurable regression surface. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is not
  modified by this issue at all.
- **`QuickFiler/Controllers/EfcItemController.cs:97` and `:153` are out of scope.** Both are
  `Task.Run(() => InitializeWebViewAsync());` against that class's own same-named member
  (`EfcItemController.cs:174`). `EfcItemController` is class-level `[ExcludeFromCodeCoverage]`
  (`EfcItemController.cs:25`) and has no injectable WebView2 seam — its `InitializeWebViewAsync` calls
  `CoreWebView2Environment.CreateAsync` directly — so a fix there yields zero covered lines and admits no
  deterministic regression test. This must be promoted as its own potential entry through the promotion lifecycle
  (suggested slug `efc-item-controller-initializewebviewasync-fault-is-unobserved`) rather than folded into #670.
- **`TaskScheduler.UnobservedTaskException` at the add-in boundary is not adopted.** It fires only at finalization,
  is process-global, would land in `TaskMaster/ThisAddIn.cs` outside this issue's file scope, has no in-repo
  precedent, and cannot be regression-tested deterministically. It may be captured separately as a backstop if
  wanted.
- **`void Initialize(bool async)` is not converted to `async Task`.** It is declared on the public interface
  `QuickFiler/Interfaces/IQfcItemController.cs:25` and has three production callers
  (`QuickFiler/Controllers/QfcCollectionController.cs:710`, `:1870`, `:1918`); converting it is a breaking API
  change and is not required by this fix.
- **Sites 288 and 324 are not converted to `await`.** `InitializeGraphicsAsync` is awaited inside a serial loop
  over item groups (`QuickFiler/Controllers/QfcCollectionController.cs:444-447`, second site at `:539`) and
  `InitializeSequentialAsync` is awaited by the controller factory at
  `QuickFiler/Controllers/QfcItemController.Initialization.cs:485`. Awaiting would insert a full WebView2
  out-of-process handshake into each iteration and into controller construction, which is exactly the cost the
  "Fire and forget WebView initialization" comment at `QfcItemController.Initialization.cs:191` exists to avoid.

Explicitly excluded systems, integrations, or datasets:
- No change to `QuickFiler/Viewers/ItemViewer.cs`, `QuickFiler/Viewers/IItemViewer.cs`, or
  `UtilitiesCS/Threading/WpfUiDispatcher.cs`. These are read for reference only.
- No change to `coverage.config`, `.csharpierignore`, or any analyzer configuration.
- No live WebView2 runtime, no Outlook process, no network, no filesystem writes in tests.

## Root Cause Analysis
The three discarding sites were written as deliberate fire-and-forget dispatches, with the comment "Fire and forget
WebView initialization" at `Initialization.cs:191`. The intent — not blocking initialization on a WebView2 round trip
— is sound; discarding the fault is the part that is not.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler/Controllers/QfcItemController.Initialization.cs` are owned by feature
`qfc-item-controller-defects-484`, so this was **not** fixed inside #488. Research §3.5 required that if the task
proves unobserved the correct response is a new issue against `ViewerSetup.cs`, **not** a weakening of D5's guard.
D5's guard is delivered unweakened.

Two structural facts complete the analysis:

1. **Site 192 is doubly nested.** `_itemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher`
   (`QuickFiler/Viewers/IItemViewer.cs:36`), not the repository's `IUiDispatcher` seam. A method group returning
   `Task` has no method-group conversion to `Action`, so overload resolution binds
   `DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task`. The discarded
   expression is therefore a `DispatcherOperation<Task>`, and observing it observes only the dispatch, not the
   WebView2 work.
2. **The logger is not injectable.** `private static readonly log4net.ILog logger` is declared at
   `QuickFiler/Controllers/QfcItemController.cs:30`. A test cannot substitute it, and the
   `log4net.Appender.MemoryAppender` alternative supplies no completion signal, so asserting through it would
   require polling — a wall-clock wait banned by `.claude/rules/general-unit-test.md`. The fix must therefore route
   through an injectable seam to be testable at all.

The defect was anticipated and explicitly deferred by the #230 work:
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:126-127` reads "The discarded task's
fault path is deliberately not asserted (research section 9)." Issue #670 is the discharge of that deferral.


## Proposed Fix

### Design summary (what changes where)

Adopt a shared fault boundary with an injectable observation policy — the shape already ratified in this repository
under issue #464 for the sibling `EfcFormController`
(`QuickFiler/Controllers/EfcFormController.cs:127-129`, consumed by tests in
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs`).

Add one new production partial file carrying two members: an `Action<string, Exception>` sink defaulting to the
static log4net logger, and an `async Task` guard that awaits `InitializeWebViewAsync()` inside a `try` and contains
any fault by routing it to the sink. Then substitute the guarded member for the raw member at the three discarding
call sites. The observed call site at line 256 and the excluded `InitializeWebViewAsync` member are untouched.

The guard makes the fix directly awaitable in a unit test, which removes the pump and the dispatcher from two of the
three test assertions entirely.

### Boundaries and invariants to preserve

- **Fire-and-forget latency is preserved.** No call site gains an `await`. `Initialize(bool)` remains synchronous;
  `InitializeGraphicsAsync` and `InitializeSequentialAsync` still return before WebView2 initialization completes.
- **`IQfcItemController` is unchanged.** No public API signature changes; no caller in
  `QuickFiler/Controllers/QfcCollectionController.cs` changes.
- **The already-observed path stays observed.** Line 256 continues to propagate its fault to
  `InitializeAsync`'s returned task.
- **Cooperative cancellation is not a fault.** `InitializeWebViewAsync` opens with
  `Token.ThrowIfCancellationRequested()` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:52`) and the
  token is cancelled during normal QuickFiler teardown, so `OperationCanceledException` must be swallowed without
  reaching the sink. This mirrors `EfcFormController.BindBreadcrumbRowsAsync`.
- **The 500-line file ceiling holds for every touched file** (`.claude/rules/general-code-change.md`, "File Size
  Limit").
- **No production file is excluded from coverage by this change**, and no existing
  `[ExcludeFromCodeCoverage]` attribute is added, removed, or moved.

### Dependencies or blocked work

- None blocking. The mocked `IWebViewCoreInitializer` seam that supplies the deterministic controlled fault already
  exists (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:243`, both members stubbed
  `.ThrowsAsync(new WebViewSentinelException())` at `:253` and `:261`; sentinel type declared at `:269`).
- Issue #511's determinism work on the pump-hosted test files has already landed on this tree, so the historical
  test-file overlap is not an in-flight collision.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

| File | Change | Current lines |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` | **new file** — sink property and guard method | n/a |
| `QuickFiler/QuickFiler.csproj` | one added `<Compile Include>` entry, adjacent to `:333` | n/a |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | three call-expression substitutions at `:192`, `:288`, `:324` | 489 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | three added tests | 398 |

A new production file is **mandatory**, not stylistic: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is
499 lines against the repository's 500-line ceiling (independently re-verified while authoring this spec: 499), so
the members cannot land there. `QuickFiler/Controllers/QfcItemController.Initialization.cs` is 489 lines and has
room only for the three substitutions, which are net-zero-line replacements plus at most a short `#670` comment.

Tests go in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (398 lines, roughly 102
lines of headroom), which already carries a `<Compile Include>` entry, so **no `QuickFiler.Test.csproj` edit is
required**. Do **not** place the tests in `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`, which
is 498 lines and has 2 lines of headroom.

`QuickFiler/QuickFiler.csproj` enumerates the `QfcItemController` partials explicitly at `:331-340` with no
wildcard, so the new partial requires exactly one added `<Compile Include>` line. `.csharpierignore:12` excludes
`*.csproj` from CSharpier, so that edit is not reformatted and does not participate in the format check.

#### Functions/classes/members impacted

- **New:** `QuickFiler.Controllers.QfcItemController.WebViewInitializationErrorSink` (property).
- **New:** `QuickFiler.Controllers.QfcItemController.InitializeWebViewGuardedAsync()` (method).
- **Modified call expressions only:** `QfcItemController.Initialize(bool)` (declared
  `QfcItemController.Initialization.cs:168`), `QfcItemController.InitializeGraphicsAsync()` (declared `:263`),
  `QfcItemController.InitializeSequentialAsync()` (declared `:295`). Each of these three enclosing members carries
  an explicit "#230: de-exempted" comment (`:164-167`, `:259-262`, `:291-294`) and no coverage attribute, so all
  three edited lines are already executed by existing tests.
- **Unchanged:** `QfcItemController.InitializeWebViewAsync()`, `QfcItemController.InitializeAsync()`,
  `IQfcItemController`.

#### Data flow and validation changes

None. No data model, no serialization format, no validation rule changes. The only flow change is that an exception
raised inside `InitializeWebViewAsync` now terminates at the sink instead of propagating into a discarded task.

#### Error handling and logging updates

- `OperationCanceledException` is caught and swallowed as non-fault, with a comment recording that cooperative
  cancellation during teardown is expected.
- Every other `Exception` is routed to `WebViewInitializationErrorSink` with a message identifying WebView2
  initialization and the exception instance.
- The default sink writes through the existing static log4net logger at `QuickFiler/Controllers/QfcItemController.cs:30`.
- **Logging call form is message-first:** `logger.Error(string message, Exception exception)`. This is the form used
  throughout this type (`QfcItemController.Conversation.cs:70`, `QfcItemController.FolderHandling.cs:97` and `:103`)
  and by the sibling sink at `EfcFormController.cs:129`. The exception-first form `logger.Error(ex, "...")` is a
  Serilog/NLog idiom, appears nowhere in this repository, and would not compile against `log4net.ILog`.
- No new log level, appender, category, or configuration is introduced.

#### Rollback / feature-flag considerations

No feature flag. The change is three call-expression substitutions plus one additive file; reverting the commit
restores the previous behavior exactly.

### Technical specifications (interfaces/contracts)

New file `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`:

- Namespace `QuickFiler.Controllers`; type `internal partial class QfcItemController`, matching the declaration
  style at `QuickFiler/Controllers/QfcItemController.Initialization.cs:23-26`.
- Accessibility is `internal` for both members. `QfcItemController` is itself `internal`
  (`QuickFiler/Controllers/QfcItemController.cs:25`), so `public` would be meaningless, and `internal` matches
  `EfcFormController.BoundaryErrorSink` and every existing seam on this type.
- The file must **not** carry `#nullable enable`. Neither sibling partial does, the repository is per-file opt-in
  with no `Directory.Build.props`, and adding the directive would conscript the file into the
  `TreatWarningsAsErrors` gate for no benefit.

Member contracts:

- `internal System.Action<string, System.Exception> WebViewInitializationErrorSink { get; set; }`
  - Default value: `(message, exception) => logger.Error(message, exception)`.
  - Named distinctly from `EfcFormController.BoundaryErrorSink` so no shared contract between the two types is
    implied.
  - Contract: invoked at most once per guarded invocation, on the thread that observed the fault. Never invoked for
    `OperationCanceledException`.
- `internal async Task InitializeWebViewGuardedAsync()`
  - Awaits `InitializeWebViewAsync()` inside `try`.
  - `catch (OperationCanceledException)` — swallowed, sink not invoked.
  - `catch (Exception ex)` — invokes `WebViewInitializationErrorSink` with a WebView2-identifying message and `ex`;
    does not rethrow.
  - Contract: the returned `Task` never transitions to `Faulted`.

Call-site edits (exactly three, all in `QuickFiler/Controllers/QfcItemController.Initialization.cs`):

- `:192` becomes `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);`
- `:288` becomes `_ = InitializeWebViewGuardedAsync();`
- `:324` becomes `_ = InitializeWebViewGuardedAsync();`

**No `.Unwrap()` is required at site 192 after this change.** The dispatched delegate becomes an `async Task` method
that catches `Exception`, so its returned `Task` cannot transition to `Faulted`, and an `async` method never throws
out of its invocation — all exceptions, including ones raised before the first `await`, are captured into the
returned task. The discarded `DispatcherOperation<Task>` therefore carries no observable fault. (Dispatcher shutdown
surfaces as abort/cancellation, not a fault.)

#### Inputs/outputs and formats

No serialized inputs or outputs. The sink signature is `(string message, Exception exception)`; the guard takes no
parameters and returns `Task`.

#### Required configuration keys and defaults

None. The sink's default is a code-level default; there is no configuration key.

#### Backward-compatibility expectations

Fully backward compatible. No public API changes, no interface changes, no behavior change on the success path or on
the already-observed path at line 256. The only observable difference is that a WebView2 initialization failure on
the three fire-and-forget paths now produces a log entry instead of nothing.

#### Performance constraints (latency/throughput/memory)

No measurable change. One additional `async` state machine allocation per initialization, against an operation that
already performs an out-of-process WebView2 environment negotiation. No call site gains a blocking wait, so
per-item and per-controller initialization latency is unchanged.

## Assumptions, Constraints, Dependencies
- Assumptions: the mocked `IWebViewCoreInitializer` continues to raise `WebViewSentinelException` deterministically
  at `CreateEnvironmentAsync`, the first seam call inside `InitializeWebViewAsync`
  (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:70-73`); the `WinFormsPumpHost` seam
  (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`) continues to service the viewer's WPF dispatcher from its
  WinForms loop, as pinned by `WinFormsPumpHostTests.BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread`.
- Constraints: 500-line ceiling per file; `ViewerSetup.cs` has 1 line of headroom and cannot host the fix;
  `ViewerSetupTests.cs` has 2 lines of headroom and cannot host the tests; MSTest + Moq + FluentAssertions only;
  no temporary files; no external processes; no wall-clock waits.
- External dependencies: none added. No NuGet package changes.

## Data / API / Config Impact
- User-facing or API changes: none. `IQfcItemController` and all public signatures are unchanged.
- Data or migration considerations: none.
- Logging/telemetry updates: WebView2 initialization failures on the three fire-and-forget paths now emit a
  `logger.Error(message, exception)` entry through the existing log4net logger. No new appender, level, or category.
- Compatibility notes: one added `<Compile Include>` line in `QuickFiler/QuickFiler.csproj`; no change to any other
  project file, to `coverage.config`, or to `.csharpierignore`.

## Test Strategy

Seeded from issue:

- [ ] Unit coverage areas: a test that forces `InitializeWebViewAsync` to fault at the mocked web-view seam and asserts the fault is observed and logged rather than discarded
- [ ] Integration scenario to retest: dispose an `ItemViewer` mid-initialization and confirm the resulting `ObjectDisposedException` reaches a log
- [ ] Manual verification notes: confirm the three fire-and-forget sites still do not block initialization after the change

### Bugfix-workflow sequencing (RED step)

The repository bugfix workflow requires a failing regression test first. That cannot be applied literally here: the
primary test asserts against `InitializeWebViewGuardedAsync` and `WebViewInitializationErrorSink`, neither of which
exists before the fix, so the test would fail to *compile* rather than fail to *pass*, and a non-compiling test
assembly is not a usable red signal. The delivery sequence is therefore:

1. Author `InitializeWebViewGuardedAsync` and `WebViewInitializationErrorSink` in the new partial file, plus the
   `QuickFiler/QuickFiler.csproj` entry, so the members exist and the assembly compiles.
2. Author the test that asserts the sink is invoked with the seam fault.
3. Demonstrate the red state by removing the sink invocation from the guard's `catch (Exception)` arm and
   confirming the test fails, then restore it and confirm the test passes.
4. Apply the three call-site substitutions and add the remaining two tests.

Step 3 is the substantive red step and must be recorded in the delivery run's evidence.

### Regression tests to add

All three live in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. No new test file is
created.

1. `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` — the core assertion.
   Arrange a `HarnessController` (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:28`); inject
   `_webViewInitializer` with the faulting mock and `_uiDispatcher` with
   `QfcItemControllerTestSupport.BuildSyncDispatcher()` (`TestSupport.cs:105`) via
   `QfcItemControllerTestSupport.SetField` (`TestSupport.cs:40`); inject an `IItemViewer` mock whose
   `UiSyncContext` returns a plain `SynchronizationContext` so the await at `ViewerSetup.cs:64` completes; capture
   the sink. Act by awaiting `InitializeWebViewGuardedAsync()` directly. Assert `NotThrowAsync` and that the
   captured exception is `WebViewSentinelException`. No pump, no dispatcher, no `[Timeout]`.
2. `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` — covers the default lambda body so it is
   not always replaced by a test double. Mirrors
   `EfcFormControllerTests.BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`.
3. `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` — the site-192 dispatcher path,
   pump-hosted. Build the pump harness, install a signalling sink on the controller **before** invoking
   `Initialize(async: false)` (the dispatched operation may complete before `host.InvokeAsync` returns), then await
   the `TaskCompletionSource` and assert the exception type. Teardown follows the existing
   `finally { harness?.Restore(); await host.StopAsync(); }` shape.

Sites 288 and 324 receive no dedicated pump-hosted test. The change at those sites is a call-expression
substitution on a line already executed by existing passing tests
(`InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` at `Part3.cs:83` and
`InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` at `:40`), and the behavioral content of
the fix at those sites lives entirely inside `InitializeWebViewGuardedAsync`, which test 1 covers directly. Adding
two further 60-second-timeout pump tests for a call-expression change is disproportionate. If a reviewer wants
structural evidence beyond the existing tests, the substitute is a source assertion that the three sites name the
guarded member — the repository has precedent for that kind of structural pin.

### Pre-existing tests that must still pass unchanged

- `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` (`Part3.cs:40`)
- `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` (`Part3.cs:83`)
- `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` (`Part3.cs:245`)

The third is the pin that prevents an over-broad fix: if line 256 were routed through the guard, the fault it
asserts would be swallowed and the test would fail.

### Edge cases and negative scenarios

- Fault at the mocked seam — covered by tests 1 and 3.
- Default sink invoked directly — covered by test 2.
- `OperationCanceledException` — swallowed without reaching the sink; asserted implicitly by the sink capture in
  test 1 remaining unset when a cancelled token is supplied, if the planner elects to add that arm within the
  Part3.cs headroom.
- Successful `InitializeWebViewAsync` completion is **not** reachable in a unit test. Success requires a live
  CoreWebView2 runtime, an external process barred by policy. With a mock returning completed tasks, execution
  proceeds to `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` at `ViewerSetup.cs:85`, which is null without
  the real runtime, producing a `NullReferenceException` that the guard itself catches. That still exercises the
  guard but does not exercise a successful `InitializeWebViewAsync`. This is stated plainly rather than promised
  away.

### Determinism

No `Thread.Sleep`, no `Task.Delay`, no polling, no wall-clock wait. Tests 1 and 2 are effectively synchronous. The
only wait in test 3 is `await` on a `TaskCompletionSource` completed from the sink callback, guarded by the existing
`[Timeout(PumpTimeoutMs)]` attribute, whose documented role is to convert a genuine deadlock into a test failure
rather than to serve as a wait mechanism.

### Coverage impact and targets

The new partial file is a new module for policy purposes and must reach `>= 90%` line coverage per CLAUDE.md. Test 1
covers the guard's fault arm; test 2 covers the default sink lambda. The three edited call-site lines are already
covered by existing tests. No line is added inside a `[ExcludeFromCodeCoverage]` member.

### Toolchain commands to run (format → analyze → type-check → test)

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Do not add `/p:Nullable=enable` and do not substitute `/t:Build`; both deviations are prohibited by CLAUDE.md.

### Manual validation steps

Optional and not gating: with a real Outlook host, confirm that a QuickFiler session with the WebView2 runtime made
unavailable now writes a `logger.Error` entry naming the WebView2 initialization failure, and that item
initialization still returns without blocking.


## Acceptance Criteria

- [x] **AC1** — `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` exists, declares
  `namespace QuickFiler.Controllers` and `internal partial class QfcItemController`, carries no `#nullable enable`
  directive, and is compiled by exactly one added `<Compile Include="Controllers\QfcItemController.WebViewFaultBoundary.cs" />`
  entry in `QuickFiler/QuickFiler.csproj`; `QuickFiler.Test/QuickFiler.Test.csproj` is unchanged, because the new
  tests land in the already-included `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.
  Verified by file existence, `git diff --stat` on the two `.csproj` files, and a successful build.
- [x] **AC2** — That file declares
  `internal System.Action<string, System.Exception> WebViewInitializationErrorSink { get; set; }` whose default
  value is `(message, exception) => logger.Error(message, exception)`, using the log4net message-first overload
  `ILog.Error(string, Exception)`. Verified by reading the declaration and by the build succeeding (the
  exception-first form does not exist on `log4net.ILog` and would not compile).
- [x] **AC3** — That file declares `internal async Task InitializeWebViewGuardedAsync()` which awaits
  `InitializeWebViewAsync()` inside a `try`, catches `OperationCanceledException` without invoking the sink,
  catches `Exception` and invokes `WebViewInitializationErrorSink` with the exception, and does not rethrow.
  Verified by reading the member and by AC4.
- [x] **AC4** — `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` exists in
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` and passes: the awaited guard does
  not throw, and the sink receives a `WebViewSentinelException`.
- [x] **AC5** — `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` exists in the same file and
  passes, exercising the default sink lambda body rather than a test double.
- [x] **AC6** — `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` exists in the same file and
  passes: driving `Initialize(async: false)` through the pump host delivers the seam fault to the sink.
- [x] **AC7** — `QuickFiler/Controllers/QfcItemController.Initialization.cs` lines 192, 288 and 324 each name
  `InitializeWebViewGuardedAsync`; no `.Unwrap()`, `ContinueWith`, or `await` is introduced at any of the three
  sites. Verified by a grep for `InitializeWebViewGuardedAsync` in that file returning exactly three executable
  call sites at those lines.
- [x] **AC8** — `QuickFiler/Controllers/QfcItemController.Initialization.cs:256` still reads
  `await InitializeWebViewAsync();`, calling the **unguarded** member, and
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is unmodified — including
  `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `:47` and the body of `InitializeWebViewAsync` at
  `:48`. Verified by `git diff --stat` showing zero changed lines in `ViewerSetup.cs` and by reading line 256.
- [x] **AC9** — The pre-existing tests
  `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`,
  `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`, and
  `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` all pass, with their method bodies and
  assertions unchanged. Verified by the test run result and by `git diff` on those test method bodies being empty.
- [x] **AC10** — The full four-stage C# toolchain completes in one clean pass, in this order, with no failure and no
  file rewritten by a later stage: (1) `dotnet tool run csharpier format .` verified by
  `dotnet tool run csharpier check .`; (2)
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
  (3) `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`;
  (4) `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. No `/p:Nullable=enable` and no `/t:Build`.
- [x] **AC11** — After the change, every touched file is at or below the 500-line ceiling: the new partial file,
  `QuickFiler/Controllers/QfcItemController.Initialization.cs`, and
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. Verified by line count per file.
- [x] **AC12** — The new tests introduce no determinism-banned API: no `Thread.Sleep`, no `Task.Delay`, no polling
  loop, and no real wall-clock wait. The only wait in the pump-hosted test is `await` on a
  `TaskCompletionSource` completed from the sink callback. Verified by grep over the added test code and by reading
  the test bodies.
- [x] **AC13** — `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` reaches `>= 90%` line coverage
  in the `/EnableCodeCoverage` report, per the CLAUDE.md new-module rule. Verified by the per-file figure in the
  generated coverage artifact.
- [x] **AC14** — Repository-wide line coverage does not regress relative to the Phase 0 baseline captured before any
  edit in this delivery run. Verified by comparing the Phase 0 and post-change coverage artifacts, both stored under
  `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/coverage/`.

**Note on the coverage-floor divergence (not resolved by this issue).** Two authorities in this repository state
different repository-wide floors: CLAUDE.md (General Unit Test Policy §UT2) states `>= 80%` line coverage with a
ratified COM/VSTO/WinForms testable-denominator exemption and `>= 90%` for new modules, while
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%` line and `>= 75%` branch
across all tiers together with a Coverage Exclusion Policy that forbids excluding production files at all — which is
in tension with the `[ExcludeFromCodeCoverage]` attributes this issue depends on. No acceptance criterion above
asserts a specific repository-wide percentage as a pass/fail number. AC13 uses the unambiguous new-module rule and
AC14 uses a no-regression comparison instead. Resolving the divergence is out of scope for #670 and should be raised
separately.

## Risks & Mitigations

Technical and operational risks:

- **File-size headroom on `ViewerSetup.cs` (admission condition).** `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
  is at 499 of 500 lines — one line of headroom — and
  `QuickFiler/Controllers/QfcItemController.Initialization.cs` is at 489. A concurrent merge into either file could
  invalidate this plan. **Phase 0 of the delivery run must re-verify these line counts against the then-current
  `main`** and confirm that `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` still has
  headroom, before committing anything.
- **In-flight branches whose feature folders are not visible from this tree.** Several `bug/*` branches have no
  feature folder on `main`, so their file claims cannot be read here. From branch names none targets the two
  production files in this change set, but this is unverified. Mitigation: the Phase 0 re-verification above, plus
  a rebase onto current `main` before the final toolchain pass.
- **Over-broad fix at line 256.** Routing the already-observed site through the guard would silently swallow the
  fault asserted by `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`. Mitigation: AC8 and
  AC9 pin the site and the test explicitly.
- **Sink installed after dispatch in the pump-hosted test.** The dispatched operation may complete before
  `host.InvokeAsync` returns, so a sink installed after the Act would miss the callback and the test would hang to
  its timeout. Mitigation: the test design installs the sink during Arrange, and this is called out in the Test
  Strategy.
- **Success path of the guard is not coverable.** A successful `InitializeWebViewAsync` requires a live WebView2
  runtime. Mitigation: state the limitation in the delivery evidence rather than manufacturing a test that appears
  to cover it; AC13 is satisfied by the fault arm and the default sink lambda.
- **Coverage-floor divergence between CLAUDE.md and `.claude/rules/general-unit-test.md`.** Mitigation: the
  acceptance criteria avoid asserting a repository-wide percentage; see the note above.

Mitigations and rollbacks:

- The change is additive plus three one-line substitutions. Reverting the commit restores prior behavior exactly.
- No feature flag, no configuration switch, and no migration is required for rollback.

## Rollout & Follow-up

Release/rollout steps:
- Deliver on branch `bug/qfc-initializewebviewasync-fault-is-unobserved-670`, rebased onto current `main` before the
  final toolchain pass.
- Store all delivery-run evidence under
  `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/<kind>/`.
- Open the pull request only after a single clean four-stage toolchain pass and all acceptance criteria are checked
  off with evidence.

Post-fix monitoring or clean-up tasks:
- Promote a new potential entry for the `EfcItemController` fire-and-forget sites
  (`QuickFiler/Controllers/EfcItemController.cs:97` and `:153`) through the promotion lifecycle, recording the
  class-level `[ExcludeFromCodeCoverage]` at `EfcItemController.cs:25` and the absent `IWebViewCoreInitializer` seam
  as preconditions that must be addressed before that fix can be regression-tested.
- Optionally promote a separate potential entry for a `TaskScheduler.UnobservedTaskException` backstop at the add-in
  boundary, if a process-wide safety net is wanted.
- Optionally raise the coverage-floor divergence between CLAUDE.md and `.claude/rules/general-unit-test.md` as its
  own governance item.

Links:
- Issue: https://github.com/drmoisan/TaskMaster/issues/670
- Issue record: `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/issue.md`
- Research: `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/research/initializewebviewasync-fault-observation.2026-08-31T20-30.md`
- Ratified precedent: issue #464, `QuickFiler/Controllers/EfcFormController.cs:127-129`
- Upstream context: issue #488 D5, `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/d5-faulted-task-observation.md`
