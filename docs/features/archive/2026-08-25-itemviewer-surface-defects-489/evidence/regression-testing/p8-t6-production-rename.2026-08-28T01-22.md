# P8-T6 — Production call-site rename, SetFolderItems to AddFolderItems

Timestamp: 2026-08-28T01-22
Command: git grep -F -n "SetFolderItems" -- QuickFiler/Controllers/ ; git grep -F -n "AddFolderItems" -- QuickFiler/Controllers/ ; git diff --numstat <BASELINE_SHA> -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs QuickFiler/Controllers/QfcItemController.MailActions.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance — exactly one surviving SetFolderItems, and it is the historical comment

```
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:165:        // issued ClearFolderItems + SetFolderItems + SetFolderSelectedIndex(1) +
```

That is one match and one only. It is identified by its quoted comment text
`// issued ClearFolderItems + SetFolderItems + SetFolderSelectedIndex(1) +`, not by its line number.
The observed line number is `165`, recorded here for the record only; it gates nothing, exactly as
the task specifies. P2-T5 edited this same file six phases earlier and was constrained to append
after the file's last member, but this assertion does not depend on that constraint having held.

## The three renamed sites

```
QuickFiler/Controllers/QfcItemController.FolderHandling.cs:182:                _itemViewer.AddFolderItems(_folderHandler.FolderArray);
QuickFiler/Controllers/QfcItemController.MailActions.cs:239:                _itemViewer.AddFolderItems(new[] { "Trash to Delete" });
QuickFiler/Controllers/QfcItemController.MailActions.cs:251:                    _itemViewer.AddFolderItems(new[] { "Trash to Delete" });
```

All three were located by the member anchors re-derived in P0-T18: the single site inside
`AssignFolderComboBox` (446-owned file, one token), and the two `FolderContains("Trash to Delete")`-guarded
sites in `MarkItemForDeletion` and `MarkItemForDeletionAsync` (484-owned file). Each rename is
invocation-only; no declaration, no test method name and no comment was renamed by this task.

## Diff shape

```
1	1	QuickFiler/Controllers/QfcItemController.FolderHandling.cs
3	3	QuickFiler/Controllers/QfcItemController.MailActions.cs
```

`FolderHandling.cs` carries exactly the one-token rename and nothing else — P8-T12 asserts that
independently. `MailActions.cs` carries three changed lines against `BASELINE_SHA`: the two renames
here plus P8-T3's discard form `_ = _itemViewer.FocusSubject();`. The `FlagTaskDialogResult`
read-back removal is P8-T8 and had not run when this measurement was taken.

Both files retain their UTF-8 BOM and CRLF line endings, and their line counts are unchanged at 235
and 257 respectively — the rename is token-for-token (`Set` to `Add`, both three characters) and adds
no line.

Output Summary: The three production call sites are renamed and
`git grep -F -n "SetFolderItems" -- QuickFiler/Controllers/` returns **exactly one** match, the
historical `// issued ClearFolderItems + SetFolderItems + SetFolderSelectedIndex(1) +` comment in
`QfcItemController.EventHandlers.cs`, left unchanged. `AddFolderItems` returns the three expected
invocation sites. `FolderHandling.cs` reports `1` added and `1` deleted; `MailActions.cs` reports `3`
and `3`, the two renames plus the earlier P8-T3 discard form.
