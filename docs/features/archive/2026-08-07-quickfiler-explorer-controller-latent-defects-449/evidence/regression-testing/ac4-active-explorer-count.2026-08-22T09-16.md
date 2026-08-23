# AC-4 — Residual `ActiveExplorer()` Re-Resolution Count (Issue #449, [P2-T3])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n -F "ActiveExplorer()" -- QuickFiler/Controllers/QfcExplorerController.cs
```
EXIT_CODE: 0

Full output (verbatim, complete — nothing elided):
```
QuickFiler/Controllers/QfcExplorerController.cs:35:            _activeExplorer = _globals.Ol.App.ActiveExplorer();
```

## Result

**Exactly ONE matching line.** The count is 1, and that one line is the **constructor capture** at
line 35:

```csharp
_activeExplorer = _globals.Ol.App.ActiveExplorer();
```

This is the single legitimate resolution of the active explorer: it happens once, in the constructor,
and its result is stored in the `_activeExplorer` field that every other member of the class reads.
The AC-4 condition — that no residual re-resolution of `ActiveExplorer()` remains anywhere in the
file — is satisfied.

`-F` (fixed string) was used so the parentheses in `ActiveExplorer()` are matched literally rather
than as regular-expression grouping metacharacters, which would otherwise have matched the bare
identifier `ActiveExplorer` and over-reported.

## What was removed

Before [P2-T1] this search returned **two** lines. The second was line 140, inside the private helper
`NavigateToOutlookFolder(MailItem)`:

```csharp
_globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;
```

[P2-T1] replaced that assignment target with `_activeExplorer.CurrentFolder`, leaving the right-hand
side `(MAPIFolder)mailItem.Parent` unchanged. That was the only re-resolution in the file, so the
count fell from 2 to 1 and no further occurrence remains to remove.

## Scope note

The search is scoped to `QuickFiler/Controllers/QfcExplorerController.cs`, the file AC-4 concerns.
Other files in the repository legitimately call `ActiveExplorer()` — including the uncompiled
`QuickFiler/Legacy/QuickFileController.cs` — and are outside AC-4's scope and outside this issue's
file set. `git grep` reports working-tree content for tracked files, so this result reflects the
post-[P2-T1] state of the file rather than the committed state.

## Output Summary

`git grep -n -F "ActiveExplorer()"` scoped to `QuickFiler/Controllers/QfcExplorerController.cs`
returns **exactly one matching line**, EXIT_CODE 0. That line is
`:35: _activeExplorer = _globals.Ol.App.ActiveExplorer();` — the constructor capture. No residual
re-resolution of the active explorer remains anywhere in the file, so AC-4 is satisfied. The
previously present second occurrence at line 140 was eliminated by the [P2-T1] one-line fix.
