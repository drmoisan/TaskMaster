# Phase 0 — Capacity Budget ([P0-T18])

Timestamp: 2026-08-28T05-21

Command: derived from the measured headroom recorded in `[P0-T16]` and the planned per-file delta
stated in constraint C2 of the plan of record. No command was run for this task beyond reading those
two inputs.
EXIT_CODE: 0

## Budget table

| File | Lines now (`[P0-T16]`) | Headroom to 500 | Planned delta (C2) | Projected |
| --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 181 | +33 (D1 +4, D3 +8, D4 +14, D5 +4, #475 +3) | ~352 |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **19** | +6 (D2 only) | **~487** |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **6** | −4 or more (#475 deletion only) | **~490** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | 37 | 0 (two identifier swaps) | 463 |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | 118 | +30 (D2 test, `ThemesApplied` recorder) | ~412 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 20 | −26 then +22 (replacement test) | ~476 |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | 500 | +8 test methods and its own helpers | at most 480 |

## Aggregate planned addition

Summing the planned deltas across all seven owned files: `+33 +6 −4 +0 +30 −4 (net of −26 then +22)`
for the six existing files gives **+61 lines**, and the new test file adds **at most 480 lines**. The
aggregate planned addition is therefore **at most 541 lines** across the seven owned files, of which
at most 480 land in a file that starts empty.

## The two constrained files, stated explicitly

Constraint C2 names two files as constrained, and both are called out here with their projections:

- **`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`** — 481 lines now, **19 lines of
  headroom**, planned delta +6 for D2 only, **projected ~487**. This is the highest-likelihood scope
  risk in the whole change-set.
- **`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`** — 494 lines now, **6 lines of headroom**,
  planned delta −4 or more from the #475 deletion only, **projected ~490**. It receives no addition at
  all, so its projection can only fall.

## The four capacity rules of constraint C2, stated in substance

1. **The two constrained files admit only their one named edit.**
   `BreadcrumbItemViewerLifecycleCoordinator.cs` and `BreadcrumbPopupUiOperations.cs` are the two
   constrained files, and no edit other than the one named for each in the budget table above may
   target either. If D2's delivered diff exceeds its nineteen lines of headroom after formatting, the
   excess is removed rather than the 500-line ceiling waived. Because CSharpier reflows argument
   lists, a hand count taken before the format stage is not authoritative: the measurement that
   governs is taken **after** formatting.
2. **`BreadcrumbPopupUiOperations.cs` receives no addition of any kind.** Its only change is the
   deletion of the `CaptureCurrentOrTests` declaration and the blank line separating it from the
   preceding member. If a #475 test helper needs a home, it belongs in a test file, never here.
3. **The new test file is projected at or under 480 lines**, not 500, so that CSharpier reflow cannot
   push it over the ceiling. If the eight new methods plus their helpers would exceed 480, the
   remedy is to compact by folding a second assertion into an existing new test rather than adding a
   near-duplicate method. **No new test file beyond the single one named in constraint C1 may be
   created**, because a second file would require a second `Compile Include` line and would break the
   one-added-line `.csproj` diff criterion that `[P7-T7]` measures and `[P7-T13]` flips.
4. **If no allocation fits, execution stops rather than degrading.** The executor writes a blocker
   artifact to
   `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/other/capacity-blocker.md`
   and reports. It must not edit a forbidden file, must not create an extra file, and must not leave
   any file over 500 lines.

## Related ceiling that is not part of this budget

`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` sits at exactly **500** lines, at the
ceiling, with zero headroom. It is a **forbidden** file under constraint C1: it must be byte-identical
at delivery, so it carries no planned delta and no budget line. `[P8-T8]` verifies both that it is
still exactly 500 lines and, separately, that
`git diff --name-only <BASE_SHA> -- <that file>` produces no output, since a line count alone does not
establish byte-identity.

Output Summary: Aggregate planned addition is at most **+61 lines** across the six existing owned
files plus at most **480** lines in the new test file. The two constrained files are
`BreadcrumbItemViewerLifecycleCoordinator.cs`, projected **~487** against a 481-line baseline and 19
lines of headroom, and `BreadcrumbPopupUiOperations.cs`, projected **~490** against a 494-line
baseline and 6 lines of headroom with no addition permitted. All four capacity rules are recorded
above.
