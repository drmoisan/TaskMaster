# P9-T11 nonnumeric adapter correction design

Timestamp: 2026-07-27T03-30
Command: Get-Content QuickFiler/Viewers/ItemViewer.cs, QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs, and focused QuickFiler.Test/Viewers sources; git diff 314358197..HEAD; git grep ExcludeFromCodeCoverage 314358197 and HEAD; git diff --check
EXIT_CODE: 0
Output Summary: The bounded three-production/two-test design preserves the historical ItemViewer exclusion, removes or narrows branch-local exclusions only, and defines explicit production seams and creator-thread queue/drain tests.

## Decision

The correction is implementable within the P9-T12/P9-T13 scope: one new
production source, two modified production sources, two new test sources, and
exactly three adjacent legacy project includes. It requires no added source
scope, no added or widened exclusion, and no uncontrolled ambient dispatch.

The retained `ItemViewer` type-level exclusion at
`QuickFiler/Viewers/ItemViewer.cs:20` predates this branch and remains in
place. P9-T12 removes the two branch-local method exclusions in
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` at current lines 71 and 84. The
new `BreadcrumbItemViewerLifecycleCoordinator` has no
`ExcludeFromCodeCoverage` attribute. The seven popup adapter exclusions are
the only remaining popup exclusions; their bodies are narrowed to direct
WebView2 or WinForms bindings after host-neutral work moves to unexcluded
helpers.

## Baseline and exclusion invariant

The required branch-local baseline is commit
`314358197c4c309fc76af38de305bb2200ff8e82`. At that commit,
`BreadcrumbPopupUiOperations.cs` has exactly seven exclusions at lines 97,
377, 380, 387, 394, 421, and 431, and
`ItemViewer.Breadcrumb.cs` has the two exclusions at lines 71 and 84. The
live source has the same attribute inventory. Relative to that baseline,
P9-T12/P9-T13 may only remove the two ItemViewer.Breadcrumb exclusions or
narrow a popup exclusion; it must not add, relocate to cover additional
statements, or widen any exclusion. `origin/main` retains the separate
type-level `ItemViewer` exclusion. The correction therefore neither removes
that historical type-level exclusion nor uses it to justify host-neutral
uncovered code.

The final popup adapter ranges, inclusive of the attributes, are limited to:

| Adapter | Current range | Post-correction permitted body |
| --- | --- | --- |
| `ShowOwnedPopup` | 97-102 | `ToolStripDropDown.Show` and `Control.PointToClient` only |
| `CreateProductionControl` | 377-378 | `new WebView2` and direct `Dock` assignment only |
| `BeginProductionInitialization` | 380-385 | `IWebViewCoreInitializer.EnsureCoreWebView2Async` WebView2 cast/call only |
| `ReadProductionCore` | 387-392 | direct `WebView2.CoreWebView2` property access only |
| `BeginProductionNavigation` | 394-419 | direct WebView2 navigation/messenger construction binding only |
| `DisposeProductionSurface` | 421-423 | direct SDK dispose invocations only |
| navigation event binder replacing `NavigateToDocument` | 431-478 | direct CoreWebView2 and owner event add/remove bindings only |

The unexcluded helpers own validation, readiness state, event translation,
messenger construction failure cleanup, two-resource cleanup, and all branch
outcomes. Final accounting must record the actual final inclusive ranges, not
the current ranges if they shrink further.

## Production design

P9-T12 changes only these production sources:

1. Add `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`.
   This internal sealed coordinator owns the measurable lifecycle state now in
   `ItemViewer.Breadcrumb.cs`: breadcrumb hub, collapsed attachment,
   collapsed and popup messenger slots, configured open coordinator, resource
   ownership, reset generation, and the four bridge/host event subscriptions.
   It remains at or below 500 physical lines and carries no exclusion.
2. Modify `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` as a compatibility and
   native-wrapper surface only. It retains public and reflection-visible
   members and delegates host-neutral calls to the new coordinator. It removes
   both method exclusions. Native operations remain limited to the existing
   `WebView2` access, `RectangleToScreen`, `Screen.FromControl`, focus, and
   constructor wiring.
3. Modify `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` to move
   host-neutral navigation/readiness, validation, cleanup, and event-state
   work into measurable helpers while retaining the seven direct adapter
   boundaries above.

The new coordinator receives explicit collaborators rather than reading
ambient state: `BreadcrumbBridgeCoordinator`, `BreadcrumbMessengerHub`,
`BreadcrumbCollapsedAttachment`, `BreadcrumbPopupUiOperations`, and explicit
delegates for native candidate creation, host release, anchor bounds, working
area, focus, and selector cancellation. Its lifecycle API is internal and
minimal: `ConfigureHost`, `AttachCollapsed`, `AttachPopup`, `SetDroppedDown`,
`Reset`, and `Dispose`. `ItemViewer` retains its current internal method names
and overloads and forwards to this API. No existing public member, internal
reflection-visible wrapper, event name, overload, or exception contract is
removed or renamed.

The popup operations retain the existing primitive production seams and make
their contracts explicit:

```csharp
internal delegate Task BeginInitialization(
    IWebViewCoreInitializer initializer, Control control, CoreWebView2Environment environment);
internal delegate IDisposable NavigationSubscription(
    Action<long> navigationStarted,
    Action<long, bool, string> navigationCompleted,
    Action ownerDisposed);
internal delegate NavigationSubscription CreateNavigationSubscription(
    CoreWebView2 core, Control owner);
```

`Func<Control>`, `BeginInitialization`, `Func<Control, CoreWebView2>`,
`Func<CoreWebView2, Control, string, NavigationSurface>`, and
`Action<Control?, IWebViewMessenger?>` remain the direct adapter seams for
creation, initialization, core reading, navigation binding, and final SDK
disposal. The new `CreateNavigationSubscription` returns an object whose
`Dispose` removes the exact delegates that it added. The unexcluded navigation
helper accepts that subscription factory plus `Action navigate`; it creates
`BreadcrumbNavigationReadiness`, translates starting/completed/disposed
events, calls `BeginNavigation`, and disposes the subscription/readiness if
binding or navigation throws. The production factory is one thin excluded
method that captures the exact `NavigationStarting`, `NavigationCompleted`,
and `Disposed` delegate identities and removes those same instances.

The unexcluded two-resource cleanup helper invokes messenger disposal first
and control disposal second, continues after either exception, and rethrows
the first captured exception after both calls. `BeginProductionNavigation`
uses the host-neutral helper, then constructs `WebView2Messenger`; if that
construction fails it disposes the readiness subscription before propagating
the failure. `ReadProductionCore` only obtains the SDK property; the
unexcluded caller performs absent-core validation. This keeps all non-SDK
branches measurable.

## Subscription and disposal identity

The new coordinator stores named delegate instances once and uses those exact
instances for both operations:

1. Subscribe `BreadcrumbBridgeCoordinator.SelectionChanged`,
   `FolderArrowKeyDown`, `UnhandledArrow`, and `SelectorOpenStateChanged` to
   four stored handlers during initialization.
2. Subscribe the active `IBreadcrumbDropDownHost.PopupMessengerReady` to one
   stored handler after the host/open coordinator is installed.
3. On host replacement, first unsubscribe that exact host handler, release the
   old open coordinator, detach the old popup messenger from the hub, then
   create/configure the replacement, subscribe the stored handler, and attach
   any already-published popup messenger.
4. On messenger replacement, retain the same instance by re-attaching it;
   otherwise detach the old hub slot before attaching and storing the new
   messenger. A failed attach leaves the new slot unset.
5. On reset/dispose, first invalidate the generation used by queued callbacks,
   then unsubscribe the four bridge handlers and host handler, cancel/release
   the open coordinator, detach popup then collapsed messengers from the hub,
   reset/dispose the collapsed attachment, dispose the hub, dispose the bridge
   coordinator, and clear stored references. A late callback checks the
   generation/disposed state before attachment and cannot reattach a surface.

The order preserves existing `ItemViewer` wrapper semantics while placing the
subscription, replacement, and cleanup branches in an unexcluded coordinator.

## Test design and branch mapping

P9-T13 adds exactly these files, each at or below 500 physical lines:

- `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`

The first file owns lifecycle, subscription, replacement, reset, delegation,
geometry, and focus cases. The second owns direct primitive adapter seam,
navigation binding, initialization, core, readiness, and cleanup cases. Both
use MSTest, Moq, FluentAssertions, strict primitive probes, and no live
WebView2, `Control`, or `Panel` instance.

| Required test | Production seam and branches proved |
| --- | --- |
| `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | coordinator named bridge/host subscribe-unsubscribe identity; old popup detach before new host subscription and replacement messenger attach |
| `CandidateFailure_CleansMessengerAndReadiness` | candidate factory failure; readiness subscription and messenger cleanup; first failure preserved |
| `ResetDispose_LateCallbackDoesNotReattach` | reset generation invalidates queued callback; detach order; no late hub attachment |
| `SelectorDelegation_UsesCoordinator` | ItemViewer compatibility wrapper delegates `SetBreadcrumbDropDownState` and selector-open notification to coordinator |
| `QueuedGeometryAndFocusGuards_RunOnCreatorThread` | explicit queue drains geometry/focus callbacks only on its creator thread; disposed/focus guards and stale callback no-op |
| `CoreProbe_AbsentAndPresentPaths` | `Func<Control, CoreWebView2>` seam supplies present core and null core; unexcluded absent-core validation and report path |
| `Initializer_ThrowAndNullTaskPaths` | `BeginInitialization` seam throws and returns null; both exceptions and cleanup/report branches |
| `MessengerConstructionFailure_DisposesReadiness` | navigation seam returns readiness then messenger construction probe throws; subscription/readiness is disposed exactly once |
| `NavigationBinder_TranslatesDetachesAndCleansOnThrow` | fake `CreateNavigationSubscription` records start/completed/disposed callbacks, navigation-id/error translation, exact detach, and bind/navigate failure cleanup |
| `TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup` | injected messenger/control disposers both run; first exception is rethrown only after second resource is attempted |

The tests exercise the production `BreadcrumbPopupUiOperations` seam paths and
the new coordinator directly. They do not replace the coordinator with a
higher-level host/provider substitute.

Each test creates a private, explicit `QueuedCreatorThreadSynchronizationContext`
that records `Post` callbacks in FIFO order with the creator managed-thread ID.
The test invokes `DrainOnCreatorThread()` synchronously on that same thread;
it asserts each callback executes there and fails if a different thread drains
the queue. The test context is a concrete queue owned by the test, not a base
`SynchronizationContext`, `CreateForCurrentThreadTests`, a thread-pool post,
or an ambient application context. There are no waits, delays, retries,
temporary files, `[Ignore]`, or `[DoNotParallelize]` attributes.

## Project include and scope invariants

P9-T12/P9-T13 add exactly these three adjacent legacy project includes:

```xml
<!-- QuickFiler/QuickFiler.csproj, adjacent to other Viewers includes -->
<Compile Include="Viewers\\BreadcrumbItemViewerLifecycleCoordinator.cs" />
<!-- QuickFiler.Test/QuickFiler.Test.csproj, adjacent to Breadcrumb viewer tests -->
<Compile Include="Viewers\\BreadcrumbItemViewerLifecycleCoordinatorTests.cs" />
<Compile Include="Viewers\\BreadcrumbPopupUiOperationsDirectAdapterTests.cs" />
```

The change budget is exactly one added production file, two modified production
files, two added test files, and three project include entries. No other source,
test, project, coverage, filter, policy, configuration, or protected file is
authorized. Each new or changed measurable host-neutral member must achieve at
least 90% coverage; every changed/new source and test file remains at or below
500 physical lines. The existing `coverage.config` and `.csharpierignore`
content remains unchanged.

## Scoped invariant review

Reviewed source state:

- `QuickFiler/Viewers/ItemViewer.cs` retains the historical type-level
  exclusion at line 20.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is 399 physical lines and
  presently contains only the two method exclusions slated for removal.
- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is 480 physical lines
  and presently contains the seven P5-T104 popup exclusions.
- `git diff 314358197..HEAD --` these production paths shows only
  `ItemViewer.Breadcrumb.cs` changes; it adds no popup exclusion after the
  branch-local baseline.
- `git diff --check` returned no whitespace errors before this design artifact
  or P9-T11 marker update.

DESIGN: APPROVED
