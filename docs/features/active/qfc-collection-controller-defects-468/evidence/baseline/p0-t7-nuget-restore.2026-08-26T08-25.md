# [P0-T7] NuGet package restore

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

The worktree had no `packages/` directory before this task ran. Every project declares an
`EnsureNuGetPackageBuildImports` target whose `<Error>` fires before compilation when the tree is
missing, so the restore is a hard precondition for the analyzer and nullable gates.

The wrapper resolves MSBuild through `vswhere.exe` and emits:

```
msbuild <WS>\TaskMaster.sln /t:Restore "/p:Configuration=Debug" "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m
```

Tail of the restore log (host paths replaced with `<WS>`):

```
    Installed:
        172 package(s) to packages.config projects
1>Done Building Project "<WS>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.89
```

### Acceptance verification

- `EXIT_CODE: 0` — the wrapper throws on a non-zero MSBuild exit code and did not throw; the
  MSBuild summary reports `Build succeeded. 0 Warning(s) 0 Error(s)`.
- A `packages/` directory now exists at the workspace root and contains **172** entries, matching
  the `172 package(s) to packages.config projects` reported by NuGet.

Result: PASS. Both acceptance conditions are met.
