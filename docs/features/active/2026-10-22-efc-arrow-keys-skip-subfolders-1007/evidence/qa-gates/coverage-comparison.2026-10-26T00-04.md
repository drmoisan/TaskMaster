# Coverage Comparison (Issue #1007, remediation cycle 1, P5-T5)

Timestamp: 2026-10-26T00-04
Command: SCRATCH per-file helper over SCRATCH `issue-1007-r1-baseline-cobertura.xml` (P0-T4) and SCRATCH `issue-1007-r1-final-cobertura.xml` (the P5-T4 post-processed Cobertura document). Lines are de-duplicated per file and line number. Exclusion search: `git diff --stat HEAD -- coverage.config`, `git diff HEAD -G ExcludeFromCodeCoverage --name-only`, and a search of the two new files.
EXIT_CODE: 0
Output Summary: PASS. No final figure is below its baseline. Every N1-LINES line has at least one hit (all 0 to 1 for the body lines). No exclusion was added. Repository: lines 86.47 to 86.49 percent, branches 80.81 to 80.85 percent.

## Repository

| Figure | Baseline (P0-T4) | Final (P5-T4) | Result |
|---|---|---|---|
| Lines | 58411/67553 (86.47%) | 58427/67553 (86.49%) | PASS |
| Branches | 14433/17860 (80.81%) | 14440/17860 (80.85%) | PASS |

## Per-file (the four N-1 production files)

| File | Baseline line | Final line | Baseline branch | Final branch | Result |
|---|---|---|---|---|---|
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | 122/133 (91.73%) | 128/133 (96.24%) | 42/52 (80.77%) | 45/52 (86.54%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 143/152 (94.08%) | 146/152 (96.05%) | 60/68 (88.24%) | 61/68 (89.71%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 131/134 (97.76%) | 134/134 (100.00%) | 40/42 (95.24%) | 41/42 (97.62%) | PASS |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 243/249 (97.59%) | 249/249 (100.00%) | 69/86 (80.23%) | 73/86 (84.88%) | PASS |

## N1-LINES hits (instrumented lines only; condition lines, then body lines)

| Member block | Baseline hits | Final hits | Result |
|---|---|---|---|
| `BreadcrumbBridgeRouter.HandleLeafToggleAsync`, `row.IsCollapsed && !row.IsAnchoredCollapse` | condition 1; body 0, 0, 0, 0, 0, 0 | condition 1; body 1, 1, 1, 1, 1, 1 | PASS |
| `FolderBreadcrumbBridgeRouter.ToggleAsync`, `CollapsedAfterIndex != ActiveSegmentIndex` | condition 1, 1, 1, 1; body 0, 0, 0 | condition 1, 1, 1, 1; body 1, 1, 1 | PASS |
| `BreadcrumbStateModel.Expand`, matching block | condition 1, 1, 1, 1; body 0, 0, 0 | condition 1, 1, 1, 1; body 1, 1, 1 | PASS |
| `BreadcrumbBridgeCoordinator.PublishRouterOutputs`, `UnhandledArrowMessage` arm | condition 1; body 0, 0, 0 | condition 1; body 1, 1, 1 | PASS |
| `BreadcrumbBridgeCoordinator.RaiseSyntheticArrowKey`, `UnhandledArrowMessage` arm | condition 1; body 0, 0, 0 | condition 1; body 1, 1, 1 | PASS |

No production C# line changed, so no changed-line figure applies.

## Exclusion search

| Check | Result |
|---|---|
| `coverage.config` diff against `HEAD` | none (no output) |
| Files in the WRITE-SET diff whose changes add or remove `ExcludeFromCodeCoverage` | none |
| `ExcludeFromCodeCoverage` in the two new test files | 0 |
