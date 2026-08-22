# Phase 0 — Test-Assembly Resolution (Issue #445)

Timestamp: 2026-08-22T09-24

Command:
```powershell
$assemblies = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | ForEach-Object { Resolve-Path -LiteralPath $_.FullName -Relative } | Where-Object { $_ -notmatch '\\\.claude\\' }
$assemblies.Count
$assemblies
```
Run from `WS` = `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6e508cbcd1e0a79d` via `pwsh -NoProfile`.

EXIT_CODE: 0

## Resolved assemblies (verbatim, relative paths)

```
COUNT=9
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

Count is **9**, one per `*.Test.csproj`, and the list includes `.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` as the task requires.

## Why the filter is applied to the RELATIVE path (Non-negotiable Command Constraint 4)

`WS` is itself located under `.claude\worktrees\`:

```
C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6e508cbcd1e0a79d
```

Every assembly's `FullName` therefore contains the literal `\.claude\`. Applying the exclusion to the absolute path would match all nine and discard every assembly in the workspace, producing an empty list and a vacuously green test run. The exclusion is applied only after `Resolve-Path -Relative` rewrites each path relative to `WS`, at which point `\.claude\` appears only in a genuinely foreign sibling agent worktree. This was verified empirically: the count is 9, not 0.

The purpose of the exclusion is to prevent a concurrent sibling agent worktree's stale `*.Test.dll` from being collected, which would produce bogus assembly-initialization failures attributable to a different branch's build.

Output Summary: The workspace-relative filter resolved exactly 9 test assemblies, one per `*.Test.csproj` in the solution, matching the expected count. `.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` is present and is the primary target for this issue's scoped runs. All nine paths are relative to `WS` and none lies under a sibling agent worktree. Applying the `\.claude\` exclusion to the relative rather than the absolute path is load-bearing: because `WS` itself sits under `.claude\worktrees\`, an absolute-path filter would have returned 0 assemblies and made every downstream test gate vacuous.
