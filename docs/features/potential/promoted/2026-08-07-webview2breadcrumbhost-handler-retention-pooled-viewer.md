# webview2breadcrumbhost-handler-retention-pooled-viewer (Issue #458)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/webview2breadcrumbhost-handler-retention-pooled-viewer/ (Issue #458)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #458
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/458
- Last Updated: 2026-08-08
## Summary

`WebView2BreadcrumbHost`'s constructor performs an unhook-then-hook sequence intended to be
idempotent across pooled-viewer reuse, but the unhook cannot remove the previous instance's
subscription. When a new `WebView2BreadcrumbHost` is constructed over a `WebView2` control that a
prior host instance already wrapped, the prior instance stays subscribed to the control's
`CoreWebView2InitializationCompleted` event. The old host is kept alive by the control, and
initialization completion is handled more than once.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- Affected path: pooled viewer reuse (`EfcViewerQueue` / `ItemViewerQueue` recycling a viewer whose
  Designer-owned `WebView2` control is retained across uses)

## Steps to Reproduce

1. Construct a `WebView2BreadcrumbHost` over a Designer-owned `WebView2` control.
2. Recycle the pooled viewer so a second `WebView2BreadcrumbHost` is constructed over the **same**
   control instance.
3. Drive CoreWebView2 initialization to completion.
4. Observe that both host instances handle the completion: `IsCoreInitialized` is set and
   `CoreInitialized` is raised on the stale instance as well as the live one.

## Expected Behavior

Exactly one host instance — the current one — is subscribed to the control's events, and exactly
one `CoreInitialized` notification reaches the controller per initialization.

## Actual Behavior

Both instances remain subscribed. The stale instance is retained for the lifetime of the control.

## Suspected Cause

`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:48-50`:

```csharp
// Idempotent hookup: pooled viewers re-run initialization, so unhook before hooking.
_control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;
_control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;
```

`OnCoreInitializationCompleted` is an **instance** method. The delegate formed by `-=` in a
constructor is bound to the instance under construction, which has never subscribed. Delegate
equality is `(target, method)` pairwise, so the removal matches nothing and is a no-op. The
comment's stated intent — de-duplicating across pooled-viewer re-initialization — is therefore not
achieved. The pattern is only idempotent for repeated calls on the *same* instance.

The same instance-bound unhook-then-hook pattern appears at
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:131-132` for `core.WebMessageReceived`. That one is
genuinely idempotent within a single instance, but has the same cross-instance leak when two hosts
share one `CoreWebView2`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Consequences are a managed-object retention leak (each recycled viewer accumulates one dead host
plus its event-handler chain) and duplicated `CoreInitialized` / `MessageReceived` notifications
routed to `BreadcrumbBridgeRouter.NotifyCoreInitialized`. Duplicate notification can produce
duplicate breadcrumb document initialization work. Severity is Medium rather than High because the
stale host's handler does not throw and the observable breadcrumb output is idempotent in the
common path.

## Suggested Remediation

Give the host an explicit `IDisposable` (or `Detach()`) that unsubscribes the live instance's
handlers, and call it when the pooled viewer releases the host — rather than relying on a
constructor-side unhook that cannot see the previous instance. Alternatively, store the
subscription on the control (for example in a single owner field) so the new host can unsubscribe
its predecessor explicitly.

## Why this is not fixed under epic #136

Epic #136 child F13 (issue #455) carries a hard no-behavior-change NFR: it raises coverage and
removes unratified coverage exemptions without altering observable QuickFiler flows. Fixing this
changes subscription lifetime and notification counts, which is a behavior change and belongs in
its own issue.

## Related

- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage (where this was found).
- Issue #136 — parent epic.
- Issue #349 — the change that introduced the WebView2 breadcrumb control.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Reconcile against F13's plan before scheduling, since F13 adds tests over this file
