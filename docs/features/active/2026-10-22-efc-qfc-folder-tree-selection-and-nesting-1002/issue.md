# efc-qfc-folder-tree-selection-and-nesting (Issue #1002)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/ (Issue #1002)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1002
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1002
- Last Updated: 2026-10-22
- Work Mode: full-bug

## Summary

Three defects in the QuickFiler folder-selection surfaces. (1) The EFC viewer does not support selecting a subfolder of a listed folder the way the QFC viewer now does after issue #997 (PR #998). (2) Pressing the EFC trash button creates a trash entry but does not select it. (3) In the QFC viewer, a subfolder that has its own subfolders shows its expand affordance (+/−) on the left instead of the right, and deeper levels are not indented beneath their visible parent.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in), WebView2 breadcrumb surface
- Python version: not applicable (C# .NET Framework 4.8, JavaScript breadcrumb asset)
- Command/flags used: interactive use of the EFC and QFC viewers
- Data source or fixture: any mailbox whose folder tree has at least three nested levels

## Steps to Reproduce

1. Open the EFC viewer, open the folder suggestions, and try to drill into and select a subfolder of a suggested folder.
2. In the EFC viewer, press the trash button.
3. Open the QFC viewer, expand a suggested folder, then look at a child subfolder that itself has subfolders and expand it.

## Expected Behavior

1. EFC offers the same recursive subfolder expansion and selection that QFC gained in #997, and the selected subfolder becomes the filing destination.
2. After the trash button adds the trash entry, that entry is the selected entry.
3. In QFC, the expand affordance of every expandable subfolder row sits on the right of the row, matching the top-level rows, and each nested level is indented one step further than its visible parent.

## Actual Behavior

1. EFC does not offer recursive subfolder selection.
2. The trash entry is created but the selection stays where it was.
3. The expand affordance of a nested subfolder renders on the left, and nested levels share one indent.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: reported by the maintainer on 2026-10-22; no log excerpt.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

- The #997 fix (merge 59b888d38) added descendant support in `QuickFiler/Resources/FolderBreadcrumb.js`, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs`, `BreadcrumbSelectionSession.Descendants.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs`, and wiring in `QfcItemController.BreadcrumbWiring.cs` and `ItemViewer.Breadcrumb.cs`. The EFC path (`EfcViewer`, `EfcFormController`) is likely not wired to the descendant messages.
- Trash handling lives in `EfcFormController.Actions.cs` and `EfcDataModel.cs`.
- The subfolder toggle is appended after the label in `FolderBreadcrumb.js` (around the `subfolderElement.appendChild(toggle)` call); the left placement and flat indent are likely CSS or DOM-order issues in the descendant row rendering.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: EFC descendant wiring, trash selection, descendant row rendering order and depth indentation
- [ ] Integration scenario to retest: three-level folder tree in EFC and QFC
- [ ] Manual verification notes: affordance on the right at every level; indent increases per level

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
