# [P0-T5] NuGet package restore

Timestamp: 2026-09-08T00-20

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`

EXIT_CODE: 0

Output Summary:

PACKAGE_DIRECTORY_COUNT: 172

The restore reported:

```
Installed:
    172 package(s) to packages.config projects
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:04.18
```

The `PACKAGE_DIRECTORY_COUNT` value is the integer produced by `(Get-ChildItem -Path 'packages' -Directory).Count` and is greater than 0. `packages/` is matched by `.gitignore:191` `**/[Pp]ackages/*` and is therefore invisible to `Glob` and to `Grep`; the count was taken with `Get-ChildItem`, which is not gitignore-aware. It agrees with the restore's own installed-package figure.

This task is load-bearing because every project in this solution declares an `EnsureNuGetPackageBuildImports` target whose `<Error>` fires at `BeforeTargets="PrepareForBuild"` (`UtilitiesCS/UtilitiesCS.csproj:1286`), so MSBuild hard-fails without `packages/`.
