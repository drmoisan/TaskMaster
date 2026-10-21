Timestamp: 2026-08-31T10-53
Command: git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637
EXIT_CODE: 0
Before porcelain output:
(no output)

Command: pwsh -NoProfile -Command 'dotnet tool run csharpier format .; "EXIT_CODE=$LASTEXITCODE"'
EXIT_CODE: 0
Output:
Formatted 1564 files in 6281ms.
EXIT_CODE=0

Command: git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637
EXIT_CODE: 0
After porcelain output:
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs
 M QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs
 M QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
 M QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
 M QuickFiler/Controllers/EfcDataModel.FilingStem.cs
 M QuickFiler/Controllers/EfcDataModel.cs
 M QuickFiler/Controllers/EfcSelectionGuard.cs

Command: git status --porcelain -- UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization
EXIT_CODE: 0
Out-of-scope porcelain output:
(no output)

Output Summary: CSharpier formatted 1564 files and exited 0. The eight changed paths are all plan-owned source or test paths. BASELINE_FORMAT_DRIFT is empty, and the out-of-scope tree guard produced no output.
