# Baseline — MSBuild Analyzer Rebuild (P0-T8)

Timestamp: 2026-09-01T12-22

Working directory: repository root (worktree for branch
`bug/qfc-metrics-flush-writes-empty-session-file-646`)
HEAD: `8a2054cd6c857195712c7db6cee0a34b631f3ca7`

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

## Verbatim Printed Summary Lines

```
Build succeeded.

    5 Warning(s)
    0 Error(s)
```

## Output Summary

The analyzer gate passes at baseline: `Build succeeded.`, 5 warnings, 0 errors, exit code
0. All 5 warnings are the same non-analyzer MSBuild warning emitted by the
`_RxCheckPackagesConfig` target in
`packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets(31,5)`,
reporting that a `packages.config` project is not supported by System.Reactive v7.0 or
later. It is raised once each for `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`,
and `UtilitiesCS.Test`. No Roslyn or .NET analyzer diagnostic (`CAxxxx`, `IDExxxx`,
`Sxxxx`, `RCSxxxx`, `MAxxxx`, `AsyncFixerxx`) appears in the output. This warning set is
pre-existing on the branch and unrelated to issue #646.

## Non-Vacuity Check

`/t:Rebuild` (not `/t:Build`) was used as required by `CLAUDE.md` C#1.2, so MSBuild's
incremental up-to-date check cannot skip `CoreCompile`. The captured build log contains 75
`CoreCompile` references and 36 `csc.exe` command-line occurrences, confirming compilation
and therefore analyzer execution actually occurred rather than being skipped. The gate was
capable of failing.

## Precondition Micro-Action Recorded

The first invocation of this command on this fresh worktree failed with EXIT_CODE 1 and
`37 Error(s)`, every error being the same NuGet message: `This project references NuGet
package(s) that are missing on this computer. Use NuGet Package Restore to download
them.` The worktree had no restored `packages/` directory. The repository's standard
restore for these `packages.config`-style legacy projects was run:

Command: `nuget restore TaskMaster.sln`
EXIT_CODE: 0
Output: `Installed: 172 package(s) to packages.config projects`

`packages/` is excluded from version control by `.gitignore` line 358, confirmed by
`git check-ignore -v packages/`, so this restore adds nothing to the change footprint
checked by P2-T8. The command above was then re-run and produced the recorded result.
