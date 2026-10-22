# P4-T22 — analyzer gate after the regression suite

Timestamp: 2026-09-13T16-37

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

EXIT_CODE: 0

Output Summary:
- `Build succeeded.`
- ErrorCount: 0
- WarningCount: 0
- Both integers were captured by an anchored regular expression over the whole summary line, per
  the rule P0-T9 states.
- Matches the P0-T9 baseline of 0 errors and 0 warnings, and the P1-T5, P2-T7 and P3-T9 results.
  The two `CS0649` diagnostics that P4-T3 recorded against the harness part cleared once P4-T9 and
  P4-T10 assigned the two fields concerned.

ErrorCount: 0
WarningCount: 0

Anchored patterns used:

```
^\s*(\d+) Error\(s\)$
^\s*(\d+) Warning\(s\)$
```

Build summary lines as printed:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
