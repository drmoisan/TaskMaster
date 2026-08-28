---
name: webview2-host-initializer-defects-476
description: "#476/#458/#477 research findings: EfcViewerQueue is NOT a recycle pool so #458's premise is latent; real WebView2 controls ARE constructible in QuickFiler unit tests; BreadcrumbUiDispatcher captures nothing from the control"
metadata:
  type: project
---

Research completed 2026-08-24 for feature `webview2-host-initializer-defects` (issue #476, closing
#458, #476, #477). Artifact:
`docs/features/active/webview2-host-initializer-defects-476/research/2026-08-24T00-45-webview2-host-initializer-defects-research.md`

Four findings that contradict or go beyond the issue text and are expensive to re-derive:

1. **`EfcViewerQueue` is a pre-warm pool of FRESH instances, not a recycle pool.** `ViewerQueueCore`
   has no return-to-pool method; every enqueue calls `_viewerFactory()`. Issue #458's "pooled viewer
   reuse" premise therefore does not occur in production, and the class XML doc that cites
   `EfcViewerQueue` is wrong. The defect is real at the type level but LATENT. A regression test must
   be unit-level (two hosts, one control), not a production repro.
   **Why:** an executor can burn a cycle trying to reproduce a recycle that does not exist.
   **How to apply:** check `ViewerQueueCore` before accepting any "pooled viewer" claim about
   QuickFiler viewers.

2. **`EfcFormController.Cleanup()` does not touch `_breadcrumbHost` or `_router`, and the controller
   implements no `IDisposable`.** No disposal path anywhere reaches the breadcrumb host, so an
   `IDisposable`/`Detach()` remedy has zero possible callers without editing `EfcFormController.cs`.
   **Why:** the "obvious" fix for a handler-retention leak is dead on arrival here.
   **How to apply:** for any QuickFiler lifetime fix, verify a caller exists before designing a
   Dispose-based remedy.

3. **Real `Microsoft.Web.WebView2.WinForms.WebView2` controls ARE already constructed in
   QuickFiler unit tests** — transitively via `new ItemViewer()` on `WinFormsPumpHost`, and a test
   asserts both children report `IsHandleCreated == true`. Constructing the CONTROL needs no
   Evergreen runtime; only `EnsureCoreWebView2Async` / `CoreWebView2Environment.CreateAsync` do.
   `QuickFiler.Test` references both WebView2 assemblies directly.
   **Why:** the standing assumption that "WebView2 cannot exist in a unit-test host" is false and has
   been used to justify coverage exemptions.
   **How to apply:** when auditing a WebView2 coverage exemption, the barrier is core INIT, not
   control construction.

4. **`WebView2Messenger.CaptureProductionDispatcher(coreWebView)` captures NOTHING from the control.**
   It null-guards the argument then returns `BreadcrumbUiDispatcher.CaptureCurrent()`, which captures
   the ambient `SynchronizationContext` and thread id. `CaptureCurrent()` THROWS when
   `SynchronizationContext.Current` is null, so copying that precedent into a constructor adds a new
   throwing precondition.
   **Why:** the factory's control parameter reads as if it were the capture source; it is only an
   argument-order device to preserve the `ArgumentNullException` contract.
   **How to apply:** prefer capturing from an explicitly supplied `SynchronizationContext` (e.g. the
   one `InitializeAsync` already receives) over `CaptureCurrent()` at construction.

Also recorded: `QuickFiler/Viewers/WebView2CoreInitializer.cs` and `IWebViewCoreInitializer.cs` are
NOT `#nullable enable` (per-file opt-in; no `Directory.Build.props`), so adding the directive
conscripts them into the `TreatWarningsAsErrors` gate. `WebView2BreadcrumbHost.cs` IS nullable-enabled.

Related: [[qfc-breadcrumb-webview2-351]], [[efcviewer-breadcrumb-webview2-349]],
[[winforms-pump-seam-230]], [[qfc-item-controller-227-r2-denial]].
