# P0-T9 — Analyzer gate baseline

Timestamp: 2026-09-13T04-57
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

## Build summary, verbatim from the tail of the build output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.91
ANALYZE-EXIT: 0
```

## Counts captured by anchored pattern, not by substring search

The two integers below were captured by anchored regular expressions applied to the whole summary
line, as the acceptance condition requires. The patterns match a line consisting only of leading
whitespace, an integer, and the literal count word followed by end of line, so a zero-error
substring occurring inside a ten-error line cannot satisfy either pattern. The matched lines are
reproduced in full so the capture is auditable.

```
ERRLINE: [    0 Error(s)]
WARNLINE: [    0 Warning(s)]
```

AnalyzerBaselineErrorCount: 0
AnalyzerBaselineWarningCount: 0

## Evidence that the compile actually ran

The build used the rebuild target, as the catalogue requires. The output shows the solution and its
project files completing the Rebuild target, including `CopyFilesToOutputDirectory` steps that copy
freshly produced assemblies from the intermediate directory, so the gate was not satisfied by an
incremental up-to-date skip.

Output Summary: The analyzer-enforcing solution rebuild exited 0 with 0 errors and 0 warnings. Both
counts were read by anchored patterns over the whole summary line. The command was run while this
item held the shared build lock, which was released immediately after it returned. Acceptance met.
