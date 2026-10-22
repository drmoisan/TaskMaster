# Seam Build: Group B Declaration-Only Seam (#1002)

Timestamp: 2026-10-22T10-09
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary:
- `0 Error(s)`, `0 Warning(s)`.
- Seam: internal `BreadcrumbBridgeRouter.BindRowsAsync` gains optional trailing `string? preferredRowText = null` (with its XML `param`) and calls `SelectPreferredRow(preferredRowText)` immediately before `DeliverDocument()`; `private void SelectPreferredRow(string? preferredRowText)` is declared in `BreadcrumbBridgeRouter.Selection.cs` with an empty body; `EfcFormController.BindBreadcrumbRowsAsync` gains optional `string preferredRowText = null` (no `?`, because that file has no `#nullable enable`) passed through to the router.
- Every existing caller of both bind methods compiled unchanged (public three-argument overload, internal four-argument calls, and the controller one-argument calls).
