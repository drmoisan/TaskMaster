# P1-T8 — Seam build with the analyzer gate

Timestamp: 2026-09-08T09-37
Task: [P1-T8]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p1-t8.log;Verbosity=normal"
EXIT_CODE: 0

## Observations

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines in the file log exactly equal to `    0 Error(s)` | `1` |
| Analyzer / compiler warning count | `0` |
| `BASELINE_ANALYZER_WARNINGS` from P0-T8 | `0` |
| Warning count does not exceed the baseline | yes (0 is not greater than 0) |

Summary lines quoted verbatim from `coverage/msbuild-p1-t8.log`:

```
    0 Warning(s)
    0 Error(s)
```

A scan of the file log for lines matching the diagnostic shape
`: (warning|error) <id>` returned no lines, so there is no warning to enumerate by diagnostic id.

## What this proves

The build succeeded across the whole solution after Phase 1 replaced two public static properties
with optional parameters and added optional `TimeProvider?` parameters to two public members. The
in-solution callers named in spec.md therefore still compile unchanged against the new signatures:

- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:15,82-89` — calls
  `GetEmailDataInViewAsync` with four positional arguments and reads the frame builder; the new
  trailing `TimeProvider? timeProvider = null` is source-compatible.
- `ToDoModel/Data Model/ID/IDList.cs:130,226` — calls `FromDefaultFolder(Store, ...)`; the new
  trailing optional delegate parameter is source-compatible.

The two deleted static properties `DfDeedle.TableEtlInvoker` and `DfDeedle.StoreTableEtlInvoker`
are a public-surface removal. A surviving reader anywhere in the solution would have produced a
CS0117, and the build reports 0 errors, so every reader and writer of both properties was updated
in this same change.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The log contains `    0 Error(s)`. PASS
- Warning count recorded (0) and does not exceed `BASELINE_ANALYZER_WARNINGS` (0). PASS

## Output Summary

Full-solution rebuild after the Phase 1 seam work: exit 0, 0 warnings, 0 errors, no analyzer
diagnostics introduced. Production behaviour is unchanged because every new parameter defaults to
null, which resolves to the system clock and to the production ETL delegate respectively.
