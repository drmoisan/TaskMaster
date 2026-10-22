# Phase 0 — Baseline Coverage of the Owned Production Files ([P0-T15])

Timestamp: 2026-08-28T05-20

Command: read from the `[P0-T14]` Cobertura file
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/coverage-baseline.cobertura.xml`,
aggregating by `filename` across every `class` element whose `filename` attribute matches the target,
and counting hit and total `line` elements rather than reading any single element's `line-rate`
attribute. No new coverage run was started for this task.
EXIT_CODE: 0

## Why aggregation by `filename` rather than a single element's attribute

Per decision D-13, async state machines and lambdas are emitted as separate `class` elements, so a
single `class` element's `line-rate` attribute describes only part of a file. The figures below are
computed as `covered / valid` over every `line` element belonging to every `class` element whose
`filename` names the target file. For these three files the Cobertura emitted by `dotnet-coverage`
happens to carry exactly one `class` element each, so the aggregation and the single-element reading
coincide here; the aggregation is used regardless, because the post-change run in `[P8-T7]` must be
computed the same way for the comparison to be valid.

## BASELINE AGGREGATED LINE RATES — the three measured owned production files

| Owned production file | Lines covered | Lines valid | **Baseline line-rate** |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 288 | 318 | **0.905660** |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 232 | 234 | **0.991453** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 279 | 281 | **0.992883** |

All three are already above the `>= 90%` new-or-changed-member floor. These three values are the
comparison basis for `[P8-T7]`.

## The fourth owned production file is ABSENT from the measured set

`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` matches **zero** `class` elements in the Cobertura
document. It contributes no `line` element, no covered line, and no valid line, so it has no
line-rate at all rather than a line-rate of zero.

The reason is a coverage exclusion this feature must neither rely on removing nor extend.
`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial
**type** declaration:

```
QuickFiler/Viewers/ItemViewer.cs:20:    [ExcludeFromCodeCoverage]
QuickFiler/Viewers/ItemViewer.cs:21:    public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal
```

A type-level attribute on one part of a partial type applies to the whole type, so **every member of
`ItemViewer.Breadcrumb.cs` is already excluded from coverage measurement**. The citation
`ItemViewer.cs:20` used by `spec.md` and by constraint C5 resolves exactly at this branch's base
commit; no line drift occurred for this particular citation.

## Consequences carried forward

- D1, D3, D4, D5, and #475 part 3 all land in `ItemViewer.Breadcrumb.cs` and therefore **move no
  coverage number**. Their regression tests are required by the CLAUDE.md Bugfix Workflow and by the
  acceptance criteria, not by a coverage delta. A reviewer must not read flat coverage on this feature
  as a testing gap.
- Only D2 (`BreadcrumbItemViewerLifecycleCoordinator.cs`), #475 part 1
  (`BreadcrumbPopupUiOperations.cs`), and #475 part 2 (`BreadcrumbDropDownHost.cs`) are measured.
- `ItemViewer.cs` is a forbidden file under constraint C1 and its attribute is assumption D489-2, so
  the exclusion must not be removed to "fix" the flat coverage. Constraint C5 additionally forbids
  introducing any new `[ExcludeFromCodeCoverage]` attribute anywhere in this feature and forbids
  removing any existing one; `[P0-T17]` records the baseline attribute counts that `[P8-T10]` compares
  against.

Output Summary: Baseline aggregated line-rates are **0.905660** for
`BreadcrumbItemViewerLifecycleCoordinator.cs`, **0.991453** for `BreadcrumbPopupUiOperations.cs`, and
**0.992883** for `BreadcrumbDropDownHost.cs`. `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is absent
from the measured set because `QuickFiler/Viewers/ItemViewer.cs:20` carries a type-level
`[ExcludeFromCodeCoverage]` on the `ItemViewer` partial type.
