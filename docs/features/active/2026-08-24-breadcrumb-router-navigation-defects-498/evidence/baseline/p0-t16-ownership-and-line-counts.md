# P0-T16 — Ownership and File-Size Baseline

Timestamp: 2026-08-26T08-49

Command: `pwsh -NoProfile -Command '$owned = @("QuickFiler/Controllers/BreadcrumbBridgeRouter.cs","QuickFiler/Controllers/KeyboardHandler.cs","QuickFiler/Resources/FolderBreadcrumb.html","UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs","UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs","UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs","QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs","QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs","UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs","UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs","UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs","UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs"); $gate = @("QuickFiler/Controllers/KbdActions.cs","QuickFiler/Controllers/EfcFormController.cs","QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs","UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs","UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs","UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs","QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs","QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs"); "=== OWNED ==="; foreach ($p in $owned) { "{0}|{1}" -f $p, (Get-Content -LiteralPath $p).Count }; "=== GATE ==="; foreach ($p in $gate) { "{0}|{1}|{2}" -f $p, (Get-Content -LiteralPath $p).Count, (git hash-object $p) }; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Every line count below was measured with `(Get-Content -LiteralPath $path).Count` in this execution
worktree at branch `bug/breadcrumb-router-navigation-defects-498`, HEAD
`61edc19befcf6c4e95b5acd32542f2dcdab41b78`. Git object ids are `git hash-object` over the working-tree
content, so a later re-measurement that returns the same id proves the file was not modified.

### `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` at baseline

**596 lines — YES, OVER 500 lines at baseline.** It exceeds the limit by 96 lines before this feature
adds anything, which is what makes the decision-D8 partial-class split in Phase 1 mandatory and first.

### A. Owned files (eight production, six test) — line counts

| # | File | Lines | File-Size Constraint table | Divergence |
|---:|---|---:|---:|---|
| 1 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 | 596 | none |
| 2 | `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | 414 | none |
| 3 | `QuickFiler/Resources/FolderBreadcrumb.html` | 489 | 489 | none |
| 4 | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1000 | 1000 | none |
| 5 | `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 98 | 98 | none |
| 6 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 457 | 457 | none |
| 7 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | 361 | none |
| 8 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 485 | 485 | none |
| 9 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | 462 | none |
| 10 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 435 | 435 | none |
| 11 | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 282 | not tabulated | n/a |
| 12 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | 314 | not tabulated | n/a |
| 13 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` | 379 | not tabulated | n/a |
| 14 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` | 320 | not tabulated | n/a |

`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` and
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` are owned but do not yet exist;
they are created by `P1-T2` and `P2-T1` respectively and therefore carry no baseline row.

### B. The twelve files in `P7-T3`'s ownership gate — line counts and git object ids

Eleven MUST-NOT-WRITE files plus the owned-but-unwritten `FolderPredictor.cs` (decision D5). Every one
of these must be byte-identical at `P7-T3` time, so the object id column is the binding record.

| # | File | Lines | Git object id |
|---:|---|---:|---|
| 1 | `QuickFiler/Controllers/KbdActions.cs` | 146 | `7ee548e822232533d900a6cc1a22f19206c51615` |
| 2 | `QuickFiler/Controllers/EfcFormController.cs` | 1084 | `836c013ca3667bd4c35a6478c2ae449156df5259` |
| 3 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | `ad1fc565a381237140c65e31baa1e7b7f22077c1` |
| 4 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` | 238 | `9bb41825d20843f0595458fd01d562d66cbb45c9` |
| 5 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 138 | `28edeb26f6eaa1c5960d80a4f0cb469927d2d875` |
| 6 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | `531e2a99e3161634d4579824c26769dd1f84cde7` |
| 7 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs` | 120 | `67d922abc8ec0d4f7778d521ecb60765c60ad9b1` |
| 8 | `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs` | 65 | `5eed70d34de5ec2999f51c5a2062aec75c10869e` |
| 9 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | 474 | `9b9e4cda995fb3d58257676e13c8fd56d1154d88` |
| 10 | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1000 | `8e9c834c5b5eac7ae238f3c8969392e310ff0d41` |
| 11 | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` | 405 | `ba01f8d30bbb6a5f1edf267aafb79b48b0bf2218` |
| 12 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | `57af52e2ff05729e537274e8b14a00b0b00b6189` |

### C. Comparison against the plan's "File-Size Constraint" table

**Zero divergence.** All twelve rows of the plan's table were re-measured and every one matches
exactly: 596, 694, 1084, 1000, 485, 457, 489, 462, 435, 414, 361 and 98. No figure in that table is
stale in this worktree, so `P7-T2`'s advisory comparison figures and this measured baseline agree.

### D. Pre-existing 500-line violations recorded for `P7-T2`

Three files exceed 500 lines at baseline and are neither caused nor repaired by this feature:

| File | Lines | Status |
|---|---:|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | MUST-NOT-WRITE (feature 464). Pre-existing. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1000 | Owned but not written (decision D5). Pre-existing. |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | MUST-NOT-WRITE (landed #439). Pre-existing. |

A fourth file, `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` at 596 lines, is also over the limit
but IS owned and IS repaired by this plan, in `P1-T2`.

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (487) and
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` (474) are close to the limit but are
MUST-NOT-WRITE, so this plan adds nothing to either.
