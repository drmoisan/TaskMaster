# [P6-T2] Post-Format File-Size Audit (authoritative)

- **Issue:** #438
- **Task:** [P6-T2]
- **Timestamp:** 2026-08-08T11-41
- **`<P0-T3-sha>`:** `904b4c38dba0f9f41707c3c0f077e123c78de59c`

## Command

`pwsh -NoProfile -Command "$files = @(git diff --name-only 904b4c38dba0f9f41707c3c0f077e123c78de59c -- '*.cs') + @(git ls-files --others --exclude-standard -- '*.cs') | Sort-Object -Unique | Where-Object { Test-Path $_ }; $files | ForEach-Object { '{0} {1}' -f (Get-Content $_).Count, $_ } ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

## First run — one violation, remediated

The first audit reported **502 lines** for `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs`, exceeding the 500-line ceiling by 2.

Remediation followed the D2 pattern: the AC-6 end-to-end typing scenario
(`EightCharacterQueryTypedThroughTheSeam_DeliversTheFullTextAndCompleteRowSet`) was extracted verbatim into a new partial file `BreadcrumbDropDownSearchIntegrationTests.Part2.cs`, the primary class gained the `partial` keyword, and no `[TestClass]` attribute was repeated. A `<Compile Include>` entry was added for the new file. **No assertion, expected value, or test name changed.**

Per the plan, the QA loop then restarted from P6-T1:

- `csharpier format .` -> EXIT_CODE 0, `Formatted 1501 files in 1496ms.`
- `csharpier check .` -> EXIT_CODE 0, `Checked 1501 files in 4446ms.`

## Second run — authoritative result

| Lines | File |
|---:|---|
| 477 | `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` |
| 298 | `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` |
| 499 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` |
| 234 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` |
| 463 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` |
| 173 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` |
| 394 | `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` |
| 129 | `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs` |
| 345 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` |
| 477 | `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` |
| 223 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` |
| 487 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` |
| 102 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` |
| 463 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` |
| 75 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` |
| 355 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` |
| 459 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` |
| 53 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs` |
| 481 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` |
| 45 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` |
| 68 | `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` |
| 143 | `QuickFiler/Viewers/IItemViewer.cs` |
| 319 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| 81 | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` |
| 304 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs` |
| 275 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs` |
| 474 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` |
| 59 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` |
| 485 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` |
| 97 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` |

**Files enumerated: 30. Files exceeding 500 lines: 0.** Largest is `BreadcrumbDropDownHostTests.cs` at 499.

## Manifest completeness check

The audit is valid only if it enumerates all new `.cs` files created by this plan. All 12 required files are present:

| # | Required file | In list |
|---|---|---|
| 1 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` | yes (59) |
| 2 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | yes (97) |
| 3 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | yes (75) |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs` | yes (53) |
| 5 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` | yes (102) |
| 6 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` | yes (45) |
| 7 | `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` | yes (298) |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | yes (234) |
| 9 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | yes (173) |
| 10 | `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` | yes (394) |
| 11 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs` | yes (304) |
| 12 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs` | yes (275) |

Plus one file added by this task's remediation: `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs` (129), bringing the new-file total to **13**. The `git ls-files --others` term is what makes these untracked files visible; `git diff` alone would not list them.

## Result

- **Output Summary:** EXIT_CODE 0. The first audit found one violation (`BreadcrumbDropDownSearchIntegrationTests.cs` at 502 lines); it was remediated by extracting a cohesive partial per D2 and the QA loop was restarted from P6-T1 (format and check both EXIT_CODE 0). The authoritative second audit enumerates 30 added or modified `.cs` files, **all at or under 500 lines** (largest 499), and contains every one of the 12 manifest files plus the remediation partial. Accept criteria met.
