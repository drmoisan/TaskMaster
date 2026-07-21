# Final numeric coverage delta

Timestamp: 2026-07-21T17-49Z

Command: `$baselineCoveragePath = 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\baseline\coverage-baseline.2026-07-21T16-00.cobertura.xml'; $finalCoveragePath = 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-final.2026-07-21T17-44.cobertura.xml'; $baselineSha = 'df5ad49c909f6b739edef45d0336151f44e827a6'; [xml]$baselineCoverage = Get-Content -LiteralPath $baselineCoveragePath -Raw; [xml]$finalCoverage = Get-Content -LiteralPath $finalCoveragePath -Raw; $changedDiff = @(git diff --unified=0 $baselineSha -- '*.cs'); if (-not $baselineCoverage.coverage -or -not $finalCoverage.coverage) { throw 'Cobertura coverage root missing.' }`

EXIT_CODE: 0

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

Baseline Cobertura: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/coverage-baseline.2026-07-21T16-00.cobertura.xml`

Final Cobertura: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-44.cobertura.xml`

## Repository and changed-scope results

| Scope | Baseline | Final | Delta | Result |
|---|---:|---:|---:|---|
| Repository line coverage | 87,397/104,178 = 83.8920% | 89,113/105,884 = 84.1610% | +1,716 covered; +1,706 valid; +0.2690 percentage points | PASS, at least 80% |
| Modified tracked hunks | 32/36 = 88.8889% | 355/355 = 100.0000% | +11.1111 percentage points | PASS, no regression |
| All changed/new production executable lines | No aggregate new-file baseline | 1,030/1,030 = 100.0000% | 0 uncovered | PASS, at least 90% |
| `FolderBreadcrumbBridgeRouter` | 199/204 = 97.5490% | 277/282 = 98.2270% | +0.6780 percentage points | PASS, no per-file regression |
| Router modified hunks | 24/24 = 100.0000% | 109/109 = 100.0000% | 0 percentage points | PASS |

The added `SetSuggestions_ProviderCancellation_PropagatesCancellation` regression test covers the previously uncovered `OperationCanceledException` propagation branch. The final report has no uncovered changed/new executable production line.

## New and changed selector types

| Type | Covered | Valid | Line coverage | Result |
|---|---:|---:|---:|---|
| `BreadcrumbBridgeCoordinator` | 264 | 264 | 100.0000% | PASS |
| `BreadcrumbDropDownHost` | 215 | 215 | 100.0000% | PASS |
| `BreadcrumbMessengerHub` | 141 | 141 | 100.0000% | PASS |
| `BreadcrumbMessengerHub.Attachment` | 10 | 10 | 100.0000% | PASS |
| `BreadcrumbMessengerHub.CachedState` | 5 | 5 | 100.0000% | PASS |
| `BreadcrumbPopupPlacement` | 44 | 44 | 100.0000% | PASS |
| `BreadcrumbPopupPlacementResult` | 4 | 4 | 100.0000% | PASS |
| `BreadcrumbRenderProjection` | 85 | 85 | 100.0000% | PASS |
| `BreadcrumbCellRender` | 12 | 12 | 100.0000% | PASS |
| `BreadcrumbRowRender` | 20 | 20 | 100.0000% | PASS |
| `BreadcrumbSubfolderRender` | 6 | 6 | 100.0000% | PASS |
| `BreadcrumbSelectionSession` | 135 | 135 | 100.0000% | PASS |
| `BreadcrumbSelectorActivationMessage` | 11 | 11 | 100.0000% | PASS |
| `BreadcrumbSelectorKeyMessage` | 5 | 5 | 100.0000% | PASS |
| `BreadcrumbSelectorMessageSerializer` | 85 | 85 | 100.0000% | PASS |
| `BreadcrumbSelectorToggleMessage` | 1 | 1 | 100.0000% | PASS |
| `BreadcrumbSelectorViewMessage` | 19 | 19 | 100.0000% | PASS |
| `BreadcrumbStateModel` | 88 | 88 | 100.0000% | PASS |
| `BreadcrumbStateRow` | 135 | 135 | 100.0000% | PASS |
| `FolderBreadcrumbBridgeRouter` | 277 | 282 | 98.2270% | PASS |

Every measurable dedicated selector type is at least 98.2270% covered. The broad pre-existing `QfcItemController` is 111/154 = 72.0779%; it is not a new selector type, its non-excluded issue #400 hunk is 13/13 = 100.0000%, and its controller integration seams pass in `QfcItemControllerBreadcrumbDropDownTests` and `BreadcrumbDropDownIntegrationTests`.

## Changed and new source members

The AST-to-Cobertura source-member audit resolved 112 changed/new methods, constructors, accessors, and expression-bodied properties with numeric executable-line data.

| Measurable member result | Count | Minimum line coverage |
|---|---:|---:|
| At least 90% | 112 | 100.0000% |
| Below 90% | 0 | Not applicable |

All 112 measurable changed/new source members are 100.0000% covered. There is therefore no below-threshold method list.

## Bounded nonnumeric coverage accounting

The orchestrator authorized the following exact nonnumeric surfaces because they are direct third-party/live-UI adapters or issue #400 integration code within pre-existing exclusions. No numeric rate is asserted for them:

- New method-level adapters `BreadcrumbDropDownHost.CreateProductionSurfaceAsync` and `BreadcrumbDropDownHost.ShowOwnedPopup`. Their host-neutral lifecycle, error, placement, ownership, and focus behavior is exercised through injected seams. No new class-level exclusion was added.
- Issue #400 methods and nested `BreadcrumbResourceOwner` in the `ItemViewer` partial. The whole `ItemViewer` type was already marked `[ExcludeFromCodeCoverage]` at baseline in `QuickFiler/Viewers/ItemViewer.cs`; this change did not add or widen that attribute.
- Issue #400 code within the pre-existing method-level Qfc exclusions `InitializeWebViewAsync` and `EnsureBreadcrumbPipeline`. `git show df5ad49c909f6b739edef45d0336151f44e827a6:QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` confirms both exact attributes existed at baseline, and the current diff adds zero Qfc exclusion attributes. No other Qfc method or class is included in this treatment.
- Interfaces, enums, abstract members, and auto-properties without sequence points are non-executable declarations and do not receive invented numeric rates.

The direct integration seams pass in the 115-test issue #400 integrated suite and the complete final 5,830-test repository run. `coverage.config` has no diff from the baseline SHA and retains Git object hash `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`. Independent feature review must audit the bounded nonnumeric treatment.

Output Summary: PASS under the explicitly bounded coverage-accounting treatment above. Repository, changed-scope, measurable type, and measurable changed/new member thresholds pass; changed-line and Router coverage do not regress; no coverage configuration was weakened.
