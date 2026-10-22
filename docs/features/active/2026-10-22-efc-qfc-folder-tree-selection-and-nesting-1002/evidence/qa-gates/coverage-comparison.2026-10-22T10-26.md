# QA Gate: Coverage Comparison (#1002)

Timestamp: 2026-10-22T10-26
Command: pwsh -NoProfile -File SCRATCH/issue-1002-covcompare.ps1 -Baseline SCRATCH/issue-1002-baseline-cobertura.xml -Final SCRATCH/issue-1002-final-cobertura.xml; pwsh -NoProfile -File SCRATCH/issue-1002-methods.ps1 (per-method lines, including async MoveNext state machines); git diff --stat $(git merge-base origin/main HEAD) -- coverage.config; git diff $(git merge-base origin/main HEAD) -- '*.cs' | grep '^+' | grep -c ExcludeFromCodeCoverage
EXIT_CODE: 0
Output Summary: no shortfall. Repository line and branch coverage rose; both new files and every new method are at 100 percent line coverage; every changed-line figure is 100 percent and not below the baseline coverage of the lines it replaced; no coverage exclusion was added.

## Repository figures (post-processed first-party Cobertura)

| Measure | Baseline (P0-T6) | Final (P9-T4, loop pass 3) | Floor | Result |
|---|---|---|---|---|
| Line | 57272/66515 = 86.10% | 57523/66780 = 86.14% | 80% | PASS, not lowered |
| Branch | 13923/17337 = 80.31% | 14016/17439 = 80.37% | 75% | PASS, not lowered |

## New files (line coverage, final)

| File | Covered/Valid |
|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | 82/82 = 100.00% |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | 63/63 = 100.00% |

## New methods in modified and new files (line coverage, final)

| Method | File | Covered/Valid |
|---|---|---|
| SetLeafDescendants | BreadcrumbRow.Descendants.cs | 17/17 = 100.00% |
| HandleDescendantToggleAsync | BreadcrumbBridgeRouter.Descendants.cs | 53/53 = 100.00% (async state machine MoveNext, lines 22 to 88) |
| ActivateDescendant | BreadcrumbBridgeRouter.Descendants.cs | 10/10 = 100.00% |
| TryGetRowSelection | BreadcrumbBridgeRouter.Selection.cs | 24/24 = 100.00% |
| SelectPreferredRow | BreadcrumbBridgeRouter.Selection.cs | 17/17 = 100.00% |
| PresentsText (helper) | BreadcrumbBridgeRouter.Selection.cs | 8/8 = 100.00% |
| ApplyDeleteGestureAsync | EfcFormController.Actions.cs | 7/7 = 100.00% (async state machine MoveNext, lines 100 to 108) |
| AppendDescendants (helper) | BreadcrumbHtmlRenderer.cs | 37/37 = 100.00% |

Remediation within this loop: the pass-2 figures were `SetLeafDescendants` 15/17 = 88.24 percent and `SelectPreferredRow` 15/17 = 88.24 percent. The two tests named in the pass-3 format artifact were added, and the loop was re-run from P9-T1.

## Changed lines per modified production file

Changed lines are the added or modified lines of the merge-base diff (`git diff -U0` new-side hunks) that Cobertura instruments in the final run; replaced lines are the old-side hunk lines that Cobertura instrumented in the baseline run.

| File | Changed (final) | Replaced (baseline) | Result |
|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 8/8 = 100.00% | none instrumented | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 3/3 = 100.00% | 1/1 = 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 15/15 = 100.00% | 2/2 = 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 49/49 = 100.00% | 3/3 = 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | not instrumented (string constants only) | not instrumented | N/A |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 5/5 = 100.00% | none instrumented | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 7/7 = 100.00% | 1/1 = 100.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 48/48 = 100.00% | 17/17 = 100.00% | PASS |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 5/5 = 100.00% | 2/2 = 100.00% | PASS |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 7/7 = 100.00% | 1/1 = 100.00% | PASS |

Whole-file final line coverage for context: BreadcrumbRow.cs 98.77%, BreadcrumbHtmlRenderer.cs 97.77%, BreadcrumbBridgeRouter.cs 99.19%, .Arrows.cs 96.40%, .Selection.cs 98.37%, EfcFormController.Actions.cs 23.42% (baseline 21.30%), EfcFormController.Breadcrumb.cs 64.46% (baseline 78/115 = 67.83%, final 78/121).

Observation on `EfcFormController.Breadcrumb.cs`: the whole-file figure fell because the unchanged private method `BindFolderRows` lost its only covered caller. At baseline the delete gesture called it (6 of its 7 lines covered through `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows`); the delete gesture now awaits `BindBreadcrumbRowsAsync` directly, as P5-T2 specifies, so `BindFolderRows` is reached only through `BindSourceFolderRows`, which no unit test exercises at baseline or now. The six lines are unchanged lines, so the changed-line rule is met, and the repository figures rose. This is recorded as an observation for review, not as a shortfall against the AC23 criteria.

## Exclusions

- `coverage.config`, `TaskMaster.runsettings`, `scripts/vscode/TaskMaster.cli.runsettings`: no diff from the merge base.
- `ExcludeFromCodeCoverage` added in the tracked diff: 0; in the new files: 0.

`FolderBreadcrumb.js` is measured by the node runner: 100.00 percent line coverage before and after (see `baseline-node-suites` and `node-suites-run`).
