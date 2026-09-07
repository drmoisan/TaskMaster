# Forbidden Files Untouched ([P7-T3])

Timestamp: 2026-08-28T06-13

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/ItemViewer.Designer.cs QuickFiler/Viewers/IBreadcrumbDropDownHost.cs QuickFiler/Viewers/IItemViewer.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/QuickFiler.csproj QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
```

EXIT_CODE: 0

## Result

**Output: no lines.**

All eleven files are byte-identical to their state at `BASE_SHA`
`12465043e052fce66a1861bf1ddd037a1aa81afc`.

| # | File | Owner / reason it is forbidden |
| --- | --- | --- |
| 1 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | sibling feature `breadcrumb-coordinator-hub-defects-501` (#462). Holds the D1b residual and the rejected synchronous-`Release()` alternative. |
| 2 | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | sibling feature 501 |
| 3 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | sibling feature 501 |
| 4 | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | sibling feature 501 |
| 5 | `QuickFiler/Viewers/ItemViewer.cs` | sibling feature `itemviewer-surface-defects-489`. Carries the type-level `[ExcludeFromCodeCoverage]` at line 20 that assumption D489-2 depends on, and the `UiSyncContext` property D4's guard compares against. |
| 6 | `QuickFiler/Viewers/ItemViewer.Designer.cs` | sibling feature 489. Designer-generated; D5's design was chosen specifically to avoid editing it. |
| 7 | `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | public contract; no public API change is permitted |
| 8 | `QuickFiler/Viewers/IItemViewer.cs` | public contract |
| 9 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | sibling feature `qfc-item-controller-defects-484`. Read during `[P5-T6]` to discharge the research §3.5 open item, and deliberately not edited — the correct response to the unobserved-task finding is a new issue, not a change here. |
| 10 | `QuickFiler/QuickFiler.csproj` | this feature adds no production file, so the production project file must not change |
| 11 | `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | exactly 500 lines, at the ceiling, and required byte-identical at delivery. Holds `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` and `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`, two of the nine constraining tests. |

## Why the empty diff is the right proof for each

Several of these files were **read** during execution — `ItemViewer.cs` to confirm the coverage
exclusion and the `UiSyncContext` shape, `ItemViewer.Designer.cs` for its line count,
`QfcItemController.ViewerSetup.cs` to trace the `InitializeWebViewAsync` call sites, and
`BreadcrumbDropDownIntegrationTests.cs` for the constructor arrangement `[P6-T1]` mirrors. Reading is
permitted; writing is not. An empty `git diff --name-only` distinguishes the two, which a test outcome
or a line count could not: a passing test proves nothing about whether its file was edited, and a file
can be edited without changing its line count.

Two of these files are additionally covered by their own dedicated checks, because separate criteria
depend on them: `[P5-T5]` for `ItemViewer.Designer.cs` and `[P1-T6]` and `[P8-T8]` for
`BreadcrumbDropDownIntegrationTests.cs`. Those checks agree with this one.

Output Summary: `git diff --name-only <BASE_SHA>` over all **eleven** forbidden files produces **no
output lines**. Every one is byte-identical to its pre-change state.
