Timestamp: 2026-08-31T10:32:12-04:00
Construction 1: pwsh -NoProfile -Command 'foreach ($p in @("QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcSelectionGuard.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs","QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs","QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs")) { $p + "=" + (Get-Content -LiteralPath $p).Count }'
Construction 2: rg -c "^" --glob "*.cs" QuickFiler/Controllers/ QuickFiler.Test/Controllers/ filtered to the same six paths
Baseline absence command: pwsh -NoProfile -Command 'Test-Path "QuickFiler\Controllers\EfcDataModel.FilingStem.cs"'
EXIT_CODE: 0 for all commands
Output Summary: Both line-count constructions agree. EfcDataModel.cs=485; BreadcrumbBridgeRouter.Selection.cs=209; EfcSelectionGuard.cs=79; BreadcrumbBridgeRouterIssue439Tests.cs=694; EfcDataModelIssue614Tests.cs=123; EfcSelectionGuardTests.cs=296. EfcDataModel.FilingStem.cs baseline=False.

spec.md:401 lists 424 for EfcDataModel.cs, while AC25 at spec.md:977 lists 485. The ecdb1c84 planning base had 423. The merged-tree value 485 governs, leaving 15 lines to the 500-line limit.
