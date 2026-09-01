# NuGet restore (P0-T5)

Timestamp: 2026-09-01T10-29
Task: [P0-T5]
Working directory: WORKTREE

Command: `pwsh -File scripts/vscode/Invoke-Restore.ps1`
EXIT_CODE: 0

This is the repo-sanctioned restore. `scripts/vscode/Invoke-Restore.ps1:36` runs
`msbuild /t:Restore /p:RestorePackagesConfig=true /m`, which covers both the `packages.config` projects
and the `PackageReference` projects. A bare `nuget restore` would cover only the former.

Output (tail; absolute worktree path replaced by the token `WORKTREE`, absolute machine paths in the
NuGet configuration and feed listings elided as `<machine config path>` and `<machine feed path>`):

```
NuGet Config files used:
    <machine config path>
    <machine config path>
    <machine config path>

Feeds used:
    <machine feed path>
    https://api.nuget.org/v3/index.json
    <machine feed path>

Installed:
    172 package(s) to packages.config projects
1>Done Building Project "WORKTREE\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.74
```

## Directory check

Command: `Test-Path -LiteralPath packages`
Result: `True`

Output Summary: Restore succeeded with `EXIT_CODE: 0`, zero warnings and zero errors. 172 packages were
installed to the `packages.config` projects, and the `packages` directory now exists in the worktree,
which was the acceptance condition. `packages/` was absent before this task, so this restore was a hard
prerequisite: every `EnsureNuGetPackageBuildImports` `Error` target fires at
`BeforeTargets="PrepareForBuild"`, so msbuild would hard-fail on every subsequent task without it. No
`REMEDIATION-REQUIRED` branch was taken.
