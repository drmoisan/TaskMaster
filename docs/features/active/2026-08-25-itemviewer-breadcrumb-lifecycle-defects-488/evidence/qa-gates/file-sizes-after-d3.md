# File Sizes After D3 ([P3-T7])

Timestamp: 2026-08-28T05-41

Command: `wc -l QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`
EXIT_CODE: 0

## Line counts

| File | Baseline | After D1 | Now (after D3) | Limit | Result |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 331 | **357** | at most 500 | pass |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | 188 | **257** | at most 480 | pass |

The production file is **357** lines, which is at most 500. The test file is **257** lines, which is at
most 480.

## Remaining headroom against the constraint C2 budget

`ItemViewer.Breadcrumb.cs` has **143 lines** of headroom to the 500-line ceiling. Constraint C2 still
budgets +14 for D4, +4 for D5, and +3 for #475 part 3 in this file — 21 lines of planned growth against
143 available. The delivered totals run ahead of the C2 per-unit figures because each fix carries an
explanatory comment block, but the aggregate remains far inside the ceiling.

The new test file has **223 lines** of headroom to its 480-line working cap, which is itself twenty
lines below the 500-line ceiling so that CSharpier reflow cannot push it over. Five of the eight
planned test methods remain to be added: two for D4, one for D5, and two for #475.

Neither of the two files constraint C2 names as constrained is touched by D3.
`BreadcrumbItemViewerLifecycleCoordinator.cs` stands at 497 and `BreadcrumbPopupUiOperations.cs` at its
untouched 494; `[P2-T7]` recorded the former and `[P6-T10]` records the latter after the only edit it
receives.

Output Summary: `ItemViewer.Breadcrumb.cs` is **357** lines (at most 500, pass) with 143 lines of
headroom; `ItemViewerBreadcrumbLifecycleRegressionTests.cs` is **257** lines (at most 480, pass) with
223 lines of headroom.
