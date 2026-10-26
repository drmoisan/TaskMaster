# Coverage Comparison (Issue #1007, P19-T5)

Timestamp: 2026-10-25T23-09
Command: per-file and changed-line coverage helpers under SCRATCH over SCRATCH `issue-1007-baseline-cobertura.xml` (copied at P0-T6) and the post-processed Cobertura document of the P19-T4 run at 23-09 (copied to SCRATCH as `issue-1007-final-cobertura.xml`). Lines are de-duplicated per file and line number. Changed lines are the new-side lines of `git diff -U0` against the merge base of origin/main and HEAD that Cobertura instruments; replaced lines are the old-side lines of the same hunks in the baseline document.
EXIT_CODE: 0
Output Summary: PASS.
- Every PROD-CREATE file has at least 90 percent line coverage.
- No changed-line figure is below the baseline coverage of the lines it replaced.
- The final repository figures are not below the baseline: lines 86.40 percent against 86.30 percent; branches 80.73 percent against 80.57 percent.
- `coverage.config` is unchanged against the merge base, and the diff adds no `ExcludeFromCodeCoverage` attribute.

Baseline source note: the SCRATCH baseline copy from P0-T6 was still present in this session, so it was used directly and no regeneration from a detached worktree was needed. The committed baseline-coverage.2026-10-23T13-43.md stays authoritative for the headline baseline figures; its per-file figures match the SCRATCH copy used here.

## Repository

| Figure | Baseline (P0-T6) | Final (P19-T4) | Result |
|---|---|---|---|
| Lines | 57539/66677 (86.30%) | 58327/67510 (86.40%) | PASS |
| Branches | 14035/17419 (80.57%) | 14409/17848 (80.73%) | PASS |

## PROD-CREATE files (required: at least 90 percent line coverage)

| File | Line | Branch | Result |
|---|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs | 68/68 (100.00%) | 30/30 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs | 236/238 (99.16%) | 133/146 (91.10%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs | 114/122 (93.44%) | 47/60 (78.33%) | PASS |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs | 135/147 (91.84%) | 54/60 (90.00%) | PASS |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs | 69/73 (94.52%) | 36/40 (90.00%) | PASS |

## PROD-MODIFY C# files (final beside P0-T6)

| File | Final line | Final branch | P0-T6 line | P0-T6 branch |
|---|---|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs | 170/172 (98.84%) | 91/98 (92.86%) | 161/163 (98.77%) | 87/94 (92.55%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs | 114/116 (98.28%) | 48/50 (96.00%) | 82/82 (100.00%) | 35/36 (97.22%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs | 176/180 (97.78%) | 68/72 (94.44%) | 175/179 (97.77%) | 54/58 (93.10%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | not instrumented (constants only) | not instrumented | not instrumented | not instrumented |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs | 129/129 (100.00%) | 64/64 (100.00%) | 120/120 (100.00%) | 64/64 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs | 34/34 (100.00%) | 14/14 (100.00%) | 32/32 (100.00%) | 14/14 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 131/134 (97.76%) | 40/42 (95.24%) | 128/130 (98.46%) | 39/42 (92.86%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | 187/201 (93.03%) | 85/98 (86.73%) | 186/199 (93.47%) | 84/96 (87.50%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs | 333/336 (99.11%) | 109/114 (95.61%) | 303/303 (100.00%) | 99/102 (97.06%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | 78/78 (100.00%) | 30/32 (93.75%) | 68/68 (100.00%) | 25/26 (96.15%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs | 260/260 (100.00%) | 80/82 (97.56%) | 203/203 (100.00%) | 60/62 (96.77%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | 163/163 (100.00%) | 58/60 (96.67%) | 152/152 (100.00%) | 44/46 (95.65%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 254/255 (99.61%) | 56/60 (93.33%) | 256/256 (100.00%) | 57/60 (95.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 143/152 (94.08%) | 60/68 (88.24%) | 140/144 (97.22%) | 59/64 (92.19%) |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | 119/121 (98.35%) | 24/26 (92.31%) | 119/121 (98.35%) | 24/26 (92.31%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | 231/233 (99.14%) | 91/96 (94.79%) | 244/246 (99.19%) | 95/100 (95.00%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | 122/133 (91.73%) | 42/52 (80.77%) | 134/139 (96.40%) | 50/56 (89.29%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | 194/198 (97.98%) | 68/74 (91.89%) | 181/184 (98.37%) | 65/68 (95.59%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs | 76/76 (100.00%) | 19/20 (95.00%) | 63/63 (100.00%) | 11/12 (91.67%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 243/249 (97.59%) | 69/86 (80.23%) | 251/251 (100.00%) | 79/89 (88.76%) |

Some whole-file figures are lower than at P0-T6, because unchanged lines lost their callers when behaviour moved; examples are the coordinator's closed-arrow fall-through, now consumed by the Keyboard partial, and the EFC Arrows partial's in-flight and unbound-key branches. The changed-line rule below governs the no-regression check, and every changed line meets it.

## Changed-line coverage per modified production file

| File | Changed lines covered (final) | Replaced lines covered (baseline) | Result |
|---|---|---|---|
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | 55/55 (100.00%) | 61/61 (100.00%) | PASS |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs | 15/15 (100.00%) | 2/2 (100.00%) | PASS |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | 17/17 (100.00%) | 3/3 (100.00%) | PASS |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | 6/6 (100.00%) | 19/19 (100.00%) | PASS |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 8/8 (100.00%) | 10/10 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | none instrumented | none instrumented | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs | 7/7 (100.00%) | 6/6 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs | 10/10 (100.00%) | 1/1 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs | 3/3 (100.00%) | 1/1 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | 14/14 (100.00%) | 3/3 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs | 32/34 (94.12%) | none instrumented (additions only) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs | 27/27 (100.00%) | 18/18 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | 12/12 (100.00%) | 2/2 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs | 36/36 (100.00%) | 3/3 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs | 69/69 (100.00%) | 8/8 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | 3/3 (100.00%) | 1/1 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 37/38 (97.37%) | 32/34 (94.12%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 10/10 (100.00%) | 2/2 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 1/1 (100.00%) | 2/2 (100.00%) | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | 4/4 (100.00%) | 4/4 (100.00%) | PASS |

## Exclusion search

| Check | Result |
|---|---|
| `coverage.config` changed against the merge base | 0 files |
| `ExcludeFromCodeCoverage` added lines in the C# diff | 0 |
| `ExcludeFromCodeCoverage` in the five PROD-CREATE files | 0 |
