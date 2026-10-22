# [P0-T5] NuGet Restore

Timestamp: 2026-08-26T08-38

Task: [P0-T5]
Feature: docs/features/active/quickfiler-bug-family-446

## Precondition Observed

`ls -d packages` reported "No such file or directory" before this task ran. Every first-party
project declares an `EnsureNuGetPackageBuildImports` target whose `<Error>` fires before
compilation when the tree is missing, and every `Reference` `HintPath` under `..\packages\`
would be unresolvable.

## Command

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`
EXIT_CODE: 0

## Output Summary

MSBuild drove `/t:Restore /p:RestorePackagesConfig=true`, which covers both the `packages.config`
projects and the `PackageReference` projects. The restore reported:

```
Installed:
    172 package(s) to packages.config projects
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.43
```

Post-condition verified: `packages/` now contains 172 entries and the directory
`packages/Meziantou.Analyzer.3.0.174` exists, satisfying the task's acceptance condition.
