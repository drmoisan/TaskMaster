---
name: unobserved-task-fault-670
description: "#670 research: ViewerSetup.cs is 499/500 lines so the named fix target cannot hold the fix; IItemViewer.UiDispatcher is a raw WPF Dispatcher not the IUiDispatcher seam; #464 already ratified the fix shape"
metadata:
  type: project
---

Research 2026-08-31 for issue #670 (`QfcItemController.InitializeWebViewAsync` fault unobserved).
Artifact: `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/research/initializewebviewasync-fault-observation.2026-08-31T20-30.md`

Five findings expensive to re-derive:

1. **`QfcItemController.ViewerSetup.cs` is 499 lines; the ceiling is 500.** The issue AND the spec both name it as
   the fix target. Any fix needing more than one line must go in a NEW partial, which forces a
   `QuickFiler/QuickFiler.csproj` `<Compile Include>` edit (that project has explicit includes, no wildcard;
   partials enumerated at `:331-340`). `Initialization.cs` is 489. `ViewerSetupTests.cs` is 498 — also unusable.
   **Why:** a plan written against the issue text lands in a file with 1 line of headroom.
   **How to apply:** measure the named target file BEFORE accepting any "add a guard here" suggested fix in
   QuickFiler/Controllers.

2. **`IItemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher`** (`QuickFiler/Viewers/IItemViewer.cs:36`),
   NOT the repo's `UtilitiesCS.Threading.IUiDispatcher` seam — the controller holds that separately as
   `_uiDispatcher`. So `_itemViewer.UiDispatcher.InvokeAsync(SomeAsyncMethod)` binds
   `DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task`, yielding
   `DispatcherOperation<Task>` whose `.Task` is `Task<Task>`. Observing the outer observes the DISPATCH only.
   The unwrap expression is `.Task.Unwrap()`, proven in-repo at `UtilitiesCS/Threading/WpfUiDispatcher.cs:61`
   (contrast `:57`, the non-async overload, which does NOT unwrap).
   **How to apply:** never assume a QuickFiler viewer member routes through the injectable dispatcher seam.

3. **Issue #464 already fixed this exact defect class in `EfcFormController` and the shape is ratified.**
   `BoundaryErrorSink` = `internal Action<string, Exception>` property defaulting to `(m, e) => logger.Error(m, e)`
   (`EfcFormController.cs:127-129`), plus an `async Task` member that CONTAINS its fault
   (`InitializeBreadcrumbHostAsync`, `:938-950`), tested by calling the boundary member directly and asserting
   `NotThrowAsync` + sink call count (`EfcFormControllerTests.cs:296-324` — its comment states the premise verbatim).
   **Why:** the #670 issue's option list omits it entirely.
   **How to apply:** for any "discarded Task swallows a fault" defect in QuickFiler, start from #464, not from
   `ContinueWith`.

4. **Every controller logger here is a non-injectable `private static readonly log4net.ILog logger`**
   (`QfcItemController.cs:30`), call form `logger.Error(message, exception)` — message FIRST. The only two ways to
   assert "the fault reached the logger" are a `MemoryAppender` on the type-bound logger
   (`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs:235-252`) or an injectable sink. The
   appender gives NO completion signal, so a fire-and-forget test would have to poll = banned wall-clock wait.
   That forces the sink whenever the fault is asynchronous.

5. **`EfcItemController` is class-level `[ExcludeFromCodeCoverage]`** (`EfcItemController.cs:25`) and has NO
   `IWebViewCoreInitializer` seam — its `InitializeWebViewAsync` (`:174`) calls `CoreWebView2Environment.CreateAsync`
   directly at `:190`. So the "same fix, nearly free" framing for its two `Task.Run(() => InitializeWebViewAsync())`
   sites (`:97`, `:153`) is false: zero covered lines, zero possible regression test. Also `Task.Run(Func<Task>)`
   unwraps, so those two sites need no `.Unwrap()` — a different shape from the Qfc dispatcher site.

Also confirmed: the mocked `IWebViewCoreInitializer` in `QfcItemController.InitializationTests.Part2.cs:243-263`
throws `WebViewSentinelException` from BOTH members, so a controlled fault through `InitializeWebViewAsync` already
exists and a fault-observation test does not need CoreWebView2 to succeed. `Part3.cs:126-127` records that #230
deliberately deferred asserting this exact fault path — #670 is that discharge.

Related: [[qfc-item-controller-defects-484]], [[winforms-pump-seam-230]], [[webview2-host-initializer-defects-476]].
