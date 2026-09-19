# P0-T7 — NuGet Package Restore

Timestamp: 2026-09-19T12-30

Command:
```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911";
  $before = @(Get-ChildItem -Path ".\packages" -Directory).Count; "PACKAGE_DIRS_BEFORE=$before";
  & ".\scripts\vscode\Invoke-Restore.ps1";
  $after = @(Get-ChildItem -Path ".\packages" -Directory).Count; "PACKAGE_DIRS_AFTER=$after"'
```

EXIT_CODE: 0

## Package-directory counts

| Measurement | Value |
|---|---|
| Directories directly under `packages/` immediately **before** the run | **172** |
| Directories directly under `packages/` immediately **after** the run | **172** |
| Change across the run | 0 |

Both counts are integers greater than 100 and the count did not fall.

## Restore output

```
Using MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
MSBuild version 18.10.1-1.26427.6+3cd27c13e for .NET Framework
Build started 9/19/2026 12:16:18 PM.

     1>Project "…\dependabot-911\TaskMaster.sln" on node 1 (Restore target(s)).
     1>ValidateSolutionConfiguration:
         Building solution configuration "Debug|Any CPU".
       _GetAllRestoreProjectPathItems:
         Determining projects to restore...
       Restore:
         …
           OK https://api.nuget.org/v3/vulnerabilities/index.json 28ms
           OK https://api.nuget.org/v3-vulnerabilities/2026.09.19.05.33.30/vulnerability.base.json 20ms
           OK https://api.nuget.org/v3-vulnerabilities/…/vulnerability.update.json 48ms
     1>Done Building Project "…\dependabot-911\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.25
```

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS.
- The artifact records the count of directories directly under `packages/` immediately before and
  immediately after the run, each as an integer greater than 100: 172 and 172. PASS.
- The count did not fall across the run (172 to 172). PASS.

The tree was already restored — 172 package directories were measured in this worktree while the
plan was written, and 172 is what both captures report. This task therefore confirms the restore is
idempotent and the tree is complete rather than populating an empty tree. A count that fell across
the run, or either count at or below 100, would be a failure and neither occurred.

Output Summary: `Invoke-Restore.ps1` exited 0 with `Build succeeded, 0 Warning(s), 0 Error(s)`.
`packages/` held 172 directories before the run and 172 after, unchanged and both well above the
100 floor, confirming an idempotent restore over an already-complete package tree.
