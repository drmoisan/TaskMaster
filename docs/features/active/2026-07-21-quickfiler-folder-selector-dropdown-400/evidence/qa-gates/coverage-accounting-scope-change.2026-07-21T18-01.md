# Coverage accounting scope change

Timestamp: 2026-07-21T18-01Z

Decision: `scope_change`

Command: `git show df5ad49c909f6b739edef45d0336151f44e827a6:QuickFiler/Viewers/ItemViewer.cs; git show df5ad49c909f6b739edef45d0336151f44e827a6:QuickFiler/Controllers/QfcItemController.ViewerSetup.cs; rg -n -C 3 'ExcludeFromCodeCoverage' QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler/Viewers/ItemViewer.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs; git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- coverage.config; git hash-object coverage.config`

EXIT_CODE: 0

## Exact reason

Direct numeric execution of the live WebView2, `ToolStripDropDown`, and WinForms display adapters is not automatable under this plan's deterministic unit-test policy. That policy prohibits a live UI/display, browser environment, user interaction, external process, mutable machine state, and manual evidence. Executing the direct adapters for line coverage would violate those constraints and would not produce deterministic unit evidence.

This is a scope change, not a human exception and not a coverage-threshold waiver. Coverage accounting changes from “numeric execution for every adapter line” to:

1. Numeric coverage for every measurable host-neutral selector type and changed/new executable member.
2. Numeric changed/new production-line coverage for all instrumentable code.
3. Deterministic automated integration-seam tests for direct excluded UI adapters.
4. Explicit accounting of each nonnumeric surface, with no invented numeric rate.
5. Independent feature-review scrutiny of this scope decision and its boundaries.

## Exact nonnumeric surfaces

### Two new direct Host method exclusions

- `BreadcrumbDropDownHost.CreateProductionSurfaceAsync`: direct third-party WebView2 surface creation and initialization adapter.
- `BreadcrumbDropDownHost.ShowOwnedPopup`: direct WinForms `ToolStripDropDown.Show` display adapter.

Both are method-level exclusions. `BreadcrumbDropDownHost` has no class-level exclusion. Its host-neutral lifecycle, placement, selection, focus, error, reuse, and disposal behavior remains injected and numerically covered.

### Pre-existing ItemViewer exclusion touched by issue #400

- `QuickFiler/Viewers/ItemViewer.cs` already applied `[ExcludeFromCodeCoverage]` to the whole partial `ItemViewer` type at baseline.
- The unchanged pre-existing type attribute accounts for issue #400 changes in `ItemViewer.Breadcrumb.cs`, `ItemViewer.FolderSearch.cs`, and nested `BreadcrumbResourceOwner`.
- Issue #400 adds no `ItemViewer` coverage attribute and does not widen the pre-existing type boundary.

### Exact pre-existing Qfc method exclusions touched by issue #400

- `QfcItemController.InitializeWebViewAsync`
- `QfcItemController.EnsureBreadcrumbPipeline`

Both exact `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attributes exist in the baseline file. Issue #400 adds zero Qfc coverage attributes. No other Qfc method or class is included in this scope change. The pre-existing excluded `ResolveControlGroupsAsync` remains unchanged and is not used to account for issue #400 work.

## Baseline and configuration proof

- BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`.
- `git show <baseline>:QuickFiler/Viewers/ItemViewer.cs` contains the type-level `[ExcludeFromCodeCoverage]` attribute.
- `git show <baseline>:QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` contains the method-level attributes on `InitializeWebViewAsync` and `EnsureBreadcrumbPipeline`.
- The changed/new production scope has no new class-level exclusion.
- The only new exclusions are the two exact Host adapter methods listed above.
- `git diff --exit-code <baseline> -- coverage.config` returns 0.
- Baseline and current `coverage.config` Git object hash: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`.
- No package, runsettings, or coverage-filter configuration was weakened.

## Numeric gates retained by the changed scope

| Scope | Result | Gate |
|---|---:|---|
| Repository | 89,113/105,884 = 84.1610% | PASS, at least 80% |
| Modified tracked hunks | 355/355 = 100.0000% | PASS, no regression from 32/36 = 88.8889% |
| All changed/new production executable lines | 1,030/1,030 = 100.0000% | PASS, at least 90% |
| Measurable changed/new members | 112/112 at 100.0000% | PASS, every member at least 90% |
| Measurable dedicated selector types | Minimum 98.2270% | PASS, every type at least 90% |
| `FolderBreadcrumbBridgeRouter` | 277/282 = 98.2270% | PASS, above 199/204 = 97.5490% baseline |
| Non-excluded Qfc issue #400 hunk | 13/13 = 100.0000% | PASS |

The full numeric derivation is recorded in `evidence/qa-gates/coverage-delta.2026-07-21T17-49.md` against `coverage-final.2026-07-21T17-44.cobertura.xml`.

## Exact automated integration-seam verification

The following passing MSTest cases verify the excluded direct adapters and their surrounding integration without opening a live UI or browser:

- `Constructor_OwnsAutoClosingToolStripDropDownWithoutGlobalTopmostForm`
- `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement`
- `ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks`
- `OpenAndClose_TransferFocusIntoPendingOptionAndBackToAnchor`
- `OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing`
- `OpenAsync_ZeroWorkingArea_RestoresSelectionAndFocus`
- `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure`
- `NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications`
- `ResetAndDispose_HandleOpenOrPartialStateAndRejectLaterUse`
- `Reset_DisposesAnOrphanedPartialSurface`
- `ProductionConstructor_RejectsMissingInitializerOrHtml`
- `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`
- `Reset_DisposesSurfaceClearsHostAndAllowsOneFreshInitialization`
- `OpenAsync_PartialInitializationFailureDisposesAndRestoresFocusAndSelection`
- `Dispose_ClosesUncommittedDisposesSurfaceAndPreventsLaterCallbacks`
- `SetFolderDroppedDownTrue_OpensOnceWithScreenBoundsAndWorkingArea`
- `SetFolderDroppedDownFalse_RequestsOneUncommittedCloseAndRollback`
- `ClosedAndPopupAttachmentAndTheme_AreExactlyOncePerSurface`
- `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks`
- `InitializationFailure_CancelsSessionWithoutDuplicateClose`
- `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces`
- `ExistingAnchor_RemainsTheDesignerWebViewClosedSurface`
- `ProductionConfiguration_AcceptsExistingEnvironmentAndInitializer`
- `InjectedConfiguration_AcceptsHostAndScreenGeometryProviders`
- `ExistingFolderEventsAndDropDownIntentSignatures_AreUnchanged`
- `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
- `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`
- `Cleanup_ResetsInjectedHostForPooledViewerReuse`
- `OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler`

These cases pass within `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md` and the complete 5,830-test final run in `evidence/qa-gates/final-mstest-coverage.2026-07-21T17-44.md`.

## Review requirement

Independent feature review must audit that:

- the scope change remains limited to the exact surfaces above;
- no direct adapter grows host-neutral logic under an exclusion;
- no new class-level exclusion or coverage-filter weakening exists;
- integration-seam tests continue to exercise ownership, environment reuse, placement, focus, close/rollback, failure cleanup, disposal, and pooled reuse;
- all measurable numeric thresholds remain satisfied.

Output Summary: `scope_change` recorded. Deterministic numeric coverage remains mandatory for all measurable host-neutral and changed/new selector code; only the exact direct UI/integration surfaces above use automated seam verification instead of live numeric execution, with independent feature review required.
