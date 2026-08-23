# Remediation Coverage Accounting Baseline

Timestamp: 2026-07-21T18-55Z
Command: Parse coverage-remediation-baseline.2026-07-21T18-54.cobertura.xml and reconcile it with coverage-baseline.2026-07-21T16-00.cobertura.xml, coverage-final.2026-07-21T17-44.cobertura.xml, coverage-delta.2026-07-21T17-49.md, and coverage-accounting-scope-change.2026-07-21T18-01.md
EXIT_CODE: 0
Output Summary: Live remediation baseline coverage is 89,120/105,884 = 84.1676%. Reviewed-base coverage is 87,397/104,178 = 83.8920%. The reviewed-head accounting reports 1,030/1,030 measurable changed/new executable lines, all 112 measurable changed/new members at 100%, and a minimum selector-type rate of 98.2270%.

## Numeric Coverage Headlines

| Scope | Covered | Valid | Rate |
|---|---:|---:|---:|
| Reviewed base repository | 87,397 | 104,178 | 83.8920% |
| Reviewed head repository | 89,113 | 105,884 | 84.1610% |
| Live remediation baseline repository | 89,120 | 105,884 | 84.1676% |
| Reviewed modified tracked hunks | 355 | 355 | 100.0000% |
| Reviewed changed/new production executable lines | 1,030 | 1,030 | 100.0000% |
| Reviewed measurable changed/new members | 112 | 112 | 100.0000% minimum |

The seven-line live-run variation is test-path execution variance in the same reviewed source tree. The reviewed-base and reviewed-head artifacts remain the authoritative base-to-head change comparison.

## Per-Selector-Type Coverage at Reviewed Head

| Type | Covered | Valid | Rate |
|---|---:|---:|---:|
| `BreadcrumbBridgeCoordinator` | 264 | 264 | 100.0000% |
| `BreadcrumbDropDownHost` | 215 | 215 | 100.0000% |
| `BreadcrumbMessengerHub` | 141 | 141 | 100.0000% |
| `BreadcrumbMessengerHub.Attachment` | 10 | 10 | 100.0000% |
| `BreadcrumbMessengerHub.CachedState` | 5 | 5 | 100.0000% |
| `BreadcrumbPopupPlacement` | 44 | 44 | 100.0000% |
| `BreadcrumbPopupPlacementResult` | 4 | 4 | 100.0000% |
| `BreadcrumbRenderProjection` | 85 | 85 | 100.0000% |
| `BreadcrumbCellRender` | 12 | 12 | 100.0000% |
| `BreadcrumbRowRender` | 20 | 20 | 100.0000% |
| `BreadcrumbSubfolderRender` | 6 | 6 | 100.0000% |
| `BreadcrumbSelectionSession` | 135 | 135 | 100.0000% |
| `BreadcrumbSelectorActivationMessage` | 11 | 11 | 100.0000% |
| `BreadcrumbSelectorKeyMessage` | 5 | 5 | 100.0000% |
| `BreadcrumbSelectorMessageSerializer` | 85 | 85 | 100.0000% |
| `BreadcrumbSelectorToggleMessage` | 1 | 1 | 100.0000% |
| `BreadcrumbSelectorViewMessage` | 19 | 19 | 100.0000% |
| `BreadcrumbStateModel` | 88 | 88 | 100.0000% |
| `BreadcrumbStateRow` | 135 | 135 | 100.0000% |
| `FolderBreadcrumbBridgeRouter` | 277 | 282 | 98.2270% |

## Measurable Changed/New Member Accounting

The reviewed AST-to-Cobertura audit resolved 112 changed/new methods, constructors, accessors, and expression-bodied properties with numeric sequence points. All 112 are at 100.0000%; zero are below 90%. The member-level source of record is `evidence/qa-gates/coverage-delta.2026-07-21T17-49.md` and its cited `coverage-final.2026-07-21T17-44.cobertura.xml`. Required numeric values are present; none is treated as zero, skipped, or unverified.

## Direct Nonnumeric Adapter Surfaces and Deterministic Seams

| Nonnumeric surface | Reason numeric execution is unavailable | Deterministic seam coverage |
|---|---|---|
| `BreadcrumbDropDownHost.CreateProductionSurfaceAsync` | Direct WebView2 construction, initialization, navigation, and document-readiness adapter | `ProductionConstructor_RejectsMissingInitializerOrHtml`; `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`; remediation readiness tests in P1/P2 |
| `BreadcrumbDropDownHost.ShowOwnedPopup` | Direct WinForms `ToolStripDropDown.Show` adapter | `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement`; `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure` |
| Pre-existing excluded partial `ItemViewer` issue #400 code and `BreadcrumbResourceOwner` | The `ItemViewer` type-level exclusion predates issue #400 and covers direct WinForms composition | `SetFolderDroppedDownTrue_OpensOnceWithScreenBoundsAndWorkingArea`; `ClosedAndPopupAttachmentAndTheme_AreExactlyOncePerSurface`; `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` |
| Pre-existing excluded `QfcItemController.InitializeWebViewAsync` and `EnsureBreadcrumbPipeline` issue #400 additions | Method-level exclusions predate issue #400 and contain direct WebView/controller composition | `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`; `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`; `Cleanup_ResetsInjectedHostForPooledViewerReuse` |

No unavailable required numeric value was converted into a passing numeric result. Final remediation accounting must recompute all measurable values and re-audit every bounded nonnumeric surface.
