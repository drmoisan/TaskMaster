# File Sizes After #475 ([P6-T10])

Timestamp: 2026-08-28T06-10

Command: `wc -l` over all four owned production files and all three owned test files.
EXIT_CODE: 0

## All seven owned files

| # | File | Baseline | Now | Delta | Limit | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | **425** | +106 | at most 500 | pass |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **497** | +16 | at most 500 | pass |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **489** | **−5** | at most 500, strictly below 494 | pass |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | **463** | 0 | at most 500 | pass |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | **419** | +37 | at most 500 | pass |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | **483** | +3 | at most 500 | pass |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | **480** | +480 | at most 480 | pass |

**Every production value is at most 500.**

## The three acceptance conditions

1. **Every production value is at most 500.** 425, 497, 489, and 463 — the largest is 497, three lines
   below the ceiling.
2. **`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is strictly below 494.** It is **489**, five
   lines below its 494-line baseline. The change is a pure deletion: `git diff --numstat` reports
   `0 5` for this file, zero added and five deleted, which is the `CaptureCurrentOrTests` declaration
   plus the blank line separating it from the following member. Capacity rule 2 — that this file
   receives **no addition of any kind** — is satisfied exactly.
3. **The new test file is at most 480.** It is **480**, at the cap.

## The new test file required compaction

The eight test methods plus their helpers first measured **521** lines, over both the 480-line working
cap and the 500-line ceiling. Capacity rule 3 was applied: the doc comments were compacted rather than
a method being dropped, and **no second test file was created**, which is what keeps the one-added-line
`.csproj` diff criterion reachable. The fallback of folding a second assertion into an existing test
was not needed.

Every clause the acceptance criteria require of those comments survived the compaction and was
re-verified afterwards: both `[P4-T1]` methods still carry the sentence "proves the guard fires and does
not prove the race is absent" and the statement that a true two-thread data race cannot be reproduced
deterministically under the repository ban on sleeps and wall-clock waits; the D1 method still records
that its second and third assertions are corroborating rather than discriminating; and the #475 part 3
method still carries decision D-10's mandatory "that has run its constructor" qualifier.

The file was formatted with `dotnet tool run csharpier format` against that single path during
compaction, so **480 is a post-format figure**, not a provisional hand count. `[P8-T1]` re-runs the
format pass over all seven owned paths and records a SHA-256 comparison for each.

## The two constrained files

Both are delivered and neither is edited again by any later phase:

- `BreadcrumbItemViewerLifecycleCoordinator.cs` — **497**, a +16 delta within its 19 lines of headroom.
- `BreadcrumbPopupUiOperations.cs` — **489**, five lines *below* its baseline, with zero additions.

Output Summary: All seven owned files are within their limits. Production values are **425, 497, 489,
463**, all at most 500. `BreadcrumbPopupUiOperations.cs` is **489**, strictly below its 494-line
baseline, by a pure five-line deletion with zero additions. The new test file is **480**, at the cap,
after a capacity-rule-3 comment compaction from 521 that preserved every acceptance-required clause.
