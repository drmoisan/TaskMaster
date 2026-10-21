# P8-T12 — The QfcItemController.FolderHandling.cs diff is the one-token rename and nothing else

Timestamp: 2026-08-28T01-27
Command: git diff --numstat <BASELINE_SHA> -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

```
1	1	QuickFiler/Controllers/QfcItemController.FolderHandling.cs
```

Exactly `1` added and `1` deleted. The added line contains no `ClearFolderItems`: a filter for that
token over the added lines of the diff returns **0**.

## The diff, verbatim

```
diff --git a/QuickFiler/Controllers/QfcItemController.FolderHandling.cs b/QuickFiler/Controllers/QfcItemController.FolderHandling.cs
index 059fe83f..3510edc4 100644
--- a/QuickFiler/Controllers/QfcItemController.FolderHandling.cs
+++ b/QuickFiler/Controllers/QfcItemController.FolderHandling.cs
@@ -179,7 +179,7 @@ namespace QuickFiler.Controllers
                 // high-confidence folder is preselected when present; otherwise the index-1 top
                 // suggestion is selected. The standalone static PopulateAndSelectFolder is retained
                 // unchanged for its existing unit tests.
-                _itemViewer.SetFolderItems(_folderHandler.FolderArray);
+                _itemViewer.AddFolderItems(_folderHandler.FolderArray);
 
                 // #325: additionally hand the row model (folder identity + prediction probability)
                 // to the tree/percentage population path. Sourced verbatim from the #324 contract
```

One hunk, one changed line, inside `AssignFolderComboBox`. The receiver `_itemViewer`, the argument
`_folderHandler.FolderArray`, the surrounding comments and the statement's position are all
untouched. `Set` became `Add` and nothing else changed.

## Why the clear-insertion is deliberately absent

This file is owned by sibling 446. #490 D1 has two halves: renaming the misleading member, and
inserting a `ClearFolderItems()` call before it so the population is genuinely a set rather than an
append. Only the rename lands here. The clear-insertion half is **deferred to 446 or a follow-up
issue** and is recorded in `FEATURE/spec.md` § Out-of-Scope Findings. Inserting it would be a
behavioural change to a member this feature does not own, would grow the diff beyond one token, and
would silently alter the folder list at the two idempotent `MailActions.cs` call sites, because
`BreadcrumbBridgeCoordinator.Clear()` also calls `_upgradeLifetime.Invalidate()`.

The `0` count above is the machine-checkable form of that deferral: had the clear been inserted, the
added-line filter would have returned a non-zero count and the numstat would have read `2 1`.

Output Summary: `git diff --numstat <BASELINE_SHA>` on `QfcItemController.FolderHandling.cs` reports
exactly `1` added and `1` deleted. The single hunk is the one-token
`SetFolderItems` to `AddFolderItems` rename at the sole call site inside `AssignFolderComboBox`, and
the added line contains no `ClearFolderItems` — the clear-insertion half of #490 D1 is deliberately
deferred to 446 or a follow-up and is recorded in spec § Out-of-Scope Findings.
