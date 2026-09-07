# Phase 5 — Change set and retained locals ([P5-T7])

Timestamp: 2026-09-01T23-33

Command 1: `git diff --name-only origin/main...HEAD -- '*.cs'`
Command 2: `git status --porcelain`
Command 3: `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'FromHandle|new KeyEventArgs'` (VC-2)

EXIT_CODE: 0 for all three.

## Acceptance reading 1 — the change set is exactly three paths

Command 1 output, verbatim:

```
QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
QuickFiler/Controllers/QfcFormKeyHandler.cs
QuickFiler/Viewers/QfcFormViewer.cs
```

Three lines, and they are exactly the three authorised paths:
`QuickFiler/Controllers/QfcFormKeyHandler.cs`, `QuickFiler/Viewers/QfcFormViewer.cs` and
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. No other `.cs` path appears.

Call sites 2 through 5 in the spec's Call-Site Disposition table are therefore untouched:
QuickFiler/Viewers/QfcFormViewerDark.cs, QuickFiler/Viewers/QfcFormViewerExpanded.cs,
QuickFiler/Legacy/QfcFormLegacyViewer.cs and TaskVisualization/TaskViewer.cs are all absent from the list.

## Acceptance reading 2 — porcelain reports no `.cs` path

Paths reported by `git status --porcelain` that end in `.cs`: **0**.

The porcelain companion is required because a name-listing diff enumerates tracked changes only and is
blind to a newly created file, so the diff alone could not detect a violation of the no-new-file rule.
Together the two spans establish that no `.cs` file was created and left untracked.

## Acceptance reading 3 — VC-2 returns exactly two matches

Match count: **2**, exactly two as required. Matched lines:

```
L61: object sender = FromHandle(msg.HWnd);
L62: var e = new KeyEventArgs(keyData);
```

One match per literal: `FromHandle` on line 61 and `new KeyEventArgs` on line 62. Both lie inside the
`ProcessCmdKey` method body, which `[P5-T2]` measured as spanning lines 56 through 69.

This confirms the pre-existing unused locals were retained. Their removal is an explicit non-goal of the
spec: the bugfix policy requires the minimal targeted fix and forbids opportunistic refactors, so they
survive deliberately and a reviewer must not read their survival as an oversight. The count is unchanged
from the `[P0-T14]` pre-change reading of two; only the line numbers moved, from 64 and 65 to 61 and 62,
because the four-line guard collapsed to one line.

Output Summary: `git diff --name-only origin/main...HEAD -- '*.cs'` lists exactly the three authorised
paths and no other, `git status --porcelain` reports no path ending in `.cs`, and VC-2 returns exactly two
matches inside `ProcessCmdKey`, one per literal. AC-14 holds.
