# [P6-T5] Final QA — Full Coverage-Enabled Test Run

- **Issue:** #438
- **Task:** [P6-T5]
- **Timestamp:** 2026-08-08T11-41

## Command

`pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-final.cobertura.xml' ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

## Loop iterations

The QA loop restarted twice, as the plan requires whenever a step changes files:

1. **Iteration 1** — P6-T2 found `BreadcrumbDropDownSearchIntegrationTests.cs` at 502 lines; a partial was extracted and the loop restarted from P6-T1. First full run: 6346/6346 passed, EXIT_CODE 0.
2. **Iteration 2** — P6-T6 found `BreadcrumbBridgeCoordinator.PresentSearchResults` at 88.89% line coverage (below the 90% new-member gate). Two tests were added for the disposed-pipeline guard and the null-argument guard, and the loop restarted from P6-T1.

This artifact records the **final** iteration, which is the authoritative result.

## Test result (final iteration)

```
Discovered 9 test assemblies.
Total tests: 6348
     Passed: 6348
     Failed: 0
 Total time: 40.7638 Seconds
```

`Test Run Successful.` **Zero failures across all first-party `*.Test.dll` assemblies**, in a single attempt with no flake and no retry.

Test count grew from the 6293 baseline to 6348 — **+55 tests** added by this change.

## Assembly discovery — no `\.claude\` path collected

`grep -c "\.claude"` over the run log returned **0**. Nine first-party test assemblies were discovered, matching the baseline enumeration.

## Coverage — Cobertura root `<coverage>` element

| Metric | Baseline (P0-T7) | Post-change (P6-T5) | Delta |
|---|---|---|---|
| `line-rate` | 0.858261 | **0.858665** | **+0.000404** |
| `branch-rate` | 0.792082 | **0.792502** | **+0.000420** |
| `lines-covered` | 95285 | 95487 | +202 |
| `lines-valid` | 111021 | 111204 | +183 |
| `branches-covered` | 22069 | 22133 | +64 |
| `branches-valid` | 27862 | 27928 | +66 |

Both repository-wide rates **increased**; the post-change figure is not lower than the captured baseline (spec AC-12).

### Per-package (packages touched by #438 in bold)

| Package | Baseline line | Post-change line | Baseline branch | Post-change branch |
|---|---|---|---|---|
| **QuickFiler** | 0.8081586615283392 | **0.8091631603553062** | 0.7465236392530791 | **0.7479355092410539** |
| **UtilitiesCS** | 0.895326282732185 | **0.8957251943617782** | 0.8338995500872279 | **0.8342353912485093** |
| TaskVisualization | 0.8984326018808777 | 0.8984326018808777 | 0.8325 | 0.8325 |
| SVGControl | 0.47303128371089537 | 0.47303128371089537 | 0.4702194357366771 | 0.4702194357366771 |
| ToDoModel | 0.5731056563500534 | 0.5731056563500534 | 0.4881889763779528 | 0.4881889763779528 |
| Tags | 0.9268929503916449 | 0.9268929503916449 | 0.9157894736842105 | 0.9157894736842105 |
| TaskMaster | 0.7097004279600571 | 0.7097004279600571 | 0.6518151815181518 | 0.6518151815181518 |
| TaskTree | 0.9548387096774194 | 0.9548387096774194 | 0.9215686274509803 | 0.9215686274509803 |
| VBFunctions | 1 | 1 | 1 | 1 |

Both touched packages improved on line and branch rate. The seven untouched packages are bit-identical to baseline, confirming the change is confined to `QuickFiler` and `UtilitiesCS`.

Artifact: `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml`.

## New test classes executed (execution evidence, not discovery-only)

Sampled one method per new suite from the full-run log; each executed exactly once:

| Method | Suite |
|---|---|
| `HighlightRow_OpenSession_ChangesOnlyPendingIdentity` | `BreadcrumbSelectionSessionHighlightTests` |
| `ReplaceItemsPreservingSession_ReportsRenderRequiredOnly` | `FolderBreadcrumbBridgeRouterReplaceItemsTests` |
| `TextBoxSearch_TextChanged_IssuesThePresentationIntentExactlyOnce` | `QfcItemController_SearchFocusRegressionTests` |
| `SearchThenCancel_LeavesTheCachedFolderAtThePreSearchCommittedValue` | `QfcItemController_SearchFocusRegressionTests` |
| `PresentFolderSearchResults_OnAClosedSelector_OpensOnceWithoutFocus` | `BreadcrumbDropDownSearchIntegrationTests` |
| `EightCharacterQueryTypedThroughTheSeam_DeliversTheFullTextAndCompleteRowSet` | `BreadcrumbDropDownSearchIntegrationTests.Part2` |
| `OpenAsync_FreshOpenWithoutFocus_InvokesNeitherFocusDelegate` | `BreadcrumbDropDownHostTests.Part2` |
| `LatchedOpen_ReachesTheHostOnceWithoutFocus` | `BreadcrumbDropDownOpenCoordinatorTests.Part3` |

Every new test file is wired, discovered, and executing with count > 0.

## Pre-existing flakes: not observed

Neither `WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` nor any `QfcItemController_InitializationTests.*ThroughThePumpHost*` test failed in this run, consistent with the P0-T7 finding that they are CPU-saturation artifacts rather than defects.

## Result

- **Output Summary:** EXIT_CODE 0 with **6348 of 6348 tests passing** and zero failures across nine first-party test assemblies in 40.8 seconds; no `\.claude\` assembly was collected. Post-change repository-wide Cobertura `line-rate` = **0.858665** (baseline 0.858261) and `branch-rate` = **0.792502** (baseline 0.792082) — both increased. QuickFiler improved to line 0.8091631603553062 / branch 0.7479355092410539; UtilitiesCS to line 0.8957251943617782 / branch 0.8342353912485093. Accept criteria met.
