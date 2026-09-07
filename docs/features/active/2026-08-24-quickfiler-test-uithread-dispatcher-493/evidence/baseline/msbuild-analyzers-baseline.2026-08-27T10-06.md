# Analyzer Gate Baseline (P0-T8)

Timestamp: 2026-08-27T10-06
Task: [P0-T8]
Command: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: `Build succeeded.` with 5 warnings and 0 errors. All five warnings are the same
`System.Reactive.PackagesConfigCheck.targets(31,5)` notice ("The project contains a packages.config
file, which is not supported by System.Reactive v7.0 or later"), raised once each by
`UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, and
`UtilitiesCS.Test.csproj`. No analyzer diagnostic and no compiler diagnostic appears in the summary.

## MSBuild summary counts

| Metric | Value |
| --- | --- |
| Build result | `Build succeeded.` |
| Total warnings | 5 |
| Total errors | 0 |

Log path: `TestResults/plan-logs/p0-t8/msbuild-analyzers.log`

## Warning inventory (redacted)

All five entries are byte-identical apart from the owning project:

```
<repo-root>/packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [<repo-root>/<project>.csproj]
```

Owning projects: `UtilitiesCS/UtilitiesCS.csproj`, `ToDoModel/ToDoModel.csproj`,
`QuickFiler/QuickFiler.csproj`, `TaskMaster/TaskMaster.csproj`,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

## Interpretation

Exit code 0 satisfies the acceptance condition, so the non-zero branch of the plan's Notes rule 5
(`BLOCKED: pre-existing base-tree build failure`) was not taken. `/t:Rebuild` was used, not
`/t:Build`, so `CoreCompile` ran on every project and the analyzers actually executed. The
`P0-T6` analyzer back-fill was a precondition: without it every project fails with `error CS0006`.

Raw log is git-ignored under `TestResults/` and is not committed. This artifact quotes only redacted
excerpts.
