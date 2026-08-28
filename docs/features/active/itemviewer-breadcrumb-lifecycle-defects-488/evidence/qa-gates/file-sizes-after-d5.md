# File Sizes After D5 ([P5-T7])

Timestamp: 2026-08-28T05-55

Command: `wc -l QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`
EXIT_CODE: 0

## Line counts

| File | Baseline | After D1 | After D3 | After D4 | Now (after D5) | Limit | Result |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 331 | 357 | 398 | **412** | at most 500 | pass |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | 188 | 257 | 363 | **395** | at most 480 | pass |

The production value is **412**, which is at most 500. The test value is **395**, which is at most 480.

## Remaining headroom

`ItemViewer.Breadcrumb.cs` has **88 lines** of headroom. D5 added 14 lines against constraint C2's
planned +4: four lines for the guard itself and a nine-line comment block stating decision D-15's
statement order explicitly, which `[P5-T3]`'s acceptance requires be present in the delivered source
rather than left for a reviewer to infer. One unit remains budgeted for this file, #475 part 3 at +3,
against 88 lines available.

The new test file has **85 lines** of headroom for the two remaining #475 test methods. The D5 method
was written with a compacted six-line doc comment rather than the fifteen- to twenty-line comments the
D1 and D4 methods carry, following the compaction plan `[P4-T9]` recorded; it cost 32 lines against the
roughly 45 the earlier methods averaged. The two #475 methods will be written the same way, which
leaves adequate margin under the 480-line cap.

Capacity rule 3's fallback — folding a second assertion into an existing new test rather than adding a
near-duplicate method — has not been needed and is not expected to be. **No second test file has been
created**, which is what keeps the one-added-line `.csproj` diff criterion reachable.

Neither of the two files constraint C2 names as constrained is touched by D5.
`BreadcrumbItemViewerLifecycleCoordinator.cs` stands at 497 and `BreadcrumbPopupUiOperations.cs` at its
still-untouched 494; the latter receives its only edit, a deletion, in `[P6-T3]`.

Output Summary: `ItemViewer.Breadcrumb.cs` is **412** lines (at most 500, pass) with 88 lines of
headroom; `ItemViewerBreadcrumbLifecycleRegressionTests.cs` is **395** lines (at most 480, pass) with
85 lines of headroom for the two remaining #475 test methods.
