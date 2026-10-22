# QA Gate: EFC Router Thread Affinity (#1002)

Timestamp: 2026-10-22T10-15
Command: cat QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs | grep -c "ConfigureAwait(false)"; cat QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs | grep -cE "lock \("; grep -c "ConfigureAwait" QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs; git diff $(git merge-base origin/main HEAD) -- QuickFiler/Controllers/EfcFormController.Actions.cs | grep '^+' | grep -c "ConfigureAwait(false)"; grep -c "ConfigureAwait(false)" UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs
EXIT_CODE: 0
Output Summary:
- Files searched: `BreadcrumbBridgeRouter.cs`, `.Arrows.cs`, `.Selection.cs`, `.Descendants.cs` (all four files matching `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`).
- `ConfigureAwait(false)` in those files: 0.
- `lock (` in those files: 0.
- `ConfigureAwait` anywhere in `BreadcrumbBridgeRouter.Descendants.cs`: 0. (A first run found 1 match in the class XML comment, which named the token; the comment was reworded and the search re-run.)
- Added lines containing `ConfigureAwait(false)` in the merge-base diff of `QuickFiler/Controllers/EfcFormController.Actions.cs`: 0.
- Positive control: `ConfigureAwait(false)` in `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs` (the QFC router): 5.
