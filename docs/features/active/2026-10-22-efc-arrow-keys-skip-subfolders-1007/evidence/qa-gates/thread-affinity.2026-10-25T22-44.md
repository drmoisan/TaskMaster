# Thread Affinity (Issue #1007, P18-T2)

Timestamp: 2026-10-25T22-44
Command: ripgrep count of `ConfigureAwait\(false\)` and of `lock \(` over `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`; positive control: the same `ConfigureAwait\(false\)` count over `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs`
EXIT_CODE: 0
Output Summary: the EFC router files (5: `BreadcrumbBridgeRouter.cs`, `.Arrows.cs`, `.Descendants.cs`, `.Navigation.cs`, `.Selection.cs`) contain 0 `ConfigureAwait(false)` and 0 `lock (` occurrences, so #1002 invariant I6 holds. The positive control finds 6 `ConfigureAwait(false)` occurrences in the QFC router partial, which shows the search pattern matches when the token is present. Every check holds.

| Check | Scope | Count | Expected | Result |
|---|---|---|---|---|
| ConfigureAwait(false) | QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs (5 files) | 0 | 0 | PASS |
| lock ( | QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs (5 files) | 0 | 0 | PASS |
| ConfigureAwait(false) (positive control) | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 6 | at least 1 | PASS |
