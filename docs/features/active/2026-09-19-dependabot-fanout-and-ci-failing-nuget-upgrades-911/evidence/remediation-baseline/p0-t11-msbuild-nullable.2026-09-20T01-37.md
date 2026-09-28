# MSBuild Nullable Build Baseline — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-34-50
- Task: [P0-T11]
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

Character for character the command in `.github/workflows/_build-nullable.yml`. Two omissions are
load-bearing and were not "restored":

- **No `/p:Nullable=enable`.** No project here carries a `<Nullable>` element and there is no
  `Directory.Build.props`, so the property would conscript every file that has never adopted the
  pragma. CI omits it deliberately.
- **No `/t:Build`.** A warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every
  project, so the gate could not fail.

## Console Tail, Verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.73
EXIT=0
```

## Log Measurements

Log: `coverage/nullable.msbuild.log`, 11,854 lines, gitignored and not committed.

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `OUTLOOK-CLOSED` recorded | true | true | PASS |
| Lines containing `/out:obj\Debug\` | at least 18 | **36** | PASS |
| Distinct assembly outputs named on those lines | — | **18** | — |
| Lines containing `CS86` | — | 0 | — |

The 36 lines are two per compiled project across the same 18 distinct output assemblies the
analyzer build compiled. This is the non-vacuity observation required by **gate rule 7**: a build
that skipped every compile reports zero errors and zero such lines.

Zero `CS86` lines. Per-file nullable enforcement is opt-in by `#nullable enable` pragma, and
`/p:TreatWarningsAsErrors=true` promotes a `CS86xx` diagnostic in an opted-in file to a build
error. None arose.

The needle was built from `[char]92` rather than typed inside a PowerShell double-quoted string,
for the reason recorded in the [P0-T10] artifact.

## Gate Rule 14 Note

This is the **post-merge** nullable baseline, per decision **D6**.

## Output Summary

Nullable rebuild exited 0 with 0 warnings and 0 errors. 36 lines carrying `/out:obj\Debug\` across
18 distinct assemblies, so the build was non-vacuous. Zero `CS86` diagnostics. Outlook closed.
