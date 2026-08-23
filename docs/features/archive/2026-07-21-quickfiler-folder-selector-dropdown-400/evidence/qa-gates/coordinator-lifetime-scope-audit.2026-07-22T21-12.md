# Coordinator lifetime scope audit

Timestamp: `2026-07-22T21:12:00-04:00`

Result: PASS.

- Production scope is exactly the three P6-T1 sources: the new `BreadcrumbCoordinatorUpgradeLifetime.cs`, plus `BreadcrumbBridgeCoordinator.cs` and `ItemViewer.Breadcrumb.cs`.
- Test scope is within the three approved files. `BreadcrumbCoordinatorLifecycleTests.cs` contains the new failure-first coverage, `BreadcrumbCollapsedSurfaceReadinessTests.cs` has formatter-only changes, and `BreadcrumbMessengerHubTests.cs` remained unchanged.
- `QuickFiler.csproj` contains exactly one `Viewers\BreadcrumbCoordinatorUpgradeLifetime.cs` compile include.
- The suggestion-population route contains no `CancellationToken.None`. The one remaining occurrence in `BreadcrumbBridgeCoordinator.cs` is the unrelated inbound JSON `RouteAsync` path.
- Every suggestion population receives a coordinator generation and owned cancellation source. Later population, clear, reset, reuse, and disposal detach and cancel the prior lease.
- A completion checks `IsCurrent` before dispatch at `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync`; the queued action repeats the check through `BreadcrumbCoordinatorUpgradeLifetime.Guard` before any hub post or selector-state publication.
- ItemViewer reset invalidates coordinator work before popup and attachment reset. ItemViewer disposal cancels and unsubscribes the coordinator before detaching the messenger or disposing the hub.
- The subscribed inbound delegate is retained and removed by exact reference. The regression messenger rejects any non-identical remove delegate.
- Superseded cancellation sources are not disposed until the provider operation settles; the regression test verifies the canceled token's wait handle remains usable before settlement.
- Final physical line counts are 309, 496, 399, 459, 488, and 414 for the six approved source/test files. All are at most 500 lines.
- `git diff --check` returned exit code `0`.

Read-only review noted that a normally settled current linked source remains owned until the next population, clear/reset, or coordinator disposal, and that publication is serialized under the lifetime monitor. The current production messenger path is nonblocking and UI-bound, and coordinator disposal releases the retained source. These observations do not violate P6-T1 through P6-T8 and require no scope expansion in this batch.
