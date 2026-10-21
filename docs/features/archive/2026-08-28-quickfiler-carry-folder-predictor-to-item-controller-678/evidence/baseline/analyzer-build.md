# Phase 0 — baseline analyzer build (P0-T6)

Timestamp: 2026-09-01T21-33

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Output Summary:

The MSBuild summary lines, reproduced verbatim:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.87
```

## BASELINE_ANALYZER_SUMMARY

- Warning count: **5**
- Error count: **0**

## Warning enumeration

All five warnings are the same uncoded MSBuild warning, emitted once per project that carries a
`packages.config` and references System.Reactive 7.0.0. The text, reproduced from the summary:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference. (You can suppress this message by setting the
RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)
```

The five owning projects are:

1. `UtilitiesCS/UtilitiesCS.csproj`
2. `ToDoModel/ToDoModel.csproj`
3. `QuickFiler/QuickFiler.csproj`
4. `TaskMaster/TaskMaster.csproj`
5. `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

These are build-infrastructure warnings from a NuGet package's targets file, not Roslyn analyzer
diagnostics. **Zero coded analyzer or compiler warnings** were emitted: a scan of the full build log
for the pattern `warning <CODE>:` returned no match at all, so no `CA`, `CS`, `IDE`, `MA`, `RCS`,
`S`, `AsyncFixer` or `RS` diagnostic was reported at any severity above message level.

## Non-vacuity control

`/t:Rebuild` was used rather than `/t:Build`, so MSBuild's incremental up-to-date check cannot skip
compilation. This was verified directly rather than assumed: the build log contains **53**
`CoreCompile:` target executions, so compilation, and therefore analyzer execution, actually ran on
this invocation. A warm `/t:Build` would have skipped `CoreCompile` on every project and exited 0
without running any analyzer, which would have made this baseline vacuous.
