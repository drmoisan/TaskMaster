# Baseline Coverage (Issue #1007)

Timestamp: 2026-10-23T13-43
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 (unscoped run; launched from a helper under SCRATCH with Set-Location to the worktree)
EXIT_CODE: 0
Output Summary:
- Test run (summary derived from the trx by the script): total 7599, executed 7599, passed 7599, failed 0, skipped 0. The shell-icon carve-out was not needed: the run completed in 39.3 seconds with no stall.
- BASELINE-FAILURES: none (empty list).
- One-line first-party coverage summary: `First-party coverage: lines 57539/66677 (86.30%), branches 14035/17419 (80.57%)`.
- Repository line coverage 86.30 percent; repository branch coverage 80.57 percent.
- The post-processed Cobertura document was copied to SCRATCH as `issue-1007-baseline-cobertura.xml` immediately after the run (not committed).

## Per-file coverage of PROD-MODIFY C# files (post-processed Cobertura, lines de-duplicated per line number)

| File | Line | Branch |
|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs | 161/163 (98.77%) | 87/94 (92.55%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs | 82/82 (100.00%) | 35/36 (97.22%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs | 175/179 (97.77%) | 54/58 (93.10%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | not instrumented (constant-only file, no executable line in the Cobertura document) | not instrumented |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs | 120/120 (100.00%) | 64/64 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs | 32/32 (100.00%) | 14/14 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 128/130 (98.46%) | 39/42 (92.86%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | 186/199 (93.47%) | 84/96 (87.50%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs | 303/303 (100.00%) | 99/102 (97.06%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | 68/68 (100.00%) | 25/26 (96.15%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs | 203/203 (100.00%) | 60/62 (96.77%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | 152/152 (100.00%) | 44/46 (95.65%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 256/256 (100.00%) | 57/60 (95.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 140/144 (97.22%) | 59/64 (92.19%) |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | 119/121 (98.35%) | 24/26 (92.31%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | 244/246 (99.19%) | 95/100 (95.00%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | 134/139 (96.40%) | 50/56 (89.29%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | 181/184 (98.37%) | 65/68 (95.59%) |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs | 63/63 (100.00%) | 11/12 (91.67%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 251/251 (100.00%) | 79/89 (88.76%) |
