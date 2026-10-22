# [P0-T9] Analyzer-Build Baseline

Timestamp: 2026-09-08T09-20
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: The full-solution rebuild with the analyzer properties succeeded with zero warnings and zero errors. This is the ceiling the [P5-T2] and [P7-T3] analyzer gates are compared against.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:23.63
```

BASELINE-ANALYZER-WARNINGS: 0
BASELINE-ANALYZER-ERRORS: 0

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. No Outlook or test-host process was holding the build output, so the D5 stop condition did not fire and no process was terminated.

## D4 note

`/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, and would therefore run no analyzers at all.
