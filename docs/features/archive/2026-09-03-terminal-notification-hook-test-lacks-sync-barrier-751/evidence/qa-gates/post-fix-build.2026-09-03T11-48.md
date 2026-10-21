# P2-T5 — Post-Fix Compile Gate (Issue #751)

Timestamp: 2026-09-03T14-42

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

## Output Summary

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.33
```

- Error count: **0**
- Warning count: **0**

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |

## Why this gate exists

`Volatile.Read(ref sut.InvokedTerminalHookCount)` takes a `ref` to a field of another type. That compiles
only while `InvokedTerminalHookCount` remains a directly accessible instance **field** rather than a
property. A property would not be `ref`-assignable and the expression would fail to compile. This build
confirms the field is still a field and that the three edits compile cleanly under the analyzer
configuration.

The build rebuilt all projects (`/t:Rebuild`), so `CoreCompile` ran on every project and the analyzers
actually executed rather than being skipped by MSBuild incrementality.
