# P5-T186 — Fixed two-batch allocation for the nine below-threshold units

Timestamp: 2026-07-22T17-01Z

Command: `grep -c "\[TestMethod\]" QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`

EXIT_CODE: 0

## Allocation (each of the nine P5-T185 units appears exactly once)

### Batch N1 — target file `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (exactly one existing test file)

| Unit | Uncovered lines |
|---|---|
| `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 99 |
| `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 118 |
| `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 122 |
| `BreadcrumbDropDownOpenCoordinator.Reset()` | 133 |
| `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 224, 225, 226 |

### Batch N2 — target file `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (exactly one existing test file)

| Unit | Uncovered lines |
|---|---|
| `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` | 292-301, 310-313, 315 |
| `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` | 324 |
| `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 153-156 |
| `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 413 |

Allocation completeness: 5 + 4 = 9 units. No unit is unallocated and no unit is duplicated.

## Zero production C# files in both batches

Every listed uncovered line is a reachable failure, cancellation, rollback, retention, or late-callback branch of
already-correct production code that lacks a deterministic test, not a production defect:

- Coordinator 99, 118, 122, 133 are released-generation guard returns of already-correct invalidation logic.
- Coordinator 224-226 is the rollback's own secondary-failure containment.
- Lifetime 292-301 is the stale-lease hosted-surface disposal path; 324 is its stale-lease early return.
- Lifetime 310-313 and 315 are the post-failure cleanup containment that preserves the primary exception.
- Lifetime 153-156 is the open failure-recovery containment inside `CompleteOpenAsync`.
- Host 413 is the late-callback guard inside the scheduled `OnDropDownClosed` body.

A batch that proposes a production edit stops for atomic replanning.

## Placement rationale for the `BreadcrumbDropDownHost` lambda

`BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` is deliberately placed in the PopupBoundary partial rather than
the topically closer `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` solely because that file is
468 physical lines and therefore has approximately 12 lines of headroom to the 480-line bound and cannot accept new
cases.

Both target files are `[TestClass] partial` continuations of classes already inside the 17-class filter
(`BreadcrumbDropDownOpenCoordinatorTests` and `BreadcrumbPopupBoundaryCoverageTests`), so the class inventory stays at
exactly 17 and no `QuickFiler.Test.csproj` include changes.

## Case arithmetic governing P5-T201

- Current instrumented composition: 160 = `70+13+12+5+10+18+12+10+10`.
- Measured pre-correction case counts in the two target classes: `BreadcrumbDropDownOpenCoordinatorTests` = 5 + 5 = **10**;
  `BreadcrumbPopupBoundaryCoverageTests` = 5 + 13 = **18**.
- Batch N1 adds exactly five non-data-row cases, raising `BreadcrumbDropDownOpenCoordinatorTests` from 10 to **15**.
- Batch N2 adds exactly five non-data-row cases, raising `BreadcrumbPopupBoundaryCoverageTests` from 18 to **23**.
- Required post-correction composition: `70+13+12+5+15+23+12+10+10` = **170**.

## Output Summary

Read-only allocation ledger; no file was modified. The nine P5-T185 units are partitioned into exactly two
test-only batches, five units to batch N1 and four to batch N2, each unit allocated exactly once and none
duplicated or unallocated. Both batches change zero production C# files. Each batch edits exactly one existing
test file that is a partial continuation of a class already inside the 17-class filter, so the class inventory
remains 17 and no project include is added. The measured pre-correction case counts (10 and 18) confirm the
required post-correction composition of `70+13+12+5+15+23+12+10+10` = 170 for P5-T201.
