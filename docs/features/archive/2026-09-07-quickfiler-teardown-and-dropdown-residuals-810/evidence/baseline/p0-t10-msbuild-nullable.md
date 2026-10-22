# [P0-T10] Nullable-Build Baseline

Timestamp: 2026-09-08T09-21
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: The full-solution rebuild with warnings treated as errors succeeded with zero warnings and zero errors. This is the ceiling the [P7-T4] nullable gate is compared against.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.42
```

BASELINE-NULLABLE-WARNINGS: 0
BASELINE-NULLABLE-ERRORS: 0

## Command shape

`/p:Nullable=enable` was NOT added. No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property would be a solution-wide opt-in conscripting every file that has never adopted the pragma; CI omits it deliberately. Nullable enforcement here remains per-file opt-in through `#nullable enable`, promoted to errors by `/p:TreatWarningsAsErrors=true`.

`/t:Rebuild` was used, not `/t:Build`, for the same reason recorded at [P0-T9].

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire.
