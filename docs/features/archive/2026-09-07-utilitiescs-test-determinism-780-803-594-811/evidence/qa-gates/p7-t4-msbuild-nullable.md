# P7-T4 — Toolchain step 3 (nullable / warnings-as-errors)

Timestamp: 2026-09-08T10-07
Task: [P7-T4]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p7-t4.log;Verbosity=normal"
EXIT_CODE: 0
Toolchain pass: 3

## Observations

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines exactly equal to `    0 Error(s)` | `1` |
| Warning count | `0` |

Summary lines quoted verbatim from `coverage/msbuild-p7-t4.log`:

```
    0 Warning(s)
    0 Error(s)
```

This is the gate that matters most for the eight production files in the write set, all of which
carry `#nullable enable` and are therefore opted in to nullable flow analysis, with
`/p:TreatWarningsAsErrors=true` promoting any `CS86xx` diagnostic to a build error. The change adds
a null test against a non-nullable tuple element (`tableSnapshot.data is null` in `DfDeedle.cs`),
two new nullable-annotated optional parameters (`TimeProvider? timeProvider`, `TextWriter? writer`)
and a nullable delegate parameter, and none of them produced a diagnostic.

`/p:Nullable=enable` is not supplied, matching `.github/workflows/_build-nullable.yml`. Nullable
enforcement here is per-file opt-in through the pragma, and every file this change touches on the
production side already carries it, so no enforcement is lost. `/t:Rebuild` is used so compilation
actually runs.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The log contains `    0 Error(s)`. PASS

## Output Summary

Full-solution rebuild with warnings treated as errors after the complete change set: exit 0, 0
warnings, 0 errors. The new guard, the new optional parameters and the new delegate parameter all
compile clean under nullable analysis.
