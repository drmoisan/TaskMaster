# Baseline — Analyzer build

- Timestamp: 2026-09-02T01-04
- Issue: #678
- Task: [P0-T6]

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

## `R_BASELINE_ANALYZER_SUMMARY` — MSBuild summary lines, verbatim

```
    5 Warning(s)
    0 Error(s)
```

```
R_BASELINE_ANALYZER_SUMMARY = 5 warnings, 0 errors
```

## Non-vacuity control

`CoreCompile:` occurrences in the build log: **87**.

The count is greater than zero, so compilation actually ran and the analyzers ran with it.
A run that skipped `CoreCompile` on every project would exit 0 without executing any
analyzer, which is the vacuity hazard `/t:Rebuild` exists to remove. Total build log
length: 11778 lines. Elapsed: 00:00:19.76.

## The five warnings, enumerated

All five are the same diagnostic, emitted once per project that carries a `packages.config`
and references System.Reactive 7.0.0. None is a C# compiler diagnostic and none is an
analyzer rule.

Source (repository-relative):
`packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets(31,5)`

Text:

```
warning : The project contains a packages.config file, which is not supported by
System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this
message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this
is an unsupported scenario.)
```

Emitting projects, one warning each:

| # | Project |
|---|---|
| 1 | `QuickFiler/QuickFiler.csproj` |
| 2 | `TaskMaster/TaskMaster.csproj` |
| 3 | `ToDoModel/ToDoModel.csproj` |
| 4 | `UtilitiesCS/UtilitiesCS.csproj` |
| 5 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |

These five are pre-existing and unrelated to this cycle. P2-T3 compares its own warning
count against this baseline of 5 and names any new warning individually.

## Output Summary

EXIT_CODE 0. `5 Warning(s)` / `0 Error(s)`. `CoreCompile:` occurred 87 times, so the gate
is demonstrably non-vacuous. All five warnings are the same pre-existing System.Reactive
`packages.config` migration notice, one per affected project; no analyzer rule and no C#
compiler diagnostic was reported.
