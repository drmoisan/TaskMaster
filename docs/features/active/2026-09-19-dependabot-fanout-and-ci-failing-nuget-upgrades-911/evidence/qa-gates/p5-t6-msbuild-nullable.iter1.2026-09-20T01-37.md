# Final QA Step 6 — MSBuild Nullable Build, Iteration 1

- Timestamp: 2026-09-20T09-10-56
- Task: [P5-T6]
- Command: CMD-MSBUILD-NULLABLE
- EXIT_CODE: 0
- `OUTLOOK-CLOSED: true`

## CMD-OUTLOOK Precondition

```
Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count
```

Returned **0**. `OUTLOOK-CLOSED: true`. Closed by the user, not killed.

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"; exit $LASTEXITCODE'
```

Character for character the command in `.github/workflows/_build-nullable.yml`. Two omissions
are load-bearing and were not "restored": **no `/p:Nullable=enable`**, which would conscript
every file that has never adopted the pragma and which CI omits deliberately; and **no
`/t:Build`**, which would return exit 0 having skipped `CoreCompile` on every project.

## Console Tail, Verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.04
EXIT=0
```

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | PASS |
| `OUTLOOK-CLOSED` recorded | true | **true** | PASS |
| Lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log` | at least 18 | **36** | PASS |
| Lines containing `CS86` | — | **0** | — |

**36** lines, two per compiled project across the same 18 distinct output assemblies, which is
the non-vacuity observation **gate rule 7** requires and matches the [P0-T11] baseline exactly.

Zero `CS86` lines. Nullable enforcement here is per-file opt-in by `#nullable enable` pragma,
and `/p:TreatWarningsAsErrors=true` promotes a `CS86xx` diagnostic in an opted-in file to a
build error. None arose.

## Output Summary

Nullable rebuild exited 0 with 0 warnings and 0 errors. 36 lines carrying `/out:obj\Debug\`
across 18 assemblies, zero `CS86` diagnostics. Outlook closed throughout. Step 6 of the final
loop passes.
