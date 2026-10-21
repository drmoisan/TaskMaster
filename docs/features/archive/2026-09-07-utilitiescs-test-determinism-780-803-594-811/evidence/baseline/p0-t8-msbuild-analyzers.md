# P0-T8 — Baseline toolchain step 2 (msbuild analyzer gate)

Timestamp: 2026-09-08T09-22
Task: [P0-T8]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p0-t8.log;Verbosity=normal"
EXIT_CODE: 0

BASELINE_ANALYZER_WARNINGS: 0

`/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped
on every project, because MSBuild's up-to-date check does not invalidate on a command-line `/p:`
change, so the analyzer gate would run no analyzers and could not fail.

The normal-verbosity file log is written to `coverage/msbuild-p0-t8.log`, which is gitignored and
is not committed. `/v:q` on the console plus `Verbosity=normal` on the file logger is what makes
the `0 Error(s)` summary line readable; at minimal verbosity MSBuild omits the error summary
entirely.

## Observations

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines in the file log exactly equal to `    0 Error(s)` | `1` |
| Distinct analyzer or compiler warning ids in the log | none |
| `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` after the build | `True` |

Summary lines quoted verbatim from the file log:

```
    0 Warning(s)
    0 Error(s)
```

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The file log contains the line `    0 Error(s)` (found once). The assertion is on that verbatim
  summary line, not on the absence of the substring `error`, which a successful msbuild run prints
  in switch names and summary text regardless. PASS
- Both the `Warning(s)` and `Error(s)` lines are quoted verbatim, and
  `BASELINE_ANALYZER_WARNINGS: 0` is recorded. PASS
- `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` exists afterwards. PASS

## Output Summary

Full-solution rebuild with .NET analyzers and code-style enforcement enabled: exit 0, 0 warnings,
0 errors. The baseline analyzer warning ceiling for P1-T8, P5-T10 and P7-T3 is therefore 0, which
is a strict ceiling: any analyzer warning introduced by this change will exceed it and fail those
gates.
