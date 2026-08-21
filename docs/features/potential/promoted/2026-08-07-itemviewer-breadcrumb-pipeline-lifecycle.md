# itemviewer-breadcrumb-pipeline-lifecycle (Issue #488)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/itemviewer-breadcrumb-pipeline-lifecycle/ (Issue #488)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #488
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/488
- Last Updated: 2026-08-08
## Summary

Five lifecycle defects in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`. All are out of scope to fix
under epic #136's no-behavior-change NFR. Two of them (Defects 1 and 3) are reachable through the
existing pooled-viewer reuse path in production.

## Defect 1 — `ConfigureBreadcrumbDropDown` leaks the previous host on WebView2 environment change

`ItemViewer.Breadcrumb.cs:147-176`. The idempotence guard at `:147-153` returns early only when the
existing host is a concrete `BreadcrumbDropDownHost` **and**
`ReferenceEquals(existing.Environment, environment)`. When the environment reference differs, control
falls through to `:158-168` and constructs a second `BreadcrumbDropDownHost` over the same
`_l0vhBreadcrumb_WebView2`. The first host is never disposed by this file:
`BreadcrumbItemViewerLifecycleCoordinator.cs:127-142` calls `ReleaseHostCore()`, which unsubscribes
`PopupMessengerReady` and calls `coordinator.Release()` (`:300-303`), but does not call
`IBreadcrumbDropDownHost.Dispose()` — even though `IBreadcrumbDropDownHost` is `IDisposable`
(`Viewers/IBreadcrumbDropDownHost.cs:19`). `BreadcrumbDropDownIntegrationTests.cs:308` asserts
`host.Dispose()` is called exactly once on **viewer** disposal, proving disposal is viewer-lifetime
scoped rather than host-replacement scoped.

Reachable in production: `QfcItemController.ViewerSetup.cs:166` passes `_webViewEnvironment`, which is
recreated per controller initialization while `ItemViewer` instances are pooled and reused
(`ViewerSetup.cs:396` calls `ResetBreadcrumb()` on reuse, which does not reset host identity). Every
environment change leaks one WebView2-backed popup host for the lifetime of the viewer.

## Defect 2 — `SetBreadcrumbTheme` can be lost when issued off the UI thread

`ItemViewer.Breadcrumb.cs:197-198` forwards synchronously to
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme` (`:155-160`), which reads
`DropDownHost` → `_openCoordinator?.Host`. `_openCoordinator` is assigned inside an asynchronous post
(`ConfigureHost`, `:120-152`). On the UI thread the post runs inline
(`BreadcrumbUiDispatcher.cs:78-95`), so ordering holds and production is currently safe
(`QfcItemController.ViewerSetup.cs:166-167`). Off the UI thread — or after any `ConfigureAwait(false)`
resumption, which `BreadcrumbUiDispatcher.cs:263-268` documents as a real scenario — the post is
genuinely deferred, `DropDownHost` is still null when `SetTheme` reads it, and the popup surface keeps
the previous theme with no error surfaced. Same class of defect as the dark-mode stale-label family
(issues #254 / #269).

## Defect 3 — `InitializeBreadcrumbPipeline` silently discards a second, different provider

`ItemViewer.Breadcrumb.cs:45-48`. The guard returns without comparing providers, so a caller supplying
a different `IFolderHierarchyProvider` to an already-initialized viewer gets no error and no effect.
Pooled viewer reuse reaches this path: `QfcItemController.ViewerSetup.cs:140-146` guards
`EnsureBreadcrumbPipeline` on `viewer.BreadcrumbCoordinator == null`, so a viewer reused across two
controllers with different `_globals.Ol.FolderTreeService` instances keeps the first controller's
provider. `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` (`:66-69`) does compare by
reference before short-circuiting — the coordinator is stricter than its own wrapper.

## Defect 4 — `BreadcrumbCoordinator` initialization is a non-atomic read-then-write

`ItemViewer.Breadcrumb.cs:45` reads and `:59` writes with no synchronization and no memory barrier.
Two threads entering `InitializeBreadcrumbPipeline` concurrently both construct a
`BreadcrumbItemViewerLifecycleCoordinator` and a `BreadcrumbBridgeCoordinator`; one pair is silently
discarded without being disposed, leaking its `BreadcrumbMessengerHub` and its bridge subscriptions
(`BreadcrumbItemViewerLifecycleCoordinator.cs:73-76`). The same shape recurs at `:147`/`:159` (host)
and `:281`/`:287` (resource owner). Production currently calls only from the UI thread, so the window
is not known to be hit, but nothing in the type declares or enforces UI-thread affinity and
`AttachBreadcrumbWebViewAsync` is async-facing, which invites off-thread callers.

## Defect 5 — `EnsureBreadcrumbResourceOwnership` can create a `Container` that `Dispose` never disposes

`ItemViewer.Breadcrumb.cs:286-288` executes `components ??= new Container();` then
`components.Add(_breadcrumbResourceOwner)`. `ItemViewer.Designer.cs:16-23` disposes `components` only
if it is non-null at the moment `Dispose(bool)` runs. If breadcrumb configuration first occurs after
disposal has begun — reachable via the deferred `ConfigureHost` post
(`BreadcrumbItemViewerLifecycleCoordinator.cs:120`) racing `Control.Dispose` — the newly created
`Container` and the `BreadcrumbResourceOwner` inside it are never disposed, so
`DisposeBreadcrumbResources` (`:291-296`) never runs and the hub and messengers leak. The generation
guard at `BreadcrumbItemViewerLifecycleCoordinator.cs:122-125` protects the coordinator's own state
but not this file's container creation.

## Acceptance Criteria (early draft)

- [ ] Host replacement on environment change disposes the previous host.
- [ ] `SetBreadcrumbTheme` either orders after host configuration or reports a deferred application.
- [ ] A second, different `IFolderHierarchyProvider` either fails fast or re-initializes explicitly.
- [ ] Pipeline initialization is atomic, or UI-thread affinity is declared and enforced.
- [ ] A `Container` created during teardown is disposed, or creation during teardown is refused.
- [ ] Regression tests cover each fixed behavior deterministically.

## Constraints & Risks

- `ItemViewer.Breadcrumb.cs` is assigned to epic child F14; `BreadcrumbItemViewerLifecycleCoordinator.cs`
  and `BreadcrumbBridgeCoordinator.cs` to F12; `BreadcrumbUiDispatcher.cs` and
  `BreadcrumbDropDownHost.cs` to F13. A fix spans three children's file sets, so reconcile against all
  three plans before scheduling.
- Issue #400's live remediation plan also authorizes edits to `ItemViewer.Breadcrumb.cs`.

## Next Step

- [ ] Promote to GitHub issue (bug template)
