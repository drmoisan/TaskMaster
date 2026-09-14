# P1-T5 — Analyzer gate after the Phase 1 split

Timestamp: 2026-09-13T15-18
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded with 0 errors and 0 warnings, matching the P0-T9 baseline of 0 and 0.
No unnecessary-using diagnostic was reported against any of the three Phase 1 paths, so the
conditional remediation branch of this task did not fire and no using directive was removed.

## Counts captured by the anchored-pattern rule

The counts below were captured by matching whole summary lines against the anchored patterns
`^\s*(\d+) Error\(s\)$` and `^\s*(\d+) Warning\(s\)$` over the build output, not by a substring
search, because the zero-error text is also a substring of a ten-error line.

- Matched error summary line: `    0 Error(s)`
- Errors: 0
- Matched warning summary line: `    0 Warning(s)`
- Warnings: 0

Baseline recorded by P0-T9: 0 errors, 0 warnings. No change.

## Build summary lines

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.46
```

## Unnecessary-using remediation branch

The task text directs that any unnecessary-using diagnostic reported against
QuickFiler/Controllers/QfcQueue.cs, QuickFiler/Controllers/QfcQueue.Tlp.cs or
QuickFiler/Controllers/QfcQueue.UiIdle.cs be removed from the file the diagnostic names, followed by
a re-run of this task. The build reported zero warnings of any kind, so no such diagnostic exists and
the two new files retain the directive sets P1-T1 and P1-T2 recorded.
