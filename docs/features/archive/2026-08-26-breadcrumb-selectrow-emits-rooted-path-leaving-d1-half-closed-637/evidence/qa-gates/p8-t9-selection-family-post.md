Timestamp: 2026-08-31T14:10:00-04:00
Search 1: `rg -n "Select(Row|HierarchyPath)\\s*\\(" --glob "*.cs" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`
Search 2: `rg -n "^\\s+(private void )?Select(Row|HierarchyPath)\\(" --glob "*.cs" .`
EXIT_CODE: 0 for both searches
Output Summary: Both searches returned the identical nine-line set: two declarations and seven call sites. No Family-B `SelectRow(int)` member appears.

Declarations:

- `BreadcrumbBridgeRouter.Selection.cs:83 SelectRow`
- `BreadcrumbBridgeRouter.Selection.cs:121 SelectHierarchyPath`

Call sites:

- `BreadcrumbBridgeRouter.Selection.cs:33`
- `BreadcrumbBridgeRouter.Selection.cs:47`
- `BreadcrumbBridgeRouter.cs:201`
- `BreadcrumbBridgeRouter.cs:286`
- `BreadcrumbBridgeRouter.Arrows.cs:138`
- `BreadcrumbBridgeRouter.Arrows.cs:153`
- `BreadcrumbBridgeRouter.Arrows.cs:161`
