# Phase 0 — NuGet Restore

Timestamp: 2026-08-26T08-30
Task: [P0-T6]

Command: `pwsh -NoProfile -File ./scripts/vscode/Invoke-Restore.ps1`
EXIT_CODE: 0

`Invoke-Restore.ps1` is the `packages.config`-aware equivalent of `nuget restore TaskMaster.sln` used by
this repository. `packages/` was absent in this fresh agent worktree before the command ran.

## Output (tail)

```
         Installed:
             172 package(s) to packages.config projects
     1>Done Building Project "<repo-root>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.43
```

## Acceptance check

Command: `ls packages/Meziantou.Analyzer.3.0.174/build/Meziantou.Analyzer.props`
EXIT_CODE: 0

```
packages/Meziantou.Analyzer.3.0.174/build/Meziantou.Analyzer.props
```

The required props file exists.

Output Summary: Restore succeeded with exit code 0, installing 172 packages to the `packages.config`
projects, 0 warnings and 0 errors. `packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props`
is present.
