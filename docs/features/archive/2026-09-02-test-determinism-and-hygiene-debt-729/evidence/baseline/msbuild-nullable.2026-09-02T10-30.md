# Baseline MSBuild nullable rebuild (P0-T10)

Timestamp: 2026-09-03T01-17

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`/p:Nullable=enable` was not added and `/t:Build` was not substituted. Tool resolution used the
Block K prelude (`MSBUILD_FOUND: True`, `VSTEST_FOUND: True`).

EXIT_CODE: 0

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.96
```

## Baseline diagnostic count

BaselineWarnings: 5
BaselineErrors: 0
BaselineCS8632Occurrences: 0

All 5 warnings are the same non-diagnostic-ID `System.Reactive.PackagesConfigCheck.targets`
packages.config message recorded in the P0-T9 artifact.

## Non-vacuity check

`/t:Rebuild` was used and the captured log contains 62 `CoreCompile:` task executions, so the
nullable and compiler diagnostics actually ran rather than being skipped by MSBuild
incrementality.

Output Summary: The baseline nullable gate passes with exit code 0, 0 errors, 5 warnings, and zero
occurrences of CS8632. The result is judged by exit code and by the `N Error(s)` summary line.
