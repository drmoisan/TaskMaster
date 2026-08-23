# Collapsed Readiness Scope and Adapter Audit

Timestamp: 2026-07-22T01:27:18Z
Command: scoped `Select-String` inspections for `BreadcrumbNavigationReadiness`, navigation events/IDs, `CoreWebView2`, initialization/navigation calls, `ExcludeFromCodeCoverage`, and the exact controller project include; deterministic line counts and `git diff --check` over all P4 production/test files and `QuickFiler.csproj`.
EXIT_CODE: 0
Output Summary: Popup and collapsed surfaces use the same exact-NavigationId correlation contract. Direct WebView2 calls are method-bounded and enumerated, the new controller has exactly one project include, no class-level exclusion was added, all P4 C# files remain below 500 lines, and the scoped diff has no whitespace error.

## Shared correlation contract

- `BreadcrumbNavigationReadiness` is defined once in `BreadcrumbWebViewSurfaceFactory.cs` and owns first post-request `NavigationStarting.NavigationId` capture, exact matching `NavigationCompleted.NavigationId`, one terminal gate, translated failure, cancellation, disposal, and handler detachment.
- Popup creation calls `BreadcrumbWebViewSurfaceFactory.NavigateToDocument`, which registers `NavigationStarting` and `NavigationCompleted` before `BeginNavigation` invokes `NavigateToString`.
- Collapsed ItemViewer candidate creation calls the same `NavigateToDocument` method and passes the resulting `BreadcrumbNavigationReadiness` to `BreadcrumbCollapsedSurfaceController`; no second correlation implementation exists.

## Direct adapter enumeration

- `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` contains the bounded popup WebView control creation, `EnsureCoreWebView2Async`, initialized-core read, and navigation initiation.
- `BreadcrumbWebViewSurfaceFactory.NavigateToDocument` contains the bounded SDK navigation event subscribe/unsubscribe adapter and maps SDK event arguments to the host-neutral readiness lifetime.
- `ItemViewer.AttachBreadcrumbWebViewAsync` contains the bounded existing collapsed-control core read and `NavigateToString` callback used to create the collapsed candidate. The host-neutral controller itself has no `CoreWebView2` access.
- The two factory adapter methods retain method-level `ExcludeFromCodeCoverage` attributes at lines 176 and 216. No class-level exclusion was added to the factory, controller, ItemViewer partial, ViewerSetup partial, or messenger hub, and no host-neutral readiness/controller/hub class is excluded.

## Scope and mechanical checks

- `BreadcrumbCollapsedSurfaceController.cs` project include count: 1.
- `BreadcrumbCollapsedSurfaceController.cs`: 308 lines.
- `BreadcrumbWebViewSurfaceFactory.cs`: 273 lines.
- `ItemViewer.Breadcrumb.cs`: 454 lines.
- `QfcItemController.ViewerSetup.cs`: 426 lines.
- `BreadcrumbMessengerHub.cs`: 462 lines.
- `BreadcrumbCollapsedSurfaceReadinessTests.cs`: 468 lines.
- `BreadcrumbDropDownReadinessTests.cs`: 309 lines.
- `BreadcrumbUiThreadDispatchTests.cs`: 379 lines.
- `QfcItemControllerBreadcrumbDropDownTests.cs`: 376 lines.
- `BreadcrumbMessengerHubTests.cs`: 414 lines.
- `git diff --check` EXIT_CODE: 0.
