# Final File-Size Audit, After the Formatting Pass ([P8-T8])

Timestamp: 2026-08-28T06-29

Command: `grep -c ''` over the seven owned files and
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`, plus
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`
EXIT_CODE: 0

## Counting method

Line counts were taken with **`grep -c ''`**, which counts the file's lines including a final line that
is not newline-terminated. A word-count line switch reports a different — one lower — number for a file
without a trailing newline, which is why this task requires counting the file's lines rather than using
that switch. These are post-format figures: `[P8-T1]` ran the mutating format pass before this audit,
and `[P8-T2]` confirmed the repository reports zero unformatted files.

## The eight rows

| # | File | Baseline | Final | Limit | Result |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | **425** | at most 500 | pass |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **497** | at most 500 | pass |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **489** | at most 500 | pass |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | **463** | at most 500 | pass |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | **419** | at most 500 | pass |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | **483** | at most 500 | pass |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | **480** | at most 500 (and at most 480 by capacity rule 3) | pass |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | **500** | exactly 500 | pass |

**Every recorded value is at most 500.** The largest is 497.

## The two constrained files, named explicitly with their delivered values

Constraint C2 names two files as constrained, and this task requires each to be named explicitly with
its delivered value:

- **`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` — delivered value 497.** Against a
  481-line baseline that is a **+16** delta, within its **19 lines of headroom**, and three lines below
  the ceiling. This was the highest-likelihood scope risk in the change-set; no excess had to be removed
  and the ceiling was not waived.
- **`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` — delivered value 489.** Against a 494-line
  baseline that is a **−5** delta. The file received **no addition of any kind**, per capacity rule 2;
  its only change is the deletion of the `CaptureCurrentOrTests` declaration and the blank line
  separating it from the following member, and `git diff --numstat` reports `0 5` for it.

## `BreadcrumbDropDownIntegrationTests.cs` — both conjuncts

The criterion `[P9-T4]` flips says this file "remains at exactly 500 lines **and is unmodified**". A line
count establishes only the first conjunct, since a file can be edited without changing its line count.
Both are recorded here:

1. **Exactly 500 lines.** `grep -c ''` reports **500**.
2. **Unmodified.** `git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`
   produces **no output lines**, establishing byte-identity with its state at `BASE_SHA`.

The file sits at the ceiling with zero headroom and is a forbidden file under constraint C1, which is
why the disposal `[P1-T5]` added type-tests the concrete `BreadcrumbDropDownHost` rather than the
interface: the alternative would have broken this file's `Times.Once()` assertion and forced an edit
here.

## Note on the new test file

`ItemViewerBreadcrumbLifecycleRegressionTests.cs` sits at **480**, at the working cap capacity rule 3
sets twenty lines below the 500-line ceiling so CSharpier reflow cannot push it over. It first measured
521 lines and was compacted by shortening doc comments, not by dropping a method and not by creating a
second test file — a second file would have required a second `Compile Include` line and broken the
one-added-line `.csproj` diff criterion. `[P8-T1]`'s SHA comparison confirms the format pass left this
file byte-identical, so 480 is a settled post-format figure.

Output Summary: All eight values are at most 500 — **425, 497, 489, 463, 419, 483, 480**, and
**exactly 500** for `BreadcrumbDropDownIntegrationTests.cs`. The two constrained files are named with
their delivered values: `BreadcrumbItemViewerLifecycleCoordinator.cs` at **497** and
`BreadcrumbPopupUiOperations.cs` at **489**. `BreadcrumbDropDownIntegrationTests.cs` is exactly 500
lines **and** produces no output from `git diff --name-only <BASE_SHA>`, establishing byte-identity.
Counts were taken with `grep -c ''` rather than a word-count line switch.
