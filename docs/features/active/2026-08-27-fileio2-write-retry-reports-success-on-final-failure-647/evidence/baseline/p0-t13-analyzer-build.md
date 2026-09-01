# P0-T13 — Analyzer Baseline Build

Timestamp: 2026-08-31T18-55
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
ExpectedExitCode: 0

BASELINE_ANALYZER_WARNINGS: 5
BASELINE_ANALYZER_ERRORS: 0

Output Summary: MSBuild's final summary, transcribed:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.02
```

The build was verified to be a real compilation rather than a skipped incremental pass: the captured log carries 36 `csc.exe` invocations. `/t:Rebuild` was used deliberately, per CLAUDE.md section C#1, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change and a warm `/t:Build` would exit 0 with `CoreCompile` skipped on every project and no analyzer run.

All 5 warnings are the same diagnostic, raised once per affected project by `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`: `The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` The affected projects named in the summary are `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj` and `UtilitiesCS.Test.csproj`. None is an analyzer diagnostic and none originates from any file in this change's footprint.

Gate consequence: every later analyzer gate in this plan (P4-T8, P6-T3) is a non-increase against these two recorded integers — error count at most 0 and warning count at most 5 — never an absolute zero. Because `BASELINE_ANALYZER_ERRORS:` is 0 and `BASELINE_ANALYZER_WARNINGS:` is 5, which is non-zero, the non-increase clause governs and a later artifact recording 5 warnings and 0 errors records `CARRIED_BASELINE_ERRORS:` citing this artifact.
