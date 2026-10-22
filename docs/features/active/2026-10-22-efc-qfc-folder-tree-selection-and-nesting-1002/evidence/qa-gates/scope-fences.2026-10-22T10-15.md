# QA Gate: Scope Fences (#1002)

Timestamp: 2026-10-22T10-15
Command: MB=$(git merge-base origin/main HEAD); git diff --name-status $MB; git status --porcelain --untracked-files=all; grep -cE "FolderBreadcrumbBridgeRouter|FolderBreadcrumbDocument" QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs QuickFiler/Controllers/EfcFormController*.cs; grep -cE "FolderBreadcrumbBridgeRouter|FolderBreadcrumbDocument" QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs; git diff $MB -- 'QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs' UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | grep '^+' | grep -cE 'ArrowLeft|ArrowRight|ArrowUp|ArrowDown|HandleArrowKeyAsync'; grep -cE (same tokens) QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs
EXIT_CODE: 0
Output Summary:
- Code paths changed (merge-base diff plus porcelain status): only Write Set files. Modified: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `.Arrows.cs`, `.Selection.cs`, `QuickFiler/Controllers/EfcFormController.Actions.cs`, `EfcFormController.Breadcrumb.cs`, `QuickFiler/Resources/FolderBreadcrumb.js`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`, `BreadcrumbMessages.cs`, `BreadcrumbMessageCodec.cs`, `BreadcrumbHtmlRenderer.cs`, `BreadcrumbDocumentAssets.cs`, and the four project files. Added (untracked): `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs`, `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs`, the eight new test files, and `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs`. All other changed paths are under `<FEATURE>/`.
- Project files in the change: exactly the four in the Write Set (`QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`).
- Excluded files (BreadcrumbDescendantNode.cs, every FolderBreadcrumbBridgeRouter file, every BreadcrumbSelectionSession file, BreadcrumbRenderProjection.cs, every BreadcrumbBridgeCoordinator file, ItemViewer.Breadcrumb.cs, QfcItemController.BreadcrumbWiring.cs, FolderBreadcrumb.html, Resources.resx, EfcDataModel.cs) in the diff or status: 0.
- QFC reference search over the EFC router and controller files: 0. Positive control (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`): 3.
- Added lines containing `ArrowLeft`, `ArrowRight`, `ArrowUp`, `ArrowDown`, or `HandleArrowKeyAsync` in the router files and `BreadcrumbDocumentAssets.cs`: 0 (tracked diff) and 0 (new untracked `BreadcrumbBridgeRouter.Descendants.cs`).

## Residue outside the Write Set and the feature folder

The merge-base diff also lists three paths committed on this branch before execution began (by the promotion and research commits), not by this plan's tasks:

- `.claude/agent-memory/task-researcher/index_quickfiler_efc_defects.md` (modified)
- `.claude/agent-memory/task-researcher/project_efc_qfc_separate_breadcrumb_surfaces_1002.md` (added)
- `docs/features/potential/promoted/2026-10-22-efc-qfc-folder-tree-selection-and-nesting.md` (added; promotion lifecycle record)

These are agent-memory and promotion-lifecycle records owned by the orchestrator, not source files, and none is a spec-excluded file. The scope check therefore holds for every path this execution changed; the three inherited paths are reported here rather than reverted, because reverting committed lifecycle records is outside the plan and the executor does not commit.
