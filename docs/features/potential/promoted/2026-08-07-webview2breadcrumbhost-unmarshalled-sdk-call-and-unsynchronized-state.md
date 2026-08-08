# webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state (Issue #476)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state/ (Issue #476)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #476
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/476
- Last Updated: 2026-08-08
## Summary

`WebView2BreadcrumbHost` reaches the WebView2 SDK on the caller's thread with no UI marshalling, and
publishes its initialization state through a non-volatile field read from other threads. The
sibling adapter that serves the same purpose for the popup surface, `WebView2Messenger`, routes
every SDK call through `BreadcrumbUiDispatcher`. The two adapters therefore disagree about the
thread-affinity contract of the same SDK.

Two related defects are filed together because they share one file, one root cause area, and one
fix.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- Package: `Microsoft.Web.WebView2` 1.0.4129.50 (`QuickFiler/packages.config:29`)
- Sole construction site: `QuickFiler/Controllers/EfcFormController.cs:836` (EfcViewer surface)

## Defect 1 — unmarshalled SDK call on the caller's thread

`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:72-84`:

```csharp
public void PostMessageJson(string json)
{
    CoreWebView2? core = _control.CoreWebView2;   // :74  SDK property read
    if (core == null)
    {
        log.Error("PostMessageJson called before CoreWebView2 initialization; payload dropped.");
        return;
    }

    core.PostWebMessageAsJson(json);             // :83  SDK call
}
```

Both the property read at `:74` and the call at `:83` execute on whatever thread invoked
`PostMessageJson`. WebView2 controls are documented as requiring the UI (STA) thread — a
requirement this very file acknowledges at `:105` ("WebView2 controls must be touched on the
WinForms UI (STA) thread") and honours in `InitializeAsync` by awaiting the UI
`SynchronizationContext` before touching the control.

Contrast `QuickFiler/Viewers/WebView2Messenger.cs:55-69`, which wraps the equivalent
`PostWebMessageAsJson` forward inside `_dispatcher.Dispatch(...)`.

`PostMessageJson` is reachable from `BreadcrumbBridgeRouter` / `BreadcrumbOutboundQueue`, which are
not thread-affine, so a non-UI-thread caller is reachable in practice.

`NavigateToString` at `:66-69` has the same shape and the same exposure.

## Defect 2 — unsynchronized cross-thread state publication

`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:54`:

```csharp
public bool IsCoreInitialized { get; private set; }
```

Written at `:134` inside `OnCoreInitializationCompleted`, which runs on the UI thread. Read by
outbound-queue code on other threads. The backing field is a plain non-volatile auto-property
field, so there is no barrier and no guarantee a reader observes the write, nor that it observes the
`core.WebMessageReceived` subscription at `:131-132` that precedes it.

The compare-and-publish ordering at `:131-135` — subscribe, then set the flag, then raise
`CoreInitialized` — is clearly intended to be an ordered publication. Without a barrier that
ordering is not guaranteed to be visible to another thread.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Defect 1 can throw `InvalidCastException`/`COMException` or corrupt COM apartment state when the
breadcrumb document is posted to from a background thread; the symptom is intermittent and
apartment-dependent, which makes it expensive to reproduce from a user report. Defect 2 can cause a
reader to see `IsCoreInitialized == false` after initialization completed (dropping a payload
through the `:76` guard) or to see it `true` before the subscription at `:131-132` is visible.

Severity is High because the failure is silent-or-intermittent and concurrency-dependent rather
than deterministic.

## Suggested Remediation

Give `WebView2BreadcrumbHost` the same `BreadcrumbUiDispatcher` treatment `WebView2Messenger`
already has: route `NavigateToString`, `PostMessageJson`, and the `CoreWebView2` property read
through the captured UI boundary. Make `IsCoreInitialized` a `Volatile.Read`/`Volatile.Write` pair
over an explicit backing field, or publish state through the dispatcher so all access is
single-threaded by construction.

Aligning the two adapters on one thread-affinity contract also removes a standing source of
confusion about which adapter is safe to call from where.

## Why this is not fixed under epic #136

Epic #136 child F13 (issue #455) carries a hard no-behavior-change NFR. Introducing dispatcher
marshalling changes execution ordering and timing on a live UI path, which is a behavior change.

## Related

- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage (where this was found).
- Issue #458 — `WebView2BreadcrumbHost` handler retention across pooled viewer reuse. Same file,
  same lifecycle area; schedule together.
- Issue #136 — parent epic.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Decide whether the two WebView2 adapters should converge on one thread-affinity contract
