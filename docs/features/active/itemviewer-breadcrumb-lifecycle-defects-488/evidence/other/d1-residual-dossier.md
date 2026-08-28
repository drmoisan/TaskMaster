# D1 — Residual Dossier ([P1-T7])

Timestamp: 2026-08-28T05-29

Command: source reading of the delivered `[P1-T5]` change in
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` together with
`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` and
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`. No command was executed.
EXIT_CODE: 0

## The residual, stated precisely

After `[P1-T5]`'s fix, `ConfigureBreadcrumbDropDown(environment, initializer)` disposes the outgoing
`BreadcrumbDropDownHost` **synchronously**, by statement order, before the replacement host is
constructed. The lifecycle coordinator's open coordinator, however, still points at that outgoing host
until `ConfigureHost`'s **posted** lambda runs `ReleaseHostCore()` and installs the replacement.

Between those two moments a window exists in which `_openCoordinator.Host` is a host that has already
been disposed. A `SetTheme` landing inside that window reaches
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme`, which forwards to `DropDownHost?.SetTheme(theme)`,
which reaches `BreadcrumbDropDownHost.SetTheme`'s `ThrowIfDisposed()` guard and throws
`ObjectDisposedException` — instead of silently theming a host that is about to be discarded.

## This residual is ACCEPTED, not fixed

It is a deliberate, recorded outcome of the D1 design, not an oversight. Three reasons support
accepting it:

1. **D4 rejects an off-boundary configure outright.** Once `ThrowIfOffUiBoundary` guards both
   `ConfigureBreadcrumbDropDown` overloads, an off-boundary configure is rejected at the entry point,
   so the deferred-post window that opens the residual is no longer reachable through `ItemViewer`'s
   own surface. It remains reachable only by driving `BreadcrumbItemViewerLifecycleCoordinator`
   directly, which is what the regression tests do deliberately, and by a viewer whose
   `UiSyncContext` is null.
2. **D2's retained theme still reaches the newly adopted host.** The coordinator retains the last
   theme and replays it in `ConfigureHost`'s newly-adopted branch, so a theme issued during the window
   is not lost: it is applied to the replacement host when that host is adopted. The observable
   outcome of the window is therefore a diagnostic exception on a contract violation, not a
   silently wrong or missing theme.
3. **The window does not exist on the production UI thread.** Every post issued by
   `BreadcrumbUiDispatcher` from the owning boundary runs inline, so `ConfigureHost`'s lambda executes
   before `ConfigureBreadcrumbDropDown` returns and the outgoing host is released in the same
   synchronous sequence that disposed it. The window is an artifact of a drainable, non-inline
   context, which is a test arrangement.

Trading a silent wrong-host theming for a loud `ObjectDisposedException` on a path that only a
contract violation can reach is the repository's fail-fast default. It is recorded here rather than
left to be discovered at review.

## D1b is a recorded residual this feature does not fix

**D1b — the unobservable dispose failure.** `BreadcrumbDropDownOpenCoordinator.Release()` calls
`_host.Dispose()` inside a `_operations.PostAsync(...)` whose returned task is **discarded**
(`_ = _operations.PostAsync(...)`), and `BreadcrumbUiDispatcher.Dispatch` routes any exception raised
inside that post to its error sink. A host that fails to dispose therefore leaves a WebView2-backed
`ToolStripDropDown` alive with nothing but a log line to show for it.

**This feature does not fix D1b.** The code lives in
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`, which constraint C1 lists as a forbidden
file: it is owned by sibling feature `breadcrumb-coordinator-hub-defects-501` for issue #462, whose
own spec cedes this feature's four production files to 488 and correspondingly retains the open
coordinator. Two concurrent branches editing that file is exactly the conflict the epic's file
assignment exists to prevent. D1b is recorded as a known residual and carried forward.

`[P1-T8]` records the corroborating evidence that
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` is in fact unmodified by this feature.

## Relationship to the observed test behaviour

This is the same residual that the `[P1-T4]` and `[P1-T6]` observations exercise deliberately rather
than encounter accidentally. The D1 regression test's discriminating assertion **is** the residual,
observed on purpose: it calls `SetTheme` on the captured outgoing host inside the window and asserts
that the `ObjectDisposedException` is thrown, because that throw is the only observation that
distinguishes the fixed ordering from the unfixed one. Decision D-10a records why the two other
observations cannot discriminate.

Output Summary: D1's fix introduces one narrow residual — an `ObjectDisposedException` from a
`SetTheme` landing between the synchronous disposal of the outgoing host and the posted
`ReleaseHostCore()`. It is **accepted rather than fixed**, for three reasons: D4 rejects an
off-boundary configure outright, D2's retained theme still reaches the newly adopted host, and the
window does not exist on the production UI thread where every post runs inline. **D1b**, the
unobservable dispose failure inside the sibling-owned open coordinator, is a recorded residual that
this feature does not fix.
