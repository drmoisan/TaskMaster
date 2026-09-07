# P0-T12 — File-size baseline for the write set

Timestamp: 2026-09-07T14-14
Task: [P0-T12]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '@("QuickFiler\Controllers\QfcFormController.Deactivate.cs","QuickFiler\Interfaces\IQfcFormViewer.cs","QuickFiler\Viewers\QfcFormViewer.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs","QuickFiler\Viewers\ItemViewer.Breadcrumb.cs","QuickFiler\Controllers\QfcItemController.EventHandlers.cs","QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs","QuickFiler\Resources\FolderBreadcrumb.html","QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs","QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs") | ForEach-Object { $_ + " " + (Get-Content -LiteralPath $_).Count }'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

Every later task in this plan that re-measures a line count uses this idiom and no
other, so the baseline and the final audit are commensurable. The idiom
`(Get-Content $_ | Measure-Object -Line).Lines` is PROHIBITED throughout, because
`Measure-Object -Line` omits blank lines and under-reports every count by that file's
blank-line total.

## Measured physical line counts

| Path | Physical lines |
|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 73 |
| QuickFiler/Interfaces/IQfcFormViewer.cs | 72 |
| QuickFiler/Viewers/QfcFormViewer.cs | 293 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 498 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 107 |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 456 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 263 |
| QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | 395 |
| QuickFiler/Resources/FolderBreadcrumb.html | 490 |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 248 |
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 380 |

## The four pinned values

| Path | Required | Measured | Verdict |
|---|---|---|---|
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 498 | 498 | match |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 456 | 456 | match |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 248 | 248 | match |
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 380 | 380 | match |

All four measured values equal the values this plan was authored against. The tree
has not moved, so the file-size arithmetic in Phase 1 and Phase 5 stands and does not
need to be re-derived.

Output Summary: 11 paths measured with the recorded idiom. All four pinned values
(498, 456, 248, 380) match.
