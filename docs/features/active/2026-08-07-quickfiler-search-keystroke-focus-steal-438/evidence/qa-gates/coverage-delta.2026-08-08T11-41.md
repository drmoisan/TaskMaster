# [P6-T6] Coverage Delta and Threshold Verification

- **Issue:** #438
- **Task:** [P6-T6]
- **Timestamp:** 2026-08-08T11-41
- **Inputs:** `evidence/baseline/coverage-baseline.cobertura.xml` (HEAD `904b4c38`) vs `evidence/qa-gates/coverage-final.cobertura.xml`

## Command

Both Cobertura reports were parsed with the same method — for each `<method>`, count its `<line>` children and the subset with `hits > 0` — so baseline and post-change figures are computed identically and are directly comparable.

- **EXIT_CODE:** 0

## (a) Every measurable new or changed member reaches >= 90% line coverage

| Member | File | Covered / total | Line coverage | Gate |
|---|---|---:|---:|---|
| `BreadcrumbSelectionSession.HighlightRow` | `BreadcrumbSelectionSession.Highlight.cs` | 11 / 11 | **100%** | PASS |
| `FolderBreadcrumbBridgeRouter.ReplaceItemsPreservingSession` | `FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | 8 / 8 | **100%** | PASS |
| `FolderBreadcrumbBridgeRouter.HighlightRow` (router pass-through, D13) | `FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | 1 / 1 | **100%** | PASS |
| `FolderBreadcrumbBridgeRouter.BuildPlainRows` | `FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | 15 / 15 | **100%** | PASS |
| `BreadcrumbBridgeCoordinator.PresentSearchResults` | `BreadcrumbBridgeCoordinator.Search.cs` | 18 / 18 | **100%** | PASS |
| `BreadcrumbBridgeCoordinator.PublishSearchPresentation` | `BreadcrumbBridgeCoordinator.Search.cs` | 3 / 3 | **100%** | PASS |
| `BreadcrumbDropDownHost.OpenAsync` (3-parameter delegation) | `BreadcrumbDropDownHost.Open.cs` | 1 / 1 | **100%** | PASS |
| `BreadcrumbDropDownHost.IBreadcrumbDropDownHost.OpenAsync` (4-parameter, D11) | `BreadcrumbDropDownHost.Open.cs` | 1 / 1 | **100%** | PASS |
| `BreadcrumbDropDownHost.OpenWithFocusIntentAsync` (relocated body + guard) | `BreadcrumbDropDownHost.Open.cs` | 12 / 12 | **100%** | PASS |
| `BreadcrumbDropDownOpenLifetime.FocusCurrentSurface` | `BreadcrumbDropDownOpenLifetime.Focus.cs` | 8 / 8 | **100%** | PASS |
| `BreadcrumbDropDownOpenCoordinator.LatchNextOpenTakesNoFocus` | `BreadcrumbDropDownOpenCoordinator.cs` | 8 / 8 | **100%** | PASS |
| `BreadcrumbDropDownOpenCoordinator.NextOpenTakesNoFocus` (getter) | `BreadcrumbDropDownOpenCoordinator.cs` | 4 / 4 | **100%** | PASS |
| `BreadcrumbDropDownOpenCoordinator.BeginOpenCore` (changed) | `BreadcrumbDropDownOpenCoordinator.cs` | 20 / 21 | **95.24%** | PASS |
| `BreadcrumbItemViewerLifecycleCoordinator.PresentSearchResults` | `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` | 5 / 5 | **100%** | PASS |

**Minimum across all measurable new/changed members: 95.24%. Gate is >= 90%. PASS.**

`ItemViewer.PresentFolderSearchResults` and `ItemViewer.PresentBreadcrumbSearchResults` are thin forwarding members on the `[ExcludeFromCodeCoverage]` `ItemViewer` partial and are therefore not measurable, per the ratified exemption recorded in plan D6 and `ItemViewer.FolderSearch.cs:9-17`. Their behavior is exercised end-to-end through the `BreadcrumbDropDownSearchIntegrationTests` harness.

### Remediation performed by this task

The first measurement put `BreadcrumbBridgeCoordinator.PresentSearchResults` at **16/18 = 88.89%**, below the gate. The two uncovered lines were the disposed-pipeline early return:

```csharp
if (!_upgradeLifetime.Invalidate())
{
    return;          // <- lines 56-57, uncovered
}
```

Two tests were added to `BreadcrumbDropDownSearchIntegrationTests.Part2.cs` —
`PresentSearchResults_AfterDisposal_IsADeterministicNoOp` and
`PresentSearchResults_NullItems_ThrowsArgumentNullException` — and the QA loop was restarted from P6-T1. The member now measures **18/18 = 100%**. No assertion was weakened to reach the threshold; genuine error-handling scenarios were added.

## (b) Changed-line coverage does not regress

The only new/changed member with an uncovered line is `BeginOpenCore`. Its per-line hit map:

```
214:1  218:1  219:1  220:1  221:0  222:1  223:1  226:1  227:1
230:1  231:1  232:1  238:1  239:1  240:1  ...
```

Line **221** is `return ClosedTask;` — the stale-generation early return inside the `_sync` lock. That line is **pre-existing and was already uncovered at baseline**:

| | Baseline | Post-change |
|---|---:|---:|
| `BeginOpenCore` line coverage | 15 / 16 = **93.75%** | 20 / 21 = **95.24%** |

Every line **added** by this change inside `BeginOpenCore` is covered: `226` (`takeFocus = !_nextOpenTakesNoFocus;`), `227` (`_nextOpenTakesNoFocus = false;`), and `238`–`240` (the overload dispatch). Changed-line coverage therefore **improved**; no changed line regressed. **PASS.**

## (c) Post-change repository-wide figure >= captured baseline

| Metric | Baseline | Post-change | Delta | Gate |
|---|---:|---:|---:|---|
| `line-rate` | 0.858261 | **0.858665** | +0.000404 | PASS (not lower) |
| `branch-rate` | 0.792082 | **0.792502** | +0.000420 | PASS (not lower) |

Per-package, for the two packages this change touches:

| Package | Baseline line | Post-change line | Baseline branch | Post-change branch | Gate |
|---|---:|---:|---:|---:|---|
| QuickFiler | 0.8081586615283392 | **0.8091631603553062** | 0.7465236392530791 | **0.7479355092410539** | PASS |
| UtilitiesCS | 0.895326282732185 | **0.8957251943617782** | 0.8338995500872279 | **0.8342353912485093** | PASS |

The seven untouched packages are bit-identical between the two reports. **PASS.**

## (d) Raw repository-wide figure vs the 80% floor — reported non-blocking

Raw repository-wide line coverage is **85.87%**, which is above the CLAUDE.md § UT2 repository floor of 80% and above the `.claude/rules/general-unit-test.md` uniform floor of 85%. Branch coverage is **79.25%**, above the 75% uniform branch floor.

This figure is the raw, untested-denominator number: it includes vendored `SVGControl` (47.30%) and the COM/VSTO-bound `ToDoModel` (57.31%) and `TaskMaster` (70.97%) assemblies, which carry pre-existing debt outside this change. Per plan D6 and the ratified CLAUDE.md § UT2 testable-denominator exemption (and the #424 precedent), that raw figure is reported **non-blocking**; the binding gates are (a), (b), and (c) above, all of which pass.

## Summary of binding gates

| Gate | Requirement | Result | Status |
|---|---|---|---|
| (a) | every measurable new/changed member >= 90% line coverage | minimum 95.24% | **PASS** |
| (b) | changed-line coverage does not regress | `BeginOpenCore` 93.75% -> 95.24%; every added line covered | **PASS** |
| (c) | post-change repo-wide figure >= baseline | line 0.858261 -> 0.858665; branch 0.792082 -> 0.792502 | **PASS** |
| (d) | raw repo-wide vs 80% floor | 85.87% line / 79.25% branch, above both floors | **PASS (non-blocking)** |

No value is unavailable or a placeholder; every figure above is numeric and derived from the two committed Cobertura artifacts.

## Result

- **Output Summary:** All four coverage gates pass with numeric evidence. Every measurable new or changed member reaches at least 95.24% line coverage (twelve of fourteen at 100%); changed-line coverage improved rather than regressed, with the single uncovered line in `BeginOpenCore` proven pre-existing at baseline; repository-wide line coverage rose from 0.858261 to 0.858665 and branch coverage from 0.792082 to 0.792502; and the raw repository figure of 85.87% line / 79.25% branch clears both the 80% CLAUDE.md floor and the 85%/75% uniform floors. Accept criteria met.
