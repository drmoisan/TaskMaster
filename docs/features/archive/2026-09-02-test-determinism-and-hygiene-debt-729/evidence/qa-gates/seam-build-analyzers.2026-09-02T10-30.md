# Analyzer rebuild after the TimeProvider seam (P1-T7)

Timestamp: 2026-09-03T01-40

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Tool resolution used the Block K prelude (`MSBUILD_FOUND: True`, `VSTEST_FOUND: True`).

EXIT_CODE: 0

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.90
```

## Output Summary

The build log contains **zero occurrences of `CS0123`** and **zero occurrences of `CS8632`**. Both
counts were taken by a case-sensitive fixed-string count over the captured full build log:

```
CS0123 occurrences: 0
CS8632 occurrences: 0
```

Warnings: 5, errors: 0, matching the P0-T9 baseline exactly. All 5 warnings remain the
non-diagnostic-ID `System.Reactive.PackagesConfigCheck.targets` packages.config message.

The absence of CS0123 confirms the method-group conversion
`_delay = delay ?? NonBlockingDelay.WaitAsync;` at `TaskMaster/AppGlobals/StoreRehookCoordinator.cs`
line 102 still binds to `Func<TimeSpan, Task>`: the D1 decision to use an explicit overload pair
rather than an optional `TimeProvider? = null` parameter preserved the unique 1-parameter
candidate. The absence of CS8632 confirms the narrowly scoped
`#nullable enable annotations` / `#nullable restore annotations` pair around `ITimer? timer = null`
is sufficient for the new nullable local.

Non-vacuity: `/t:Rebuild` was used and the captured log contains 58 `CoreCompile:` task executions,
so analyzers actually ran. The result is judged by exit code and by the `N Error(s)` summary line.
