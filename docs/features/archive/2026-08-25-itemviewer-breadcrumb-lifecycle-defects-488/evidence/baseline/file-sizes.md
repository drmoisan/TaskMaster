# Phase 0 — Baseline File Sizes ([P0-T16])

Timestamp: 2026-08-28T05-20

Command: `wc -l` over the seven owned files plus
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`, run from the worktree root, together
with an existence probe for the new test file.
EXIT_CODE: 0

## The eight rows

| # | File | Expected | Observed | Result |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | **319** | match |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **481** | match |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **494** | match |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | **463** | match |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | **382** | match |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | **480** | match |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (does not yet exist) | **0** — `ls` reports `No such file or directory` | match |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | **500** | match |

## Discrepancies

**None.** All eight recorded values equal the values constraint C2 states. No deviation was observed
and nothing needs reporting before Phase 1 begins.

This agreement is worth stating explicitly rather than assuming, because the C2 table was measured at
commit `0a6aaa31` while this worktree's base is `12465043`. Every `file:line` citation in the spec,
plan, and research is anchored to the earlier commit and must be resolved by member name rather than
by line number, but the eight **file-level line counts** did not drift between the two commits: none
of the eight files was modified between them. That is what the table above establishes empirically.

## Headroom to the 500-line ceiling

| File | Lines now | Headroom |
| --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 181 |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **19** |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **6** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | 37 |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | 118 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 20 |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | 500, capped at 480 by capacity rule 3 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | **0** — at the ceiling, and a forbidden file |

The two constrained files are `BreadcrumbItemViewerLifecycleCoordinator.cs` (19 lines of headroom) and
`BreadcrumbPopupUiOperations.cs` (6 lines, and it receives no addition of any kind). The capacity
budget derived from these figures is recorded by `[P0-T18]`.

Output Summary: All eight baseline line counts match constraint C2 exactly — 319, 481, 494, 463, 382,
480, 0 (file absent), and 500. **Zero discrepancies.** The two constrained files have 19 and 6 lines
of headroom respectively.
