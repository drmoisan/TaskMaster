# P4-T1 — Unmodified-Suite Diff Gate (spec AC-5 / AC-6)

Timestamp: 2026-08-28T16-05

`BASELINE_COMMIT` read from the P0-T2 artifact and substituted literally below:
`c2d683d51d907d5591e313a550099fc267c10da6`.

## Command 1 — baseline commit against the working tree

```
git diff c2d683d51d907d5591e313a550099fc267c10da6 -- QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs
```

A single ref operand is used deliberately — no `..HEAD`. This compares the baseline commit against
the **working tree**, which is the state Phase 4 must judge; a `<BASELINE_COMMIT>..HEAD` form would
be vacuously empty because no commit exists before Phase 7.

EXIT_CODE: 0

Result: **empty output** (0 bytes).

## Command 2 — working-tree status over the same nine paths

```
git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs
```

EXIT_CODE: 0

Result: **empty output** (0 bytes).

## Output Summary

Both commands produced empty output. None of the nine pinned files differs from
`c2d683d51d907d5591e313a550099fc267c10da6` or carries any working-tree status. The nine are the five
spec AC-5 suites, all three `BreadcrumbDropDownOpenCoordinatorTests` parts, and the 499-line
`BreadcrumbDropDownHostTests.cs` primary file (DR-5 forbids editing it; the #680 host tests went to
its `.Part2.cs` partial instead).

The two mechanisms are complementary: the anchored diff is blind to untracked files, and porcelain
status goes empty once a change is committed. Together they establish that these nine paths are
byte-identical to baseline in the current working tree.

Acceptance: satisfied — both commands produced empty output.
