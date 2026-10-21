# Toolchain baseline — NuGet restore (Issue #824, task P0-T4)

Timestamp: 2026-09-09T15-00

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; & "./scripts/vscode/Invoke-Restore.ps1"'`

EXIT_CODE: 0

Output Summary:

- MSBuild resolved through vswhere: `MSBuild version 18.9.1+a81b43525 for .NET Framework`.
- The `Restore` target ran against `TaskMaster.sln` for solution configuration `Debug|Any CPU`.
- Terminal summary lines:

```
Installed:
    172 package(s) to packages.config projects
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

- `RESTORE_EXIT=0`.

Falsifiable directory-count confirmation, second invocation:

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; Write-Output ("PACKAGE_DIRS=" + @(Get-ChildItem -Path packages -Directory -ErrorAction SilentlyContinue).Count)'`

```
PACKAGE_DIRS=172
```

`PACKAGE_DIRS=172` is greater than 0. The worktree had no `packages/` directory before this task, so
the observation discriminates. A directory count is used rather than a `Glob`, because the `Glob`
tool returns file paths and a directory-only pattern can return nothing even when the directories
exist.
