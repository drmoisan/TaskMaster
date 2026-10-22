# P4-T8 deployment proof after the P4-T6 Rebuild (AC12)

Timestamp: 2026-10-06T18-26
Command: pwsh -NoProfile -Command 'foreach ($p in @("UtilitiesCS", "QuickFiler", "ToDoModel", "TaskMaster", "UtilitiesCS.Test", "TaskMaster.Test", "QuickFiler.Test", "Tags.Test")) { "BIN " + $p + " System.Linq.AsyncEnumerable.dll=" + (Test-Path -LiteralPath ("<execution-worktree-root>\" + $p + "\bin\Debug\System.Linq.AsyncEnumerable.dll")) + " System.Linq.Async.dll=" + (Test-Path -LiteralPath ("<execution-worktree-root>\" + $p + "\bin\Debug\System.Linq.Async.dll")) }' (CMD-BIN-PRESENCE with its five-project list extended by the three not-gated BIN-TRANSITIVE projects in the same invocation; the first five lines are the CMD-BIN-PRESENCE output)
EXIT_CODE: 0
Output Summary: All five gated projects carry both System.Linq.AsyncEnumerable.dll and System.Linq.Async.dll in bin\Debug after the P4-T6 Rebuild. The P0-T18 negative control read False for System.Linq.AsyncEnumerable.dll in all five. The three transitive test projects also carry both (recorded, not gated).

## CMD-BIN-PRESENCE (gated)

BIN UtilitiesCS System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN QuickFiler System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN ToDoModel System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN TaskMaster System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN UtilitiesCS.Test System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True

## Negative control (P0-T18, evidence/baseline/p0-t18-bin-absence.2026-10-03T10-52.md)

All five projects: System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True

## BIN-TRANSITIVE (recorded, not gated)

BIN-TRANSITIVE TaskMaster.Test System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN-TRANSITIVE QuickFiler.Test System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
BIN-TRANSITIVE Tags.Test System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True
