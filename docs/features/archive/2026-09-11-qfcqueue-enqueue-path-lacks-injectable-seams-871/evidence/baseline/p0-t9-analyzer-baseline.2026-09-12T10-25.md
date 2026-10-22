# P0-T9 — Analyzer gate baseline

Timestamp: 2026-09-13T14-52
ReAnchoredAt: 2026-09-13T14-52
ReAnchorReason: merge commit 8213826f brought origin/main into this branch after this baseline was
first captured. The merge changed compiled sources in two test projects and one production project and
added a new shared source file, so the analyzer diagnostic counts are properties of the post-merge tree
rather than of the tree the superseded capture measured. The baseline is therefore re-measured.
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

## Build summary, verbatim from the tail of the build output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.36
ANALYZE-EXIT: 0
```

## Counts captured by anchored pattern, not by substring search

The two integers below were captured by anchored regular expressions applied to the whole summary
line, as the acceptance condition requires. The patterns match a line consisting only of leading
whitespace, an integer, and the literal count word followed by end of line, so a zero-error substring
occurring inside a ten-error line cannot satisfy either pattern. The matched lines are reproduced in
full so the capture is auditable.

```
ERRLINE: [    0 Error(s)]
WARNLINE: [    0 Warning(s)]
```

AnalyzerBaselineErrorCount: 0
AnalyzerBaselineWarningCount: 0

## Movement against the superseded capture

SupersededAnalyzerBaselineErrorCount: 0
SupersededAnalyzerBaselineWarningCount: 0

Both counts are unchanged. The merge introduced no analyzer diagnostic.

## Evidence that the compile actually ran

The build used the rebuild target, as the catalogue requires. The output shows the solution and its
project files completing the Rebuild target, including `CopyFilesToOutputDirectory` steps that copy
freshly produced assemblies from the intermediate directory, so the gate was not satisfied by an
incremental up-to-date skip.

Output Summary: The analyzer-enforcing solution rebuild exited 0 with 0 errors and 0 warnings, both
unchanged from the superseded pre-merge capture. Both counts were read by anchored patterns over the
whole summary line. The command was run while this item held the shared build lock, which was released
immediately after it returned. Acceptance met.
