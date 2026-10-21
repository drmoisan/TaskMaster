# Bootstrap — NuGet Restore (P0-T7)

Timestamp: 2026-08-27T19-55

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug`

EXIT_CODE: 0

Output Summary:

```
Installed:
    172 package(s) to packages.config projects
Done Building Project "WS\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

(The MSBuild log printed the absolute solution path; it is recorded here with the workspace rendered
as the literal token `WS`.)

Post-condition check: the directory `packages/` exists under `WS` and contains 172 package folders.

Acceptance: `EXIT_CODE: 0` and `packages/` exists under `WS`. PASS.
