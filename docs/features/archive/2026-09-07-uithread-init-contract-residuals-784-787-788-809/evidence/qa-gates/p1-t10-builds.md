# [P1-T10] Phase 1 builds — analyzer and nullable

Timestamp: 2026-09-08T01-06

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Both builds returned 0.

## Output Summary

Analyzer build:

```
    0 Warning(s)
    0 Error(s)
```

Nullable build:

```
    0 Warning(s)
    0 Error(s)
```

Build-output arrow-line count for the analyzer build: **18**. The `BASELINE_PROJECT_COUNT:` recorded by [P0-T10] is **18**. The two are **equal**, which is expected because Phase 1 adds no project and removes none.

Both builds used `/t:Rebuild` rather than `/t:Build`, and neither passed `/p:Nullable=enable`.

The seam therefore compiles and introduces no analyzer diagnostic and no nullable diagnostic. `UiThread.cs` carries `#nullable enable` at line 1, so its nullable-flow diagnostics are promoted to errors by the second build; the new `IUiCaptureSource.cs` carries the same pragma at its line 1.
