# [P5-T4] Final QC loop, step 3 — type checking by nullable analysis

Timestamp: 2026-09-08T02-44

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

## Output Summary

```
    0 Warning(s)
    0 Error(s)
```

`/p:Nullable=enable` was not passed, and `/t:Rebuild` was used rather than `/t:Build`. `/p:Nullable=enable` is omitted because no project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that would conscript every file which has never adopted the `#nullable enable` pragma, and CI omits it deliberately; omitting it loses no enforcement over any file that has opted in, and `UtilitiesCS/Threading/UiThread.cs` and `UtilitiesCS/Threading/IUiCaptureSource.cs` both carry the pragma at line 1. `/t:Build` is not used because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on every project and would run no nullable-flow analysis at all.

TOOLCHAIN_LOOP_PASS: 1
