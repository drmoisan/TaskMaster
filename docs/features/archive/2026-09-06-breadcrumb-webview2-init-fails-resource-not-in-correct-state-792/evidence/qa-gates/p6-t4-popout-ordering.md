# [P6-T4] Carry-before-removal ordering gate over `QfcCollectionController.PopOut.cs`

- Issue: #792
- Timestamp: 2026-09-17T20-56
- Command: `Select-String -LiteralPath QuickFiler/Controllers/QfcCollectionController.PopOut.cs` for the two member declarations (`^\s*public (async Task|void) PopOutControlGroup(Async)?\(int selection\)`), the two carry reads (`-SimpleMatch '= ReadPopOutCarry(group);'`), the two removal calls (`RemoveSpecificControlGroup(Async)?\(selection\);`) and the class-closing brace (`^    \}$`), then a per-member span check; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `9ac987a969d1f4786d296fee25817c8e5dde9233`)
- EXIT_CODE: 0
- Output Summary: `ORDERING: PASS` — member 1 (`PopOutControlGroup`, declared :77) reads the carry at :81 and removes at :84; member 2 (`PopOutControlGroupAsync`, declared :95) reads at :101 and removes at :104; each read precedes its removal and both calls of each member lie strictly between that member's declaration line and the next boundary.

## The four line numbers

| Member | Declaration | Span upper bound | `= ReadPopOutCarry(group);` | `RemoveSpecificControlGroup` call | Read before removal |
|---|---|---|---|---|---|
| 1 `public void PopOutControlGroup(int selection)` | 77 | 95 (next member's declaration) | 81 | 84 (`RemoveSpecificControlGroup(selection);`) | true |
| 2 `public async Task PopOutControlGroupAsync(int selection)` | 95 | 110 (class-closing brace; there is no following member) | 101 | 104 (`await RemoveSpecificControlGroupAsync(selection);`) | true |

Reads: 81, 101. Removals: 84, 104.

ORDERING: PASS

## Mechanical derivation (helper output, verbatim)

```
FILE: QuickFiler/Controllers/QfcCollectionController.PopOut.cs
TOTAL-LINES: 111
MEMBER-DECLARATIONS: 2
  DECL: 77 | public void PopOutControlGroup(int selection)
  DECL: 95 | public async Task PopOutControlGroupAsync(int selection)
CARRY-READS: 2
  READ: 81 | (IFolderSearchHandler handler, MailItemHelper helper) = ReadPopOutCarry(group);
  READ: 101 | (IFolderSearchHandler handler, MailItemHelper helper) = ReadPopOutCarry(group);
REMOVAL-CALLS: 2
  REMOVE: 84 | RemoveSpecificControlGroup(selection);
  REMOVE: 104 | await RemoveSpecificControlGroupAsync(selection);
CLASS-CLOSING-BRACE-LINES: 1
  CLASS-CLOSE: 110
MEMBER 1: decl 77 | bound 95 | read 81 | removal 84 | READ-BEFORE-REMOVAL: true
MEMBER 2: decl 95 | bound 110 | read 101 | removal 104 | READ-BEFORE-REMOVAL: true
ORDERING: PASS
```

The span check requires exactly one read and exactly one removal inside each member's span; a member with zero or two of either, or with the removal before the read, prints `READ-BEFORE-REMOVAL: false` and `ORDERING: FAIL`. The upper bound for the last member is the class-closing brace at :110 because the task's "next member's declaration line" has no successor for it.

## Positive control

`CONTROL-READPOPOUTCARRY-ALL-MENTIONS: 3 (lines 63, 81, 101)` — the bare identifier `ReadPopOutCarry` occurs at the declaration (:63) and the two call sites (:81, :101), so the `-SimpleMatch '= ReadPopOutCarry(group);'` needle correctly selects only the two assignment-form reads and excludes the declaration. The doc-comment mentions of "the removal call" at :73-75 and :91-93 do not contain the `RemoveSpecificControlGroup` identifier, so the removal count of 2 is the two live calls and no comment. A separate Read of the file before the helper ran gave the same line numbers.
