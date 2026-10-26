# Baseline Coverage (Issue #1007, remediation cycle 1, P0-T4)

Timestamp: 2026-10-25T23-53
Command: scripts/vscode/Invoke-MSTestWithCoverage.ps1 (TEST-COVERAGE, run in the item worktree against the P0-T3 rebuild)
EXIT_CODE: 0
Output Summary: Test Run Successful. Total 7756, passed 7756, failed 0. First-party coverage: lines 58411/67553 (86.47%), branches 14433/17860 (80.81%). BASELINE-FAILURES: none. Every N1-LINES body line has 0 hits.

## Counts (derived from the trx `ResultSummary` counters)

| Figure | Value |
|---|---|
| Total | 7756 |
| Executed | 7756 |
| Passed | 7756 |
| Failed | 0 |
| Not executed | 0 |

BASELINE-FAILURES: none. No test failed in this run.

## Repository coverage (one-line first-party summary)

`First-party coverage: lines 58411/67553 (86.47%), branches 14433/17860 (80.81%)`

- Line: 86.47 percent (floor 80).
- Branch: 80.81 percent (floor 75).

## Per-file coverage (post-processed Cobertura, de-duplicated per file and line number)

| File | Line | Branch |
|---|---|---|
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | 122/133 (91.73%) | 42/52 (80.77%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 143/152 (94.08%) | 60/68 (88.24%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 131/134 (97.76%) | 40/42 (95.24%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 243/249 (97.59%) | 69/86 (80.23%) |

## N1-LINES hits (instrumented lines only)

The executor located each N1-LINES block by reading the member body in the current source. A condition line is the `if` statement; the body lines are those inside its block.

| Member | Instrumented lines in the block (condition first) | Hits |
|---|---|---|
| `BreadcrumbBridgeRouter.HandleLeafToggleAsync`, the `row.IsCollapsed && !row.IsAnchoredCollapse` block | condition; body lines 1 to 6 | 1; 0, 0, 0, 0, 0, 0 |
| `FolderBreadcrumbBridgeRouter.ToggleAsync`, the `CollapsedAfterIndex != ActiveSegmentIndex` block | condition (4 lines); body lines 1 to 3 | 1, 1, 1, 1; 0, 0, 0 |
| `BreadcrumbStateModel.Expand`, the matching block | condition (4 lines); body lines 1 to 3 | 1, 1, 1, 1; 0, 0, 0 |
| `BreadcrumbBridgeCoordinator.PublishRouterOutputs`, `UnhandledArrowMessage` arm | condition; body lines 1 to 3 | 1; 0, 0, 0 |
| `BreadcrumbBridgeCoordinator.RaiseSyntheticArrowKey`, `UnhandledArrowMessage` arm | condition; body lines 1 to 3 | 1; 0, 0, 0 |

The Cobertura copy for P5-T5 is kept in SCRATCH as `issue-1007-r1-baseline-cobertura.xml` and is not committed.
