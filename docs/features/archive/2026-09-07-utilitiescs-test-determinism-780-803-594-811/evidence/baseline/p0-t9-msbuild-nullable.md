# P0-T9 — Baseline toolchain step 3 (nullable / warnings-as-errors)

Timestamp: 2026-09-08T09-23
Task: [P0-T9]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p0-t9.log;Verbosity=normal"
EXIT_CODE: 0

## `/p:Nullable=enable` was not supplied, and why

`/p:Nullable=enable` was deliberately omitted because nullable enforcement in this repository is
per-file opt-in through the `#nullable enable` directive, and no project carries a `<Nullable>`
element or a `Directory.Build.props`; supplying the property solution-wide would conscript every
file that has never adopted the pragma, which is why `.github/workflows/_build-nullable.yml`
omits it as well (CLAUDE.md C#1.3). Omitting it loses no enforcement over any file that has opted
in, and all eight production files in this change carry `#nullable enable`.

`/t:Rebuild` was used for the same reason as P0-T8: a warm `/t:Build` skips `CoreCompile` and the
gate cannot fail.

## Observations

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines in the file log exactly equal to `    0 Error(s)` | `1` |

Summary lines quoted verbatim from `coverage/msbuild-p0-t9.log`:

```
    0 Warning(s)
    0 Error(s)
```

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The file log contains `    0 Error(s)`. PASS
- The artifact states in one sentence that `/p:Nullable=enable` was not supplied and why. PASS

## Output Summary

Full-solution rebuild with `/p:TreatWarningsAsErrors=true`: exit 0, 0 warnings, 0 errors. The
nullable baseline is clean, so any `CS86xx` diagnostic introduced by the Phase 1 or Phase 3 edits
to the eight `#nullable enable` production files will surface as a build error at P7-T4.
