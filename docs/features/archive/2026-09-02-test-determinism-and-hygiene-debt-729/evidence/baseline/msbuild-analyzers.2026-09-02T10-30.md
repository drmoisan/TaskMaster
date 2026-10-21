# Baseline MSBuild analyzer rebuild (P0-T9)

Timestamp: 2026-09-03T01-15

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Tool resolution used the Block K prelude; `vswhere.exe` resolved both `MSBuild.exe` and
`vstest.console.exe` (`MSBUILD_FOUND: True`, `VSTEST_FOUND: True`).

EXIT_CODE: 0

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.97
```

## Baseline diagnostic count (comparison basis for P6-T3)

BaselineWarnings: 5
BaselineErrors: 0

All 5 warnings are the same non-diagnostic-ID message emitted once per packages.config project by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:

```
warning : The project contains a packages.config file, which is not supported by System.Reactive
v7.0 or later. Please migrate to PackageReference.
```

Zero warnings in the log carry a compiler or analyzer diagnostic identifier of the form
`warning CSxxxx` / `warning <RULEID>`; a scan of the captured log for that pattern returns no match.

## Non-vacuity check

`/t:Rebuild` was used rather than `/t:Build`, and the captured log contains 64 `CoreCompile:` task
executions, so compilation and therefore analyzer execution actually occurred rather than being
skipped by MSBuild incrementality.

Output Summary: The baseline analyzer gate passes with exit code 0, 0 errors, and 5 warnings, none
of which carries a compiler or analyzer diagnostic identifier. The result is judged by exit code
and by the `N Error(s)` summary line, not by searching the log text for the word "error".
