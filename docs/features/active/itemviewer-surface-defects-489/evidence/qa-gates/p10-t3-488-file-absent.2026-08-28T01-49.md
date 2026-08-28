# P10-T3 — `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (488-owned) is absent from the P10-T2 list

Timestamp: 2026-08-28T01-49
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Result

The command produces **zero output lines**. The path does not appear in the diff.

The same conclusion is readable directly from the P10-T2 scope-lock list recorded in
`FEATURE/evidence/qa-gates/p10-t2-scope-lock-diff.2026-08-28T01-49.md`: that list contains 25 paths
and `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is not among them. The seven `QuickFiler/Viewers/`
paths it does contain are `ItemViewer.cs`, `ItemViewer.Designer.cs`, `ItemViewer.DisplayState.cs`,
`ItemViewer.FolderSearch.cs`, `ItemViewerExpanded.cs`, `ItemViewerExpanded.Designer.cs` and
`IItemViewer.cs`.

## Why the gate is falsifiable

`git ls-files QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` returns the path, so the file is tracked
and present on this branch. The zero-output result is therefore a real observation that the file was
not modified, not the vacuous result of asserting about a path that does not exist. Had any phase of
this plan edited it, the command would have printed the path.

A targeted absence check names only the directory holding the path it asserts about, because widening
the pathspec cannot change the outcome for that path.

## Ownership

`ItemViewer.Breadcrumb.cs` is owned by sibling child **488** and is read-only for this feature. It is
one of the paths the scope lock names explicitly.

Output Summary: `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is **absent** from the P10-T2 diff.
`git diff --name-only <BASELINE_SHA> -- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` produces zero
output lines with `EXIT_CODE: 0`. The file is tracked and present on this branch, so the result is a
genuine no-change observation rather than a vacuous one. The 488-owned file is untouched.
