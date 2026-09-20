# Final QA Step 5 — MSBuild Analyzer Build, Iteration 1

- Timestamp: 2026-09-20T09-10-27
- Task: [P5-T5]
- Command: CMD-MSBUILD-ANALYZERS
- EXIT_CODE: 0
- `OUTLOOK-CLOSED: true`

## CMD-OUTLOOK Precondition

```
Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count
```

Returned **0**. `OUTLOOK-CLOSED: true`. Outlook was closed by the user; it was not killed.

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"; exit $LASTEXITCODE'
```

`/t:Rebuild`, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a command-line
`/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project
and the gate cannot fail.

## Console Tail, Verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.88
EXIT=0
```

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | PASS |
| `OUTLOOK-CLOSED` recorded | true | **true** | PASS |
| Lines containing `CS0006` in `coverage/analyzers.msbuild.log` | exactly 0 | **0** | PASS |
| Lines containing `/out:obj\Debug\` | at least 18 | **36** | PASS |

## The Non-Vacuity Observation, Per Gate Rule 7

**36** lines carry `/out:obj\Debug\`, two per compiled project across the **18** distinct output
assemblies the [P0-T10] baseline enumerated. A build that skipped every compile reports zero
errors and zero such lines, so an exit-code-only check would pass while proving nothing.

The needle is built from `[char]92` rather than typed inside a PowerShell double-quoted string.
PowerShell does not treat the backslash as an escape, so a typed `"/out:obj\\Debug\\"` carries
doubled backslashes and matches 0 lines against a log in which the token is present 36 times.
The [P0-T10] artifact records that false zero when it first occurred.

The figure equals the [P0-T10] baseline exactly, which is expected: this cycle changed no C#
source or build-configuration file.

## Output Summary

Analyzer rebuild exited 0 with 0 warnings and 0 errors. Zero `CS0006` lines, 36 lines carrying
`/out:obj\Debug\` across 18 assemblies. Outlook closed throughout. Step 5 of the final loop
passes.
