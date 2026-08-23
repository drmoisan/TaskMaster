# P9-T7 nonnumeric adapter accounting

The only bounded nonnumeric UI boundaries are seven unchanged direct WebView2/WinForms adapter operations in `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`. Each is already marked `[ExcludeFromCodeCoverage]`; no exclusion changed or widened.

| Adapter surface | Exact lines / existing attribute | Deterministic injected branch coverage |
| --- | --- | --- |
| `ShowOwnedPopup` | 97-102; `[ExcludeFromCodeCoverage]` at 97 | `BreadcrumbDropDownLifecycleCoverageTests` injected `ShowPopup` delegate covers placement/show invocation. |
| `CreateProductionControl` | 377-378; attribute at 377 | `BreadcrumbDropDownCoverageThresholdTests` injects control creation through the operations constructor. |
| `BeginProductionInitialization` | 380-386; attribute at 380 | `BreadcrumbDropDownLifecycleConcurrencyTests` injects initialization success and failure delegates. |
| `ReadProductionCore` | 387-393; attribute at 387 | `BreadcrumbDropDownCoverageThresholdTests` injects present and absent core branches. |
| `BeginProductionNavigation` | 394-420; attribute at 394 | `BreadcrumbDropDownLifecycleCoverageTests` injects navigation and messenger-construction success/failure branches. |
| `DisposeProductionSurface` | 421-423; attribute at 421 | `BreadcrumbDropDownLifecycleCoverageTests` injects close/dispose paths and duplicate-close behavior. |
| `NavigateToDocument` | 431-479; attribute at 431 | `BreadcrumbDropDownLifecycleConcurrencyTests` injects navigation starting, completion, disposal, and failure callbacks. |

Direct ItemViewer adapter lines are limited to `ItemViewer.Breadcrumb.cs` WebView core/messenger and navigation setup (90-101), popup host focus delegate (170-174), direct rectangle/screen placement providers (182-185), and direct WebView focus (248-252). Their deterministic contract coverage is in `ItemViewerBreadcrumbDropDownContractTests` and `BreadcrumbDropDownIntegrationTests`.

Minimal one-line coordinator delegation glue is separate from the direct adapter lines: `SetDroppedDown` at 266, coordinator reset at 272, and selector-open-state notification at 297. Their deterministic injected behavior is covered by `BreadcrumbDropDownOpenCoordinatorTests`, `BreadcrumbDropDownLifecycleCoverageTests`, and `BreadcrumbDropDownIntegrationTests`.

No exclusion was added, removed, or widened. No host-neutral selector or lifecycle body is excluded; host-neutral open orchestration remains in the unexcluded coordinator and is covered at or above the P9-T6 threshold.
