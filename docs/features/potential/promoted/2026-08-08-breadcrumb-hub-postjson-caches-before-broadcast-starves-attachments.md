# breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments (Issue #501)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments/ (Issue #501)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #501
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/501
- Last Updated: 2026-08-08
## Summary

`BreadcrumbMessengerHub.PostJson` writes the message into its replay cache *before* broadcasting and
wraps the broadcast in no `try`/`catch`. If one attached surface throws — which a disposed
`WebView2Messenger` does — every attachment later in enumeration order silently never receives the
message, yet the cache records it as delivered, so no re-delivery ever occurs and a later `Attach`
replays a state the surviving surfaces never saw.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2)
- Command/flags used: n/a — reached through the QuickFiler ItemViewer breadcrumb surfaces
- Data source or fixture: two or more attached breadcrumb surfaces where one has been disposed

## Steps to Reproduce

1. Attach two breadcrumb surfaces to a single `BreadcrumbMessengerHub` — in production the inline
   selector and the popup surface.
2. Dispose one surface's `WebView2Messenger` without calling the hub's `Detach`. The two are
   independently ordered calls (`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:277`
   and `:288`), so this ordering is reachable.
3. Post a breadcrumb message through the hub.
4. Observe which surfaces received it, and inspect the hub's replay cache.

## Expected Behavior

Either every live attachment receives the message, or a failure to deliver is contained and the cache
does not claim delivery. The hub already demonstrates the correct pattern in `Attach`, which wraps
its replay in `try`/`catch` with an explicit rollback
(`QuickFiler/Viewers/BreadcrumbMessengerHub.cs:82-93`).

## Actual Behavior

`PostJson` caches at `:130`, then enumerates attachments at `:131-134` with no exception handling. The
disposed surface throws `ObjectDisposedException` from `WebView2Messenger.PostJson`
(`QuickFiler/Viewers/WebView2Messenger.cs:61` via `:130-136`). The exception propagates out of the
hub to the caller, attachments later in enumeration order are starved, and `_cachedStates` is not
rolled back.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `System.ObjectDisposedException` raised from `WebView2Messenger.PostJson`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Rationale: recorded by research as Low-Medium. Raised to Medium here because the corrupted replay
cache makes the resulting stale-surface state persistent rather than transient — no subsequent post
repairs it — and the ordering that triggers it is a normal disposal sequence rather than an exotic
race.

## Suspected Cause / Notes

Verified call chain:

1. `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:130` — `CacheState(type, json)` records the message
   **before** any surface has received it.
2. `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:131-134` — the broadcast `foreach` runs with no
   `try`/`catch` anywhere in `PostJson`.
3. A throw from the first attachment aborts the loop; attachments 2..n never receive the message and
   `_cachedStates` is not rolled back.

Contrast `Attach` at `:82-93`, which wraps its replay and rolls back on failure — the hub is
internally inconsistent about this.

No existing test covers the defect. The two `ThrowOnPost` tests
(`QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs:199-217` and
`QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs:317-322`) both throw during
`Attach`-time replay, which *is* rolled back, never during a multi-surface broadcast.

Related but distinct from **#476** (`webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state`),
which concerns `WebView2BreadcrumbHost` rather than the hub's broadcast contract — cross-link rather
than merge. Interacts with the lock-scope defect filed separately from the same research, because the
throw occurs while the hub's `_sync` is held, which aborts the broadcast under the monitor.

Discovered during preparation research for epic #136 child F12 (issue #495), recorded as LD-2 in
`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/research/2026-08-08T02-10-breadcrumb-messenger-hub.md`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a failing regression test first, per the repository Bugfix Workflow,
      attaching two surfaces where the first throws on post, then asserting the second still received
      the message and the cache does not record an undelivered state.
- [ ] Integration scenario to retest: dispose the popup surface while the inline selector remains
      attached, then drive a breadcrumb render and confirm the selector still updates.
- [ ] Manual verification notes: three candidate fixes — catch per surface and continue; defer the
      cache write until after a successful broadcast; or auto-detach a surface that throws. Each is an
      observable behavior change, which is why this was out of scope for #495 under the epic's
      no-behavior-change NFR. Auto-detach is the most invasive and should be weighed against simply
      containing the throw.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
