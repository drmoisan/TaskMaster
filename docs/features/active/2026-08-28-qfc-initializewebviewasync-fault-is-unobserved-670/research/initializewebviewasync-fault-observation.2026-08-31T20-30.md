# Issue #670 — `InitializeWebViewAsync` fault observation: research

- **Issue:** #670
- **Feature folder:** `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/`
- **Timestamp:** 2026-08-31T20-30
- **Tree state:** branch `bug/qfc-initializewebviewasync-fault-is-unobserved-670`, HEAD `2b85134b42872e405602e6064e02dc9cda6c319b` (stated by the orchestrator as identical to `origin/main`)
- **Mode:** preparation-only. No build, no `msbuild`, no `vstest` was run. All findings are from source reading.
- **Tool limitation:** the `Bash` tool is disabled in this session. `git branch -a` could not be executed. The branch inventory in §7 was derived by reading `.git/packed-refs` and enumerating `.git/refs/heads/**` directly; that method is accurate for branch *existence* but the packed snapshot can be stale for branch *tip values*.

---

## 0. Summary of decisions

| Question | Decision |
| --- | --- |
| Q1 | Adopt option **(b)**, in the exact shape issue #464 already ratified for the sibling `EfcFormController`: one fault-containing `async Task` wrapper member plus one injectable `Action<string, Exception>` sink. Reject (a), (c); accept (d) only as an optional out-of-scope backstop. |
| Q2 | `private static readonly log4net.ILog logger` at `QuickFiler/Controllers/QfcItemController.cs:30`, in scope at all three sites, call form `logger.Error(string message, Exception exception)`. It is **not** injectable, which is why the fix must add a sink seam. |
| Q3 | All three sites are unit-testable. The existing mocked `IWebViewCoreInitializer` already drives a deterministic controlled fault (`WebViewSentinelException`). The recommended shape makes the fault boundary **directly awaitable**, which removes the pump and the dispatcher from the test entirely for two of the three assertions. |
| Q4 | Both exclusion facts confirmed. The recommended fix adds **zero** lines inside an excluded member. It cannot be placed in `QfcItemController.ViewerSetup.cs` (499/500 lines) and needs a new partial file plus a `QuickFiler.csproj` `<Compile Include>` entry. |
| Q5 | **OUT OF SCOPE.** `EfcItemController` is class-level `[ExcludeFromCodeCoverage]` and has no injectable WebView2 seam, so a fix there is both invisible to coverage and unregression-testable. Promote as its own potential entry. |
| Q6 | No active feature folder in this tree claims either production file except #670's own. #511 (landed) claims the two *test* files. Six in-flight branches have no feature folder on `main`, so their file claims cannot be read from this worktree — that is the residual risk. |

---

## 1. Verification of the delegating agent's table

Re-derived against this tree. **The table is correct in every cell.** Line numbers, forms, and the two commented-out lines all match.

Evidence — repository-wide grep for `InitializeWebViewAsync` across `*.cs`:

| File:line | Text | Classification |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:30` | comment | not a call site |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:48` | `internal async Task InitializeWebViewAsync()` | declaration |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:484` | comment | not a call site |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:165` | comment | not a call site |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:192` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` | **call site, discarding** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:193` | `//Task.Run(() => InitializeWebViewAsync());` | commented out |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:200` | comment | not a call site |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:256` | `await InitializeWebViewAsync();` | **call site, observed** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:288` | `_ = InitializeWebViewAsync();` | **call site, discarding** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:324` | `_ = InitializeWebViewAsync();` | **call site, discarding** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:345` | `//    _ = InitializeWebViewAsync();` | commented out |

Enclosing members, verified by reading `QfcItemController.Initialization.cs` end to end:

- `:192` → `public void Initialize(bool async)` (declared `:168`), synchronous, returns `void`.
- `:256` → `public async Task InitializeAsync()` (declared `:202`).
- `:288` → `public async Task InitializeGraphicsAsync()` (declared `:263`).
- `:324` → `public async Task InitializeSequentialAsync()` (declared `:295`).

The "Fire and forget WebView initialization" comment is at `QfcItemController.Initialization.cs:191`, exactly as stated.

### Findings that CONTRADICT or materially extend the issue text

1. **`_itemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher`, not the repo's `IUiDispatcher` seam.** `QuickFiler/Viewers/IItemViewer.cs:36` declares `Dispatcher UiDispatcher { get; }`, with `using System.Windows.Threading;` at `IItemViewer.cs:6`. The concrete implementation is `QuickFiler/Viewers/ItemViewer.cs:65`, backed by the field at `:64`, assigned `Dispatcher.CurrentDispatcher` in the constructor at `ItemViewer.cs:27`. The controller separately holds an `IUiDispatcher _uiDispatcher` seam (defaulted at `QfcItemController.Initialization.cs:383`), but site 192 **does not use it** — it goes through the viewer's raw WPF dispatcher. Any plan that assumes site 192 is already behind the injectable dispatcher seam is wrong.

2. **Site 192 produces a doubly-nested task, and the issue text's parenthetical is right but understated.** See §2.1 — observing the outer object observes the dispatch only.

3. **`QfcItemController.ViewerSetup.cs` is 499 lines.** The repository ceiling is 500 (`.claude/rules/general-code-change.md`, "File Size Limit"; CLAUDE.md §4.1). One line of headroom. The issue text and spec both name `ViewerSetup.cs` as the fix target; **the fix cannot land there.** This is the single largest constraint on the implementation and neither the issue nor the spec records it.

4. **`EfcItemController` is class-level `[ExcludeFromCodeCoverage]`** (`QuickFiler/Controllers/EfcItemController.cs:25`). The issue text invites evaluating the Efc sites for inclusion without noting this. It changes the answer decisively (§6).

5. **The #230 work already anticipated this issue and deferred it explicitly.** `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:126-127` reads: "The discarded task's fault path is deliberately not asserted (research section 9)." #670 is the discharge of that deferral, not a new discovery.

6. **An identical defect class was already fixed in this repository under issue #464**, in the sibling `EfcFormController`, with a ratified design and passing tests. See §3.2. The issue's option list does not mention it.

---

## 2. Q1 — Which observation form is correct at each site

### 2.1 Site 192: the static type of the discarded expression

`_itemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher` (§1 finding 1).

`InitializeWebViewAsync` is `internal async Task InitializeWebViewAsync()` — a method group whose return type is `Task`.

Overload resolution against `Dispatcher.InvokeAsync`:

- `DispatcherOperation InvokeAsync(Action)` — **not applicable.** A method group whose method returns `Task` has no method-group conversion to `Action` (the delegate return type must be identity/reference-compatible; `Task` → `void` is not). This is CS0407.
- `DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult>)` — **applicable**, with `TResult` inferred as `Task`.

Therefore `_itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)` has static type **`DispatcherOperation<Task>`**.

`DispatcherOperation<Task>.Task` is a **`Task<Task>`**. The delegating agent's hypothesis is correct: this is a nested task, and observing only the `DispatcherOperation` (or only its `.Task`) observes **the dispatch**, not the WebView2 work. The inner `Task` — the one that actually carries a WebView2 failure — would remain unobserved.

**Unwrapping is required, and the exact expression is `.Task.Unwrap()`.** This is not inferred; the repository already contains the identical expression for the identical construct:

`UtilitiesCS/Threading/WpfUiDispatcher.cs:60-61`
```csharp
public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) =>
    Dispatcher.InvokeAsync(func).Task.Unwrap();
```
Contrast with the non-async-returning overload at `WpfUiDispatcher.cs:56-57`, which does **not** unwrap:
```csharp
public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
    Dispatcher.InvokeAsync(func).Task;
```
`TaskExtensions.Unwrap(this Task<Task>)` is in `System.Threading.Tasks` and is available on net481.

**Signature constraint confirmed.** `void Initialize(bool async)` is declared on the public interface `IQfcItemController` (`QuickFiler/Interfaces/IQfcItemController.cs:25`). Changing it to `async Task` is a breaking public API change, and there are three production callers that would all need to change: `QuickFiler/Controllers/QfcCollectionController.cs:710`, `:1870`, `:1918`. `await` is therefore not available at site 192 without a breaking change. Confirmed as stated.

### 2.2 Sites 288 and 324: does `await` change observable behaviour?

**Yes, decisively. Do not convert them.** Three independent lines of evidence:

1. **Latency, per item, serially.** `InitializeGraphicsAsync` is awaited inside a serial `foreach` over every item group: `QuickFiler/Controllers/QfcCollectionController.cs:444-447` (a second site at `:539`). Converting `:288` to `await` inserts a full WebView2 environment negotiation plus `EnsureCoreWebView2Async` round trip — an out-of-process Edge runtime handshake — into each loop iteration before the next item is initialized. That is the exact cost the "Fire and forget" comment at `:191` exists to avoid.

2. **`InitializeSequentialAsync` is awaited by a factory that returns the constructed controller.** `QfcItemController.Initialization.cs:485` (`await controller.InitializeSequentialAsync();`) then `:486 return controller;`. Converting `:324` to `await` makes controller construction block on WebView2 core init.

3. **It would break two currently-passing tests.** `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:40`) and `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` (`:83`) both `await` the member and assert normal completion. Under the mocked seam the member would instead throw `WebViewSentinelException`, exactly as `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` (`:245`) asserts for the awaited site. Converting is a behaviour change these tests already pin against.

Option (c) is therefore rejected on evidence, not preference.

### 2.3 Recommended shape: option (b), in the #464 form

**Recommendation: (b) — a shared fault boundary with a single injectable observation policy.**

The repository has already solved this exact defect class once, under issue #464, in the sibling controller. The design is:

- an injectable sink property that defaults to the static logger:
  `QuickFiler/Controllers/EfcFormController.cs:127-129`
  ```csharp
  /// <summary>Fault-boundary sink; an injectable seam over the static logger above.</summary>
  internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
      (message, exception) => logger.Error(message, exception);
  ```
- an `async Task` member that **contains** its fault rather than returning one:
  `QuickFiler/Controllers/EfcFormController.cs:938-950` (`InitializeBreadcrumbHostAsync`), invoked fire-and-forget at `:935` (`_ = InitializeBreadcrumbHostAsync();`).
- tests that call the boundary member **directly** and assert `NotThrowAsync` plus a sink call count:
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:300-324`, whose comment at `:296-297` states the premise verbatim: "Both call sites discard the result, so a fault becomes an unobserved faulted Task."

Adopting the same shape here is the lowest-risk, highest-precedent option, and it is the only option that makes the fix testable without a pump (§4).

**Concrete specification.**

- **File:** a new partial, `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`. It **must** be a new file: `QfcItemController.ViewerSetup.cs` is at 499/500 lines and `QfcItemController.Initialization.cs` at 489/500 (§5.2).
- **Namespace / type:** `QuickFiler.Controllers`, `internal partial class QfcItemController` (matching `QfcItemController.Initialization.cs:23-26`).
- **Accessibility:** `internal`, not `public`. `QfcItemController` is itself `internal` (`QfcItemController.cs:25`), so `public` would be meaningless, and `internal` matches `EfcFormController.BoundaryErrorSink` and every existing seam on this type.
- **Members:**
  - `internal System.Action<string, System.Exception> WebViewInitializationErrorSink { get; set; }` defaulting to `(message, exception) => logger.Error(message, exception)`. (Name it distinctly from `BoundaryErrorSink` to avoid implying a shared contract with `EfcFormController`.)
  - `internal async Task InitializeWebViewGuardedAsync()` — `try { await InitializeWebViewAsync(); } catch (OperationCanceledException) { /* cooperative cancellation is not a fault */ } catch (Exception ex) { WebViewInitializationErrorSink($"WebView2 initialization failed: {ex.Message}", ex); }`.

  The `OperationCanceledException` arm mirrors `EfcFormController.BindBreadcrumbRowsAsync` (`EfcFormController.cs:989-991`) and is load-bearing here because `InitializeWebViewAsync` opens with `Token.ThrowIfCancellationRequested()` (`ViewerSetup.cs:52`) and the token is cancelled on normal QuickFiler teardown.
- **Call-site edits (three lines, all in `QfcItemController.Initialization.cs`, all currently covered):**
  - `:192` → `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);`
  - `:288` → `_ = InitializeWebViewGuardedAsync();`
  - `:324` → `_ = InitializeWebViewGuardedAsync();`
  - `:256` (`await InitializeWebViewAsync();`) is **left unchanged**. It is already observed, and routing it through the guard would swallow the fault the existing test `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` asserts.

**Why `.Unwrap()` is not needed at site 192 under this shape.** After the change, the delegate dispatched at `:192` is `InitializeWebViewGuardedAsync`, an `async Task` method that catches `Exception`. Its returned `Task` therefore cannot transition to Faulted. The outer `DispatcherOperation<Task>` can only fault if the delegate *invocation* itself throws, which an `async` method never does (all exceptions, including ones thrown before the first `await`, are captured into the returned task). If the dispatcher is shut down the operation is *aborted*, which surfaces as cancellation, not a fault. The discarded `DispatcherOperation<Task>` at `:192` therefore carries no observable fault after the change, and no `.Unwrap()` is required.

If a planner nonetheless prefers option (a) at site 192, the **only** correct expression is `_itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync).Task.Unwrap()` fed into a `ContinueWith(..., TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default)` — cite `WpfUiDispatcher.cs:61` and `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs:404-411`. Anything that observes the `DispatcherOperation` without unwrapping is a non-fix.

**Rejected alternatives.**

- **(a) `ContinueWith(..., OnlyOnFaulted)` at each site.** Not wrong — the repository has two precedents: `OutlookFolderTreeService.ObserveFault` (`OutlookFolderTreeService.cs:404-411`) and `AppEvents.ReadinessHookup.cs:46-55`. Rejected because it (i) duplicates the policy at three sites rather than sharing it, (ii) requires the `.Task.Unwrap()` special case at site 192 that the other two sites do not need, so the three sites *look* the same but are not, and (iii) is materially harder to test — a continuation is inherently asynchronous, so every test must wait on a signal instead of awaiting the unit under test.
- **(c) `await` at 288/324.** Rejected on the three-part evidence in §2.2.
- **(d) `TaskScheduler.UnobservedTaskException` at the add-in boundary.** **Acceptable as a backstop, insufficient as the fix**, for three reasons: it fires only when the faulted task is *finalized*, so the diagnostic arrives at an arbitrary later GC with no causal context; it is process-global and would be attributed to `TaskMaster/ThisAddIn.cs`, outside this issue's file scope; and there is no in-repo precedent for it (`UnobservedTaskException` returns zero hits across `*.cs`). It also cannot be regression-tested deterministically, because it depends on finalization. Recommend it be captured as a separate potential entry if wanted at all, not folded into #670.

---

## 3. Q2 — The repository's logging pattern at these sites

**Declaration (verified, quoted):** `QuickFiler/Controllers/QfcItemController.cs:30-32`
```csharp
private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
);
```

- **Name:** `logger` (lowercase — the repo-wide convention for this field).
- **Type:** `log4net.ILog`.
- **How obtained:** a `private static readonly` field initialized from `log4net.LogManager.GetLogger(Type)`. Not DI, not `_globals`, not a constructor parameter.
- **Scope at the three call sites:** in scope at all three. `QfcItemController` is a partial class; `QfcItemController.cs` declares the field and `QfcItemController.Initialization.cs:25` and `QfcItemController.ViewerSetup.cs:26` are partials of the same type. It is already used from `ViewerSetup.cs` at `:230` and `:249`.

**Exact call form used elsewhere in this class:** message first, exception second — `logger.Error(string message, Exception exception)`.

- `QuickFiler/Controllers/QfcItemController.Conversation.cs:70` — `logger.Error($"Error in PopulateConversationAsync: {e.Message}", e);`
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:103` — `logger.Error(e.Message, e);`
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:97` — `logger.Error(e2.Message, e);`
- Sibling controllers use the same form: `EfcFormController.cs:948`, `QfcCollectionController.cs:2334`.

The form `logger.Error(ex, "...")` (exception first) does **not** appear anywhere in this repository; it is a Serilog/NLog idiom and would not compile against `log4net.ILog`.

The same field pattern exists on every controller in the namespace: `EfcDataModel.cs:23`, `EfcHomeController.cs:20`, `EfcFormController.cs:123`, `EfcItemController.cs:156`, `EmailSorter.cs:9`, `QfcCollectionController.cs:24`, `KeyboardHandler.cs:25`, `KbdActions.cs:17`, `FilerQueue.cs:16`, `QfcHomeController.cs:21`, `QfcStreamingDequeueConfidenceGate.cs:44`.

**Design consequence — this is the crux of Q3.** The logger is a **non-injectable private static**. A test cannot supply a mock for it. Two escapes exist in this repository:

- **A `log4net.Appender.MemoryAppender` attached to the type-bound logger.** Precedent in the same test assembly: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs:235-252` (`AttachMemoryAppender` / `DetachMemoryAppender`) and `BreadcrumbBridgeRouterIssue614Tests.cs:338-355`. Also `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:228-241`.
- **An injectable sink property over the static logger.** Precedent: `EfcFormController.BoundaryErrorSink` (`EfcFormController.cs:127-129`), consumed by tests at `EfcFormControllerTests.cs:261`, `:290`, `:310`.

**Recommend the sink, not the appender**, for a reason that is specific to #670 and is elaborated in §4.3: the appender gives no completion signal, so a test would have to poll for the log entry, and polling is a wall-clock wait, which `.claude/rules/general-unit-test.md` bans. The sink gives the test a callback it can complete a `TaskCompletionSource` from — or, better, lets the test skip asynchrony altogether by awaiting the boundary member directly.

A secondary hazard with the appender route is recorded in-repo at `BreadcrumbBridgeRouterIssue614Tests.cs:296`: "log4net binds one logger per TYPE, so the appender is shared with any router test" — i.e. it is cross-test-visible state, which conflicts with the Independence and Determinism principles for a type as widely tested as `QfcItemController` (27 test files under `QuickFiler.Test/Controllers/QfcItemController*`).

---

## 4. Q3 — Testability

### 4.1 The existing seam and what it does

- **Test files:** `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (209 lines, `[TestClass] public partial class QfcItemController_InitializationTests`), `...InitializationTests.Part2.cs` (393 lines, shared fixture), `...InitializationTests.Part3.cs` (398 lines, the pump-hosted tests).
- **Pump seam:** `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (482 lines), `internal sealed class WinFormsPumpHost : IDisposable`. Contract: STA background thread running `Application.Run(ApplicationContext)` (`:326`), a `WindowsFormsSynchronizationContext` installed and captured at `:303-306`, and four `Task`-returning members — `InvokeAsync(Action)` (`:81`), `InvokeAsync<TResult>(Func<TResult>)` (`:111`), `RunAsync(Func<Task>)` (`:140`), `RunAsync<TResult>(Func<Task<TResult>>)` (`:176`) — plus `StopAsync()` (`:214`) and `Dispose()` (`:232`). No synchronous bridge is exposed, deliberately (`:23-24`).
- **Mocked `IWebViewCoreInitializer`:** built by `BuildWebViewInitializerMock()` at `QfcItemController.InitializationTests.Part2.cs:243-263`. **It faults.** Both members are stubbed `.ThrowsAsync(new WebViewSentinelException())`:
  - `CreateEnvironmentAsync(It.IsAny<string>(), It.IsAny<CoreWebView2EnvironmentOptions>())` → throws (`:246-253`)
  - `EnsureCoreWebView2Async(It.IsAny<WebView2>(), It.IsAny<CoreWebView2Environment>())` → throws (`:254-261`)
- **Exception type:** `QfcItemController_InitializationTests.WebViewSentinelException`, `internal sealed class ... : System.Exception` with message `"mocked-webview-seam"`, declared at `Part2.cs:269-273`.
- **Where the fault lands inside the member:** `CreateEnvironmentAsync` is the **first** seam call in `InitializeWebViewAsync` (`ViewerSetup.cs:70-73`), reached after `Token.ThrowIfCancellationRequested()` (`:52`), the cache-folder computation (`:55-58`), the options construction (`:61`), and `await _itemViewer.UiSyncContext` (`:64`). The concrete-cast dereferences at `:75` and `:85` are never reached.

### 4.2 Settling the apparent tension

The comment at `QfcItemController.Initialization.cs:199-201` ("The terminal `await InitializeWebViewAsync()` is not completable in a unit test") and the comment at `ViewerSetup.cs:36-47` ("execution must stop at the seam call (controlled fault)") are **both correct and not in tension**. They say different things:

- *Not completable* — the member cannot run to **successful** completion, because success requires a live CoreWebView2 runtime (an external process, barred by policy).
- *Controlled fault* — the member **is** reachable and does reliably reach a deterministic exception at the mocked seam.

**A fault-observation test does not need success.** It needs a fault that is observed and reported. That fault already exists, is already deterministic, and is already asserted by `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` (`Part3.cs:245-288`), which does `await act.Should().ThrowAsync<WebViewSentinelException>(...)` at `:258-262`. The delegating agent's reading is correct.

### 4.3 Can a test assert "the fault reached the logger"?

**Not through the static logger, deterministically.** As established in §3, `logger` is a non-injectable `private static readonly`. The `MemoryAppender` route can observe the *content* of the log but provides no completion signal, so a test would have to poll — banned by `.claude/rules/general-unit-test.md` ("Banned APIs in test code: `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits").

**This turns Q2 into a hard design constraint on the fix**, exactly as the delegating agent anticipated: the fix must route through an injectable seam to be testable at all. That is the decisive argument for the sink in §2.3, over and above the #464 precedent.

With `WebViewInitializationErrorSink` present, the assertion becomes a plain synchronous counter/capture, identical in shape to `EfcFormControllerTests.cs:261` and `:310`.

### 4.4 Is the site-192 dispatcher path drainable, and what is the deterministic wait?

**Yes, drainable.** The chain is: `ItemViewer` is constructed on the pump thread (`Part2.cs:74`), so its constructor captures that thread's `Dispatcher.CurrentDispatcher` (`ItemViewer.cs:27`). A WPF `Dispatcher` on a thread running a WinForms `Application.Run` loop is serviced by that loop. The repository proves this rather than assuming it: `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:218` — `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread`. The fixture further installs that dispatcher as the process-wide `UiThread.Dispatcher` for the duration of a test (`Part2.cs:128`, `transaction.Install(viewer.UiDispatcher)`), released in `PumpHarness.Restore` (`Part2.cs:313-326`).

**Deterministic wait mechanisms in use, by name (no polling, no sleeping anywhere):**

1. `TaskCompletionSource<T>` created with `TaskCreationOptions.RunContinuationsAsynchronously` and awaited — `WinFormsPumpHost.CreateCompletion<TResult>()` (`:364-365`), returned by every `InvokeAsync`/`RunAsync` member. This is what `await host.InvokeAsync(...)` actually waits on.
2. `ManualResetEventSlim` for host startup readiness — `WinFormsPumpHost.cs:29`, set at `:315`, waited at `:60`.
3. `Task.ContinueWith(..., TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)` to bridge inner-task completion into the TCS — `WinFormsPumpHost.ContinueWithOnCompletion` (`:415-423`).
4. `[Timeout(PumpTimeoutMs)]` with `PumpTimeoutMs = 60000` (`QfcItemController.InitializationTests.cs:38`). Its documented role (`:32-37`) is explicitly *not* a wait: it "only converts a genuine deadlock in production code into a test failure instead of a CI hang."
5. `UiThreadDispatcherFixture.BeginTransactionAsync()` / `UiThreadDispatcherTransaction` (`Part2.cs:53-55`, `:313-326`) — an async gate serializing the process-wide dispatcher swap across test classes (issue #493).

**The best mechanism for #670 is to need none of them for the fault assertion.** Because `InitializeWebViewGuardedAsync` is `internal async Task`, a test can `await` it directly. That is precisely what `EfcFormControllerTests.PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` (`:300-324`) does with its boundary member. No pump, no dispatcher, no TCS, no timeout.

### 4.5 Concrete named test designs

All three live in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (398 lines, ~102 lines of headroom before the 500 ceiling), which already carries a `<Compile Include>` entry. **No new test file, and therefore no `QuickFiler.Test.csproj` edit, is required** — the project has 151 explicit `<Compile Include>` entries and no wildcard, so a new file would require a csproj edit that this scope should avoid.

**Test 1 (the core assertion — no pump needed).**
`InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault`
- **Arrange:** `HarnessController` (`QfcItemController.TestSupport.cs:28`); reflection-inject `_webViewInitializer` with `BuildWebViewInitializerMock().Object` and `_uiDispatcher` with `QfcItemControllerTestSupport.BuildSyncDispatcher().Object` via `QfcItemControllerTestSupport.SetField` (`TestSupport.cs:40`); inject an `IItemViewer` mock whose `UiSyncContext` returns a plain `SynchronizationContext` so the `await _itemViewer.UiSyncContext` at `ViewerSetup.cs:64` completes; capture the sink: `Exception captured = null; controller.WebViewInitializationErrorSink = (m, e) => captured = e;`
- **Act:** `Func<Task> act = () => controller.InitializeWebViewGuardedAsync();`
- **Assert:** `await act.Should().NotThrowAsync(...)`; `captured.Should().BeOfType<WebViewSentinelException>(...)`.
- **Notes:** fully synchronous in effect, no `WinFormsPumpHost`, no `[Timeout]`. This is the test that actually proves the defect is fixed.

**Test 2 (default sink coverage).**
`WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing`
- Mirrors `EfcFormControllerTests.BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` (`:283-294`) so the default lambda body is covered rather than always replaced.
- **Arrange:** `new HarnessController()`. **Act:** `Action act = () => controller.WebViewInitializationErrorSink("smoke", new InvalidOperationException());` **Assert:** `act.Should().NotThrow(...)`.

**Test 3 (site 192 — the dispatcher path, pump-hosted).**
`InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink`
- **Arrange:** `WinFormsPumpHost host = new WinFormsPumpHost();` `harness = await BuildPumpHarnessAsync(host, darkMode: false);` then, before Act, install a signalling sink on `harness.Controller`:
  `var observed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);`
  `harness.Controller.WebViewInitializationErrorSink = (m, e) => observed.TrySetResult(e);`
- **Act:** `await host.InvokeAsync(() => harness.Controller.Initialize(async: false));` then `Exception fault = await observed.Task;`
- **Assert:** `fault.Should().BeOfType<WebViewSentinelException>(because: "the dispatched fault must reach the sink, not be discarded")`.
- **Determinism:** the only wait is `await observed.Task` — a `TaskCompletionSource` completion, mechanism (1) above, guarded by `[Timeout(PumpTimeoutMs)]`. No polling, no sleeping. Teardown follows the existing `finally { harness?.Restore(); await host.StopAsync(); }` shape (`Part3.cs:155-163`).
- **Caveat a planner must honour:** the sink must be installed *before* `Initialize` is invoked, because the dispatched operation may complete before `host.InvokeAsync` returns.
- **Caveat b:** do **not** also add a pump-hosted variant for sites 288 and 324. Test 1 already covers the boundary member, and `InitializeGraphicsAsync`/`InitializeSequentialAsync` are already covered by `Part3.cs:83` and `:40`. Adding two more 60-second-timeout pump tests for a call-expression change is disproportionate; §4.6 records the substitute evidence.

**Nothing is quietly dropped.** For sites 288 and 324 the change is a call-expression substitution (`InitializeWebViewAsync()` → `InitializeWebViewGuardedAsync()`) on a line that is already executed by an existing passing test (`Part3.cs:83` and `:40` respectively). The behavioural content of the fix at those sites is entirely inside `InitializeWebViewGuardedAsync`, which Test 1 covers directly. Substitute evidence, if a reviewer wants it beyond the existing tests: a metadata/source assertion that the three sites name the guarded member — the repository has precedent for exactly this kind of structural pin at `QuickFiler.Test/Controllers/EfcItemControllerTests.cs:95-107` and `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:248-260`.

---

## 5. Q4 — Coverage consequences

### 5.1 The two exclusion facts — both confirmed

- `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:47`, applying to `internal async Task InitializeWebViewAsync()` at `:48`. **Confirmed**, with the 12-line rationale comment at `:36-46`.
- `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:137`, applying to `internal void EnsureBreadcrumbPipeline()` at `:138`. **Confirmed.**
- The three call sites in `QfcItemController.Initialization.cs` (`:192`, `:288`, `:324`) are **not** excluded — their enclosing members `Initialize(bool)` (`:168`), `InitializeGraphicsAsync` (`:263`) and `InitializeSequentialAsync` (`:295`) each carry an explicit "#230: de-exempted" comment (`:164-167`, `:259-262`, `:291-294`) and no attribute. **Confirmed.**
- `coverage.config` at repo root contains only third-party `ModulePath` excludes (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). There is **no** assembly-level exclusion of `QuickFiler`. Exemption in this area is attribute-only and PR-reviewable.

### 5.2 Where the recommended fix lands

**Zero new lines inside an excluded member.** The recommended change is:

- three single-line call-expression edits at `Initialization.cs:192`, `:288`, `:324` — all inside non-excluded members, all on lines already executed by existing tests;
- a new partial file containing the sink property and the guard method — **not** excluded, therefore in the coverage denominator, and directly testable by Test 1 and Test 2 of §4.5;
- **no edit to `InitializeWebViewAsync` itself**, so the excluded member is untouched.

This deliberately avoids the trap recorded for issue #485, where an in-place guard inside the excluded `InitializeWebViewAsync` would have added zero covered lines and zero testable regression surface; the resolution there was extraction into a testable member (`TryResolveCidResource`, `ViewerSetup.cs:215`). The same principle applies here.

**File-size constraint (blocking, and not recorded in the issue or spec).** Measured line counts on this tree:

| File | Lines | Headroom to 500 |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | **1** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 489 | 11 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 398 | 102 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 393 | 107 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 440 | 60 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 498 | **2** |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 436 | 64 |

Consequences:
- The sink and guard **cannot** go in `ViewerSetup.cs`. A new partial file is mandatory.
- The three call-site edits fit in `Initialization.cs` (they are substitutions, net zero lines, plus at most a short `#670` comment within the 11-line headroom).
- New tests go in `Part3.cs`. Do **not** put them in `ViewerSetupTests.cs` (2 lines of headroom).

**csproj consequence.** `QuickFiler/QuickFiler.csproj` uses explicit `<Compile Include>` entries with no wildcard — the `QfcItemController` partials are enumerated at `:331-340`. The new production partial therefore **requires one added `<Compile Include>` line** in `QuickFiler/QuickFiler.csproj`, adjacent to `:333`. A planner must budget for this; it is a `.csproj` edit and CSharpier is kept off `.csproj` by `.csharpierignore`, so it will not be reformatted.

### 5.3 New-module coverage target and the two floors

The new partial file is a new module for policy purposes and must reach **>= 90%** per CLAUDE.md ("Any new modules, classes, or methods added must target `>= 90%` coverage"). With Test 1 covering the guard's fault arm and Test 2 covering the default sink lambda, the only member left is the guard's happy path (`await InitializeWebViewAsync()` returning normally), which is unreachable under the mocked seam. A planner should expect to cover it by injecting a mock `IWebViewCoreInitializer` whose members return completed tasks — noting that the member would then proceed to `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` at `ViewerSetup.cs:85`, which is null without the real runtime, producing a `NullReferenceException` that the guard would itself catch. That still exercises the guard, but it does not exercise a *successful* `InitializeWebViewAsync`. This should be stated plainly in the plan rather than promised away.

**Coverage-floor divergence (flagged, not resolved).** Two authorities in this repository state different numbers:
- `CLAUDE.md` (General Unit Test Policy §UT2): "Repository-wide line coverage must remain `>= 80%`", with the ratified COM/VSTO/WinForms testable-denominator exemption, and `>= 90%` for new modules.
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`: "Line coverage must remain >= 85% across all tiers (T1–T4)" and ">= 75%" branch, with a Coverage Exclusion Policy that forbids excluding production files at all — which is in direct tension with the `[ExcludeFromCodeCoverage]` attributes this very issue depends on.

**Which a reviewer will apply is not determinable from the documents alone.** CLAUDE.md's own "Policy Compliance Order" section places CLAUDE.md first (item 1) and the general unit-test policy second (item 3), which reads as CLAUDE.md governing on conflict; but the `.claude/rules/*.md` files are loaded as project instructions in their own right. Note also that the `.claude/**` tree is push-down-owned from an upstream repository and is overwritten without templating, so the divergence is very likely imported rather than intentional. Do not attempt to resolve it inside #670. Record the applied floor explicitly in the plan and cite which document it came from.

---

## 6. Q5 — Scope boundary: `EfcItemController`

**Recommendation: OUT OF SCOPE. Promote as its own potential entry.**

Facts, verified:

- `QuickFiler/Controllers/EfcItemController.cs:97` and `:153` are both `Task.Run(() => InitializeWebViewAsync());`, as expression statements with no `_ =` and no observation. Enclosing members: an `InitializeDataFields`-style initializer ending at `:98-99 return this;`, and `private void Initialize(bool async)` (`:101`).
- The target is that class's own `internal async Task InitializeWebViewAsync()` at `EfcItemController.cs:174`.
- **The observation shape is different from the Qfc sites.** `Task.Run(() => InitializeWebViewAsync())` binds the `Task.Run(Func<Task>)` overload (C# overload resolution prefers the `Func<Task>` target over `Action` when the lambda body is an expression whose type is `Task`), and that overload *unwraps*. So the single discarded `Task` here already incorporates the inner fault, and one continuation would observe everything — no `.Unwrap()` needed. Note: this is a determination from C# overload-resolution rules, not something I executed.

Decisive reasons for exclusion:

1. **`EfcItemController` is class-level `[ExcludeFromCodeCoverage]`** (`EfcItemController.cs:25`). Any code added there is invisible to coverage by construction. Bundling it would add production surface with zero measurable benefit and zero regression test.
2. **There is no injectable WebView2 seam in `EfcItemController`.** Its `InitializeWebViewAsync` calls `CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` **directly** at `:190-194` and `_itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(...)` at `:203`. There is no `IWebViewCoreInitializer` field on this class. The controlled-fault technique that makes the Qfc fix testable (§4.1) **does not exist here**. Making it testable would require porting the whole #230 seam to a second controller — a substantially larger piece of work than #670.
3. **Blast radius.** The item is being admitted mid-run into an in-progress parallel cohort. Including `EfcItemController.cs` (1117 lines) widens the production-file set from 2 to 3 and pulls in a file that sits next to several in-flight EFC items (§7). The marginal cost is not free even under option (b): the shared helper would have to be reachable from `EfcItemController`, which is a different type in the same namespace, so it would need to be lifted to a static utility rather than a private partial member — a strictly larger design.
4. **The issue text itself frames it as open** ("whether that belongs in the same fix is worth evaluating", `issue.md:90`), so excluding it is consistent with the issue rather than a narrowing of it.

**Does option (b) make including it "nearly free"?** No. The sink/guard shape recommended in §2.3 is an *instance* fault boundary on `QfcItemController`. Reusing it across two unrelated controller types would require promoting it to a shared static helper (for example in `UtilitiesCS`), which changes the design, widens the diff to a third project, and still leaves the Efc sites untestable because of reasons 1 and 2. The near-free framing does not survive contact with the coverage attribute.

**Recommended follow-up:** promote a potential entry, e.g. `efc-item-controller-initializewebviewasync-fault-is-unobserved`, noting the two call sites, the class-level coverage attribute, and the missing seam as its own precondition. Per repository practice, out-of-scope defects must be routed through the promotion lifecycle into a real issue rather than left as prose in a feature folder.

---

## 7. Q6 — Ownership and collision risk

### 7.1 Feature folders under `docs/features/active/`

29 directories exist. Of the numbers the orchestrator named, the following **have** an active feature folder in this tree:

| Number | Directory |
| --- | --- |
| 469 | `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/` |
| 484 | `docs/features/active/qfc-item-controller-defects-484/` |
| 511 | `docs/features/active/winformspumphost-suite-determinism-511/` |
| 638 | `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/` |
| 644 | `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/` |
| 647 | `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/` |

The following have **no** folder in this tree: **629, 633, 646, 648, 656, 662, 663.**

### 7.2 Grep of every active folder's `spec.md` / `issue.md` / `plan*.md` for the three file names

Searching for `QfcItemController.Initialization`, `QfcItemController.ViewerSetup`, and `EfcItemController` across `docs/features/active/`:

| Folder | Match on `QfcItemController.Initialization.cs` / `.ViewerSetup.cs` / `EfcItemController.cs`? |
| --- | --- |
| `winformspumphost-suite-determinism-511` | **Yes, but test files only.** `spec.md:276-278` names the three changed files, all under `QuickFiler.Test/`. `remediation-plan.2026-08-23T20-57.md:34` states the constraint verbatim: "Do not edit anything under `QuickFiler/`, any `*.csproj`". `spec.md:643-644` references the two production files only as *comment-audit* targets. |
| `qfc-item-controller-defects-484` | 97 occurrences of the type name in `spec.md`, 20 in `issue.md`, but **no** match for either production file *path* pattern. Status `Approved`, Last Updated `2026-08-24`. Its five defects (#480/#481/#483/#484/#485) are visible as landed comments in the current tree (`ViewerSetup.cs:33`, `:90`, `:215`; `Initialization.cs` unaffected; `Cleanup()` at `ViewerSetup.cs:477-481`). |
| `2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644` | **No.** Explicit grep for the two production file paths in its folder returned no matches. |
| `2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638` | 1 type-name mention in `spec.md`; **no** file-path match. |
| `2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647` | **No** match at all. |
| `2026-08-07-qfc-collection-move-diagnostics-defects-469` | **No** file-path match. |
| `2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` | Yes — its own. |

**Overlap on `QuickFiler/Controllers/QfcItemController.Initialization.cs` or `QfcItemController.ViewerSetup.cs`: none, from any active feature folder other than #670's own.**

**Overlap on `QuickFiler/Controllers/EfcItemController.cs`: none** from a plan or spec; `efc-controller-surface-defects-464` (97 type-name occurrences) and `webview2-host-initializer-defects-476` (30) reference the *type*, and their landed work is visible in the current tree, but neither claims the file for in-flight work.

**Test-file overlap — the real risk, and it is historical.** #511's owned set is exactly `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, `...Part3.cs`, and `...ViewerSetupTests.cs` — the same files §4.5 recommends for #670's tests. #511's work appears to be **already landed on this tree**: `Part2.cs:84` carries the `viewer.Handle` read with the seven-line replacement comment its remediation plan specified, and `Part3.cs:301` and `:356` contain the two probe tests (`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`, `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`) its spec required. The collision is therefore historical, not in-flight.

### 7.3 Branch inventory

Derived from `.git/packed-refs` plus loose refs under `.git/refs/heads/` (the `Bash` tool is disabled; `git branch -a` was not run). **Local `bug/*` branches:**

`bug/breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656`, `bug/breadcrumb-left-right-arrow-parent-child-navigation-440`, `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`, `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638`, `bug/efcselectionguard-banner-prefix-arity-and-stale-comment-662`, `bug/fileio2-write-retry-reports-success-on-final-failure-647`, `bug/issue-468-residual-reflective-caller-risk-635`, `bug/qfc-collection-move-diagnostics-defects-469`, `bug/qfc-initializewebviewasync-fault-is-unobserved-670`, `bug/qfc-metrics-flush-writes-empty-session-file-646`, `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663`, `bug/qfc-unregister-navigation-count-mismatch-orphan-644`, `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`, `bug/quickfiler-keyboard-hook-leaks-to-outlook-677`, `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680`, `bug/winformspumphost-suite-determinism-511`, `bug/wpfuidispatchertests-ungated-static-swap-648`.

**Local `feature/*` branches:** `feature/qfc-remove-stackmoveditems-parameter-629` (tip equals the packed `main` value, i.e. no divergent work in the packed snapshot), plus 17 `feature/quickfiler-*-coverage*` branches belonging to the older per-file-coverage epic, and `feature/ci-parallel-job-split-553` (remote only).

**Remotes:** `origin/` counterparts exist for 440, 637, 638, 647, 635, 469, 644, 677, 680, 511. Two show divergent tips between local and origin in the packed snapshot (469: local `30d2aeb` vs origin `31dcb06`; 680: local `4219867` vs origin `78a5197`); a packed snapshot can be stale, so treat that as unconfirmed.

**No branch exists for 484 or 468** — consistent with those features having landed.

### 7.4 Residual risk statement

There are **six in-flight branches whose feature folders are absent from this worktree** (633, 646, 648, 656, 662, 663), because this worktree is at `main` and their folders have not merged. Their file claims cannot be read from here. From branch names alone none targets `QfcItemController.Initialization.cs` or `QfcItemController.ViewerSetup.cs`. The two worth watching:

- **648 `wpfuidispatchertests-ungated-static-swap`** — its subject is `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs`. §2.1 of this document *cites* `UtilitiesCS/Threading/WpfUiDispatcher.cs` but recommends no change to it; the risk is a stale citation, not a conflict.
- **663 `qfc-twin-processcmdkey-alt-chord-over-claim`** — likely touches `ItemViewer` / keyboard handling. `QuickFiler/Viewers/ItemViewer.cs` is *read* by this research (`:27`, `:65`) but is not in #670's change set.

**Recommended admission condition:** before the delivery run's Phase 0 commits anything, re-verify against the then-current `main` that no merged item has taken `QfcItemController.Initialization.cs` past 489 lines or `ViewerSetup.cs` past 499, and that `Part3.cs` still has headroom. The 1-line headroom on `ViewerSetup.cs` means any concurrent merge into that file could invalidate a plan written today.

---

## 8. Numeric Derivation Evidence

Required for the call-site count, which a planner will most likely lift into an acceptance criterion.

**Claim under derivation:** `QfcItemController.InitializeWebViewAsync` has exactly **4** production call sites, of which exactly **3** discard the returned task and **1** observes it.

- **Complete Family:** every syntactic invocation expression in first-party production source that invokes the member `QuickFiler.Controllers.QfcItemController.InitializeWebViewAsync()`. Because the member has exactly one signature (no overloads — parameterless, `internal async Task`, declared once at `QfcItemController.ViewerSetup.cs:48`), the family is complete when every textual occurrence of the identifier in production source is enumerated and classified. Method-group references (as at `:192`) count as call sites because the dispatcher invokes them; commented-out lines, prose comments, declarations, and test-file occurrences do not.
- **Exhaustive Search Scope:** all `*.cs` files in the repository working tree, then partitioned into production (`QuickFiler/`) and non-production (`*.Test/`). Both searches below cover the full identifier, not a narrower call-shape pattern, so no invocation form (`await X()`, `_ = X()`, `X()`, method group `X`, `Task.Run(() => X())`, reflection by name) can escape.
- **Inclusion Rules:** occurrence is in `QuickFiler/` (production); occurrence is on the `QuickFiler.Controllers.QfcItemController` type; occurrence is executable code, not a comment; occurrence invokes or passes the member as a delegate.
- **Exclusion Rules:** the declaration line itself; lines whose first non-whitespace token is `//`; occurrences under any `*.Test/` project; occurrences resolving to `QuickFiler.Controllers.EfcItemController.InitializeWebViewAsync`, which is a **different member on a different type** with the same name (this is the disambiguation that a naive count would get wrong).

**Primary record.**
- *Strategy / expression:* directory-scoped identifier grep — `Grep(pattern="InitializeWebViewAsync", path="QuickFiler", output_mode="content", -n=true)`. Scope restricted to the production project; every hit then classified by reading its line.
- *Member Set (production, `QfcItemController` type, executable, non-declaration):*
  1. `QuickFiler/Controllers/QfcItemController.Initialization.cs:192` — `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` — discarding
  2. `QuickFiler/Controllers/QfcItemController.Initialization.cs:256` — `await InitializeWebViewAsync();` — observed
  3. `QuickFiler/Controllers/QfcItemController.Initialization.cs:288` — `_ = InitializeWebViewAsync();` — discarding
  4. `QuickFiler/Controllers/QfcItemController.Initialization.cs:324` — `_ = InitializeWebViewAsync();` — discarding
- *Excluded by rule, with reason:* `ViewerSetup.cs:48` (declaration); `ViewerSetup.cs:30`, `:484`, `Initialization.cs:165`, `:200` (prose comments); `Initialization.cs:193`, `:345` (commented-out code); `EfcItemController.cs:97`, `:153`, `:174` (different type).
- *Primary Count:* **4 call sites; 3 discarding, 1 observed.**

**Cross-check record.**
- *Strategy / expression:* a deliberately **broader, differently-anchored** repository-wide prefix grep — `Grep(pattern="InitializeWebView", glob="*.cs", output_mode="content", -n=true)` — across the entire tree with no path restriction. This is a distinct query: it uses a shorter prefix (so it also catches any `InitializeWebView`, `InitializeWebViewCore`, etc.), it is not directory-scoped, and it includes test projects, so it would surface any production caller outside `QuickFiler/`, any reflection-by-name invocation, and any similarly named sibling member the primary scope would have missed. Independently, both production partials (`QfcItemController.Initialization.cs`, 489 lines, and `QfcItemController.ViewerSetup.cs`, 499 lines) were read end to end in full and every occurrence enumerated by eye.
- *Member Set (after applying the same inclusion/exclusion rules):* identical four entries —
  1. `QfcItemController.Initialization.cs:192` (discarding)
  2. `QfcItemController.Initialization.cs:256` (observed)
  3. `QfcItemController.Initialization.cs:288` (discarding)
  4. `QfcItemController.Initialization.cs:324` (discarding)
- *Additional hits surfaced only by the broader query, all excluded:* `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:367`, `...InitializationTests.Part3.cs:35`, `:122`, `:238`, `...InitializationTests.Part2.cs:239` (XML-doc prose in tests); `QuickFiler.Test/Controllers/EfcItemControllerTests.cs:90`, `:95`, `:99`, `:107` (a metadata-absence assertion about `InitializeWebView`, a *different, removed* member — precisely the kind of near-miss the shorter prefix was chosen to expose).
- *Cross-check Count:* **4 call sites; 3 discarding, 1 observed.**

**Member-set Comparison.** Normalizing both member sets to `{file:line}` pairs:
- Primary: `{Initialization.cs:192, Initialization.cs:256, Initialization.cs:288, Initialization.cs:324}`
- Cross-check: `{Initialization.cs:192, Initialization.cs:256, Initialization.cs:288, Initialization.cs:324}`

The sets are **identical** — same cardinality, same members, same discarding/observed classification per member. The cross-check additionally established that there are **zero** call sites outside `QfcItemController.Initialization.cs`, **zero** reflection-based invocations of the member anywhere in the repository, and that the only same-named member (`EfcItemController.InitializeWebViewAsync`, `EfcItemController.cs:174`) is on an unrelated type and is correctly excluded. The counts agree and the assertion is safe to propose.

---

## 9. Toolchain and policy notes for the delivery run

- Order is fixed: `dotnet tool run csharpier format .` → `dotnet tool run csharpier check .` → `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` → `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` → `vstest.console.exe <assemblies> /EnableCodeCoverage`. Do **not** add `/p:Nullable=enable`; do **not** use `/t:Build`. None of these were run in this preparation pass.
- Tests are MSTest + Moq + FluentAssertions. The recommended designs in §4.5 use only those.
- **Nullable:** neither `QfcItemController.Initialization.cs` nor `QfcItemController.ViewerSetup.cs` carries `#nullable enable` (line 1 of each is `using System;`). The repository is per-file opt-in with no `Directory.Build.props`. The new partial file should **not** add the directive — doing so would conscript it into the `TreatWarningsAsErrors` gate for no benefit, and would be inconsistent with its sibling partials.
- **Bugfix workflow applies:** a failing regression test first. Test 1 of §4.5 is the RED test — it does not compile before the fix (the guard member does not exist), so the plan should sequence it as "author the boundary member and the sink, then the test asserting the sink is called, and verify the test fails when the sink call is removed" rather than a literal compile-failing red step. Record that reasoning explicitly; a reviewer will look for it.
- **Evidence:** all delivery-run artifacts go under `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/<kind>/`. No evidence directory was created by this preparation pass.
