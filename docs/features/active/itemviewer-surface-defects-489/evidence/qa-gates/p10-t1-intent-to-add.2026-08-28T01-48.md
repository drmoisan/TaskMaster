# P10-T1 — Intent-to-add for the four files this plan created

Timestamp: 2026-08-28T01-48
Command: git add -N QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Acceptance

`EXIT_CODE: 0`, and all four paths appear in
`git diff --name-only <BASELINE_SHA> -- QuickFiler.Test/`.

Verbatim output of that diff:

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
```

| Created file | Present in the diff |
|---|---|
| `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` | Yes |

All four. Acceptance met.

## Why the command was still run, and why it was a no-op

The plan runs `git add -N` before any diff or `git grep` gate that must observe a file this plan
created, because `git diff` does not see untracked files and `git grep` does not search them. As
executed, all four files were **already tracked**: each was committed by the phase that created it,
under the "commit after each phase" discipline this run follows to protect against losing unsaved
work. `git ls-files` returns all four:

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
```

`git add -N` on an already-tracked path is a no-op that exits `0`, so the command was run as the plan
prints it and its acceptance is satisfied on the observable condition — the four paths appear in the
diff — rather than on any state change it produced. The condition the task exists to guarantee holds
regardless of which of the two routes established it, and every subsequent Phase 10 gate can now
observe these four files.

Output Summary: `git add -N` over the four created test files returned `EXIT_CODE: 0`. All four paths
appear in `git diff --name-only <BASELINE_SHA> -- QuickFiler.Test/`, whose full 13-path output is
recorded above. The command was a no-op because each file had already been committed by the phase
that created it; the acceptance condition is nonetheless satisfied, and every later Phase 10 diff and
`git grep` gate can observe all four.
