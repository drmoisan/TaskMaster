# Issue update mirror — #488

Timestamp: 2026-08-08T02-40
PostedAs: comment
Comment URL: https://github.com/drmoisan/TaskMaster/issues/488#issuecomment-5226505959
Issue URL: https://github.com/drmoisan/TaskMaster/issues/488
Source research: `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/research/2026-08-08T02-10-breadcrumb-item-viewer-lifecycle-coordinator.md`

Rationale for commenting rather than filing new issues: both additional defects are in the same
lifecycle family that #488 already tracks, and one of them (`SetBridgeCoordinator`) is reachable only
if #488's own Defect 3 fix lands. Filing separately would fragment a single fix. The Defect 1
correction has to live on #488 by definition.

---

## Correction to Defect 1, plus two adjacent defects in `BreadcrumbItemViewerLifecycleCoordinator.cs`

Raised from preparation research for epic #136 child F12 (issue #495), which owns
`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` — the file this issue's Defect 1
reasons about from the `ItemViewer.Breadcrumb.cs` side.

### Defect 1 is partly inaccurate — the previous host *is* disposed

Defect 1 currently states that `ReleaseHostCore()` "unsubscribes `PopupMessengerReady` and calls
`coordinator.Release()` (`:300-303`), but does not call `IBreadcrumbDropDownHost.Dispose()`."

Verified directly on the integration branch: `coordinator.Release()` does dispose the host.

```csharp
// QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:150-159
internal void Release()
{
    if (!Invalidate(release: true))
        return;
    _ = _operations.PostAsync(() =>
    {
        _detachPopupMessenger();
        _host.Dispose();
    });
}
```

So the disposal is present but **asynchronous and fire-and-forget** — posted through
`_operations.PostAsync` with the returned task discarded. The residual risk is therefore narrower
than "never disposed", but it is not nil, and it is arguably harder to reason about:

- disposal is deferred to a posted lambda, so it does not happen before the replacement host is
  constructed;
- `Invalidate(release: true)` returning `false` skips it entirely;
- the discarded task means a fault in `_detachPopupMessenger()` or `_host.Dispose()` is swallowed.

Suggest rewording Defect 1 from "the first host is never disposed" to "the first host is disposed
only via a discarded posted lambda, with no ordering guarantee against construction of its
replacement and no observation of failure." The `BreadcrumbDropDownIntegrationTests.cs:308` evidence
cited in the issue still stands on its own terms.

### Additional defect — `SetBridgeCoordinator` replaces without disposing, while `Dispose()` disposes

`BreadcrumbItemViewerLifecycleCoordinator.cs:64-77`. On replacement the method calls
`UnsubscribeBridge()` and then overwrites `_bridgeCoordinator`, but never disposes the outgoing
instance — whereas `Dispose()` (`:216`) does dispose it. The type is therefore inconsistent about
whether it owns the bridge coordinator: it owns it at teardown but not at replacement.

This is unreachable today **only** because of the reference-equality guard at `:66-69`. That is the
same guard family this issue's **Defect 3** proposes to make stricter by comparing providers. If
Defect 3's fix allows a genuinely different bridge coordinator to be installed on an already-
initialized viewer, this replacement path becomes live and will leak the outgoing coordinator's
`BreadcrumbMessengerHub` and its four event subscriptions. **The two should be fixed together.**

### Additional defect — `Reset()` detaches two surfaces with different synchrony

`BreadcrumbItemViewerLifecycleCoordinator.cs:197`. `Reset()` detaches the collapsed surface
synchronously but the popup surface only via a posted lambda. This is the same class as this issue's
**Defect 2** (an operation whose correctness depends on whether the dispatcher post runs inline),
at a different file and site. Worth folding into Defect 2's fix so the ordering rule is applied
once rather than per call site.

### Scope note

None of the above is being fixed under #495, whose epic carries a no-behavior-change NFR. #495's
tests pin **current** behavior. Whoever fixes this issue should expect to update those tests as part
of the fix rather than treat the change as a regression.
