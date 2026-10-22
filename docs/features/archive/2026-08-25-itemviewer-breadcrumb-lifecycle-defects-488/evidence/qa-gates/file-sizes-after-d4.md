# File Sizes After D4 ([P4-T9])

Timestamp: 2026-08-28T05-49

Command: `wc -l QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`
EXIT_CODE: 0

## Line counts

| File | Baseline | After D1 | After D3 | Now (after D4) | Limit | Result |
| --- | --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 331 | 357 | **398** | at most 500 | pass |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | 188 | 257 | **363** | at most 480 | pass |

The production value is **398**, which is at most 500. The test value is **363**, which is at most 480.

## Remaining headroom

`ItemViewer.Breadcrumb.cs` has **102 lines** of headroom to the 500-line ceiling. D4 added 41 lines
against constraint C2's planned +14: the helper carries a twelve-line XML doc comment recording why the
comparison is reference equality rather than thread identity and why the null escape exists, plus four
guard call sites with their separating blank lines. Two units remain budgeted for this file, +4 for D5
and +3 for #475 part 3, against 102 lines available.

The new test file has **117 lines** of headroom to its 480-line working cap. Three of the eight planned
test methods remain: one for D5 and two for #475. The four already present average roughly 45 lines
each including their doc comments, so three more at that rate would land near 498 — over the cap.

**The remaining three methods will therefore carry shorter doc comments than the first four.** Capacity
rule 3 permits exactly this kind of compaction and requires that no second test file be created,
because a second file would need a second `Compile Include` line and would break the one-added-line
`.csproj` diff criterion. If the three cannot be fitted under 480 by trimming commentary, the fallback
rule 3 prescribes is to fold a second assertion into an existing new test rather than add a
near-duplicate method.

Neither of the two files constraint C2 names as constrained is touched by D4.
`BreadcrumbItemViewerLifecycleCoordinator.cs` stands at 497 and `BreadcrumbPopupUiOperations.cs` at its
still-untouched 494.

Output Summary: `ItemViewer.Breadcrumb.cs` is **398** lines (at most 500, pass) with 102 lines of
headroom; `ItemViewerBreadcrumbLifecycleRegressionTests.cs` is **363** lines (at most 480, pass) with
117 lines of headroom for the three remaining test methods.
