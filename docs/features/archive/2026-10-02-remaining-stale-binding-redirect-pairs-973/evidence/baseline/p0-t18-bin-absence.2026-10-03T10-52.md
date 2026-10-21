# P0-T18 assembly-absence negative control (issue #973)

Timestamp: 2026-10-03T10-52
Command: pwsh -NoProfile -Command 'foreach ($p in @("UtilitiesCS", "QuickFiler", "ToDoModel", "TaskMaster", "UtilitiesCS.Test")) { "BIN " + $p + " System.Linq.AsyncEnumerable.dll=" + (Test-Path -LiteralPath ("<execution-worktree-root>\" + $p + "\bin\Debug\System.Linq.AsyncEnumerable.dll")) + " System.Linq.Async.dll=" + (Test-Path -LiteralPath ("<execution-worktree-root>\" + $p + "\bin\Debug\System.Linq.Async.dll")) }' (CMD-BIN-PRESENCE, after the P0-T17 Rebuild)
EXIT_CODE: 0
Output Summary: in all five installing projects System.Linq.AsyncEnumerable.dll is absent from bin\Debug and System.Linq.Async.dll is present (positive control).

BIN UtilitiesCS System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True
BIN QuickFiler System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True
BIN ToDoModel System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True
BIN TaskMaster System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True
BIN UtilitiesCS.Test System.Linq.AsyncEnumerable.dll=False System.Linq.Async.dll=True
