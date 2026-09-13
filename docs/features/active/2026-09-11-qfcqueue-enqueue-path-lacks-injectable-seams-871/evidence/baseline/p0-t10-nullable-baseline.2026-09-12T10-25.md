# P0-T10 — Nullable and warnings-as-errors gate baseline

Timestamp: 2026-09-13T04-58
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0

## Build summary, verbatim from the tail of the build output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.05
NULLABLE-EXIT: 0
```

## Counts captured by anchored pattern

Captured by the same anchored-pattern rule as P0-T9: the pattern matches a line consisting only of
leading whitespace, an integer and the literal count word followed by end of line, so a zero-error
substring occurring inside a larger count line cannot satisfy it. The matched lines are reproduced in
full.

```
ERRLINE: [    0 Error(s)]
WARNLINE: [    0 Warning(s)]
```

NullableBaselineErrorCount: 0
NullableBaselineWarningCount: 0

## Command shape confirmed against policy

The command carries the rebuild target and does not carry a solution-wide nullable property, exactly
as the catalogue specifies and exactly as the repository nullable workflow runs it. Nullable
enforcement in this repository is per-file opt-in through the pragma, and the property would
conscript every file that has never adopted it.

Output Summary: The warnings-as-errors solution rebuild exited 0 with 0 errors and 0 warnings. Both
counts were read by anchored patterns over the whole summary line. The command was run while this
item held the shared build lock, which was released immediately after it returned. Acceptance met.
