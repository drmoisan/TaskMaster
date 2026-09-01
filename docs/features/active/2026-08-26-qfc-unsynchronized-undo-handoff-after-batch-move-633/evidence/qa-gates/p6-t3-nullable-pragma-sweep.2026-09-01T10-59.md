# Nullable pragma preservation sweep (P6-T3)

Timestamp: 2026-09-01T10-59
Task: [P6-T3]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/regexscan.ps1` searching each file for the literal
`#nullable`
EXIT_CODE: 0

## Per-file match counts

| File | Matches |
|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | 0 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 0 |

Total match count: **0**.

Output Summary: Neither changed production file carries a `#nullable` directive.

This is a preservation gate, not a change gate. The pre-change count was also 0, verified in P1-T3 for
`FilerQueue.cs` and in P4-T4 for `QfcFormController.EventHandlers.cs`, so the gate asserts that the
change did not add one rather than that it removed one.

The gate exists because nullable enforcement in this repository is per-file opt-in. Adding
`#nullable enable` to either file would conscript it into nullable flow analysis, and
`/p:TreatWarningsAsErrors=true` — the property the P0-T9 and P7-T5 type-check builds pass — would then
promote that file's `CS86xx` diagnostics to build errors. Neither file has ever been annotated for
nullability, so the opt-in would produce errors that have nothing to do with this fix.
