# Phase 0 — Baseline Analyzer Build

Timestamp: 2026-08-26T08-31
Task: [P0-T10]

Command (run under `pwsh -NoProfile` with the `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

## Counts

| Metric | Value |
|---|---|
| Errors | **0** |
| Warnings | **5** |

## Log summary

```
Build succeeded.
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.74
```

## Warning breakdown

All 5 warnings are the same pre-existing package-compatibility notice, one per project that carries a
`packages.config` and references `System.Reactive` 7.0.0:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
Please migrate to PackageReference.
```

Affected projects: `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`.

None of the 5 is an analyzer diagnostic, and none originates in a file this feature owns. This is the
baseline the `[P7-T3]` final gate is compared against.

Output Summary: `/t:Rebuild` analyzer gate passes at the baseline with exit code 0, 0 errors, and 5
pre-existing `System.Reactive` `packages.config` warnings.
