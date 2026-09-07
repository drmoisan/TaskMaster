# Coverage-Exemption Audit ([P8-T10])

Timestamp: 2026-08-28T06-31

Command: `grep -c -F 'ExcludeFromCodeCoverage'` over the **seven** owned files — the four owned
production files and the three owned test files — compared against the seven-file `[P0-T17]` baseline.
EXIT_CODE: 0

## Per-file comparison

Seven files rather than four, because the criterion `[P9-T13]` flips says no new attribute is introduced
**anywhere** by this feature, and the three owned test files are part of this feature.

| # | Owned file | Baseline `[P0-T17]` | Final | Equal? |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 0 | **0** | yes |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 0 | **0** | yes |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 7 | **7** | yes |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 0 | **0** | yes |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 0 | **0** | yes |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 0 | **0** | yes |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (file did not exist) | **0** | yes |

**Every per-file count equals its baseline exactly. Total 7 before, 7 after.**

Equality in both directions is what the acceptance requires, and it is stronger than a one-sided check:
a count that only failed on an increase would miss a removal. **No attribute was added and none was
removed.** In particular the seven attributes in `BreadcrumbPopupUiOperations.cs` — at lines 105, 380,
383, 390, 394, 412, and 457 in the baseline — all survive the #475 deletion, which removed
`CaptureCurrentOrTests` and nothing else. The new test file was created carrying zero attributes and
still carries zero.

## All fixes in `ItemViewer.Breadcrumb.cs` are coverage-exempt

This statement is recorded here, in this artifact's own text, because `[P9-T13]` cites this artifact
alone and the criterion it flips opens on exactly this exemption claim. An artifact holding only
attribute counts could not carry it.

**All fixes in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — D1, D3, D4, D5, and #475 part 3 — are
coverage-exempt, because `QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on
the `ItemViewer` partial type, so their regression tests move no coverage number.**

The mechanism is that a type-level attribute on one part of a partial type applies to the whole type.
`ItemViewer.Breadcrumb.cs` declares `public partial class ItemViewer` and carries **zero**
`ExcludeFromCodeCoverage` occurrences of its own — row 1 above — so its exemption is inherited entirely
from `ItemViewer.cs`, which is a **forbidden** file this feature must not touch and whose attribute is
assumption D489-2. `[P7-T6]` re-confirmed that attribute is still present at line 20, and `[P7-T3]`
confirmed the file is byte-identical to `BASE_SHA`.

This is confirmed empirically rather than argued: `[P0-T15]` and `[P8-T7]` both record that
`ItemViewer.Breadcrumb.cs` matches **zero** `class` elements in the Cobertura document, before and
after the change. It contributes no covered line, no valid line, and no line-rate at all.

The consequence for review is that the five units landing in that file are required to carry regression
tests by the CLAUDE.md Bugfix Workflow and by the acceptance criteria, **not** by a coverage delta. Flat
coverage on those units is the expected and correct outcome and must not be read as a testing gap, nor
"fixed" by removing the exemption.

Only D2 (`BreadcrumbItemViewerLifecycleCoordinator.cs`), #475 part 1
(`BreadcrumbPopupUiOperations.cs`), and #475 part 2 (`BreadcrumbDropDownHost.cs`) are measured, and
`[P8-T7]` records their delivered rates as 0.909091, 0.991342, and 0.992883 respectively.

Output Summary: The `ExcludeFromCodeCoverage` count for each of the **seven** owned files equals its
`[P0-T17]` baseline exactly — 0, 0, **7**, 0, 0, 0, 0 — so **no attribute was added and none was
removed**. All fixes in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (D1, D3, D4, D5, and #475 part 3)
are **coverage-exempt** because `QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]`
on the partial type, so their regression tests move no coverage number.
