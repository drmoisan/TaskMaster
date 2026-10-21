# MSBuild Analyzer Build Baseline — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-34-15
- Task: [P0-T10]
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

`/t:Rebuild`, not `/t:Build`. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, and
the gate could not fail.

## Console Tail, Verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.16
EXIT=0
```

## Log Measurements

Log: `coverage/analyzers.msbuild.log`, 11,876 lines. `coverage/` is gitignored at `.gitignore:144`
and the log is not committed.

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `OUTLOOK-CLOSED` recorded | true | true | PASS |
| Lines containing `CS0006` | exactly 0 | **0** | PASS |
| Lines containing `/out:obj\Debug\` | at least 18 | **36** | PASS |
| Distinct assembly outputs named on those lines | — | **18** | — |
| Lines containing `: error ` | — | 0 | — |
| Lines containing `: warning ` | — | 0 | — |

## The Non-Vacuity Observation

The `/out:obj\Debug\` count is the observation that distinguishes a real compile from a skipped
one. A warm build that skipped every compile reports zero errors and zero such lines, and would
therefore pass an exit-code-only check while proving nothing.

The 36 matching lines are two per compiled project — the echoed `csc.exe` command line and the
`BuildResponseFile` echo — across **18 distinct output assemblies**:

`QuickFiler.dll`, `QuickFiler.Test.dll`, `SVGControl.dll`, `SVGControl.Test.dll`, `Tags.dll`,
`Tags.Test.dll`, `TaskMaster.dll`, `TaskMaster.Test.dll`, `TaskTree.dll`, `TaskTree.Test.dll`,
`TaskVisualization.dll`, `TaskVisualization.Test.dll`, `ToDoModel.dll`, `ToDoModel.Test.dll`,
`UtilitiesCS.dll`, `UtilitiesCS.Test.dll`, `VBFunctions.dll`, `VBFunctions.Test.dll`.

Every project in the solution compiled. The log also carries 87 `CoreCompile` references and 36
`csc.exe` references, consistent with the same conclusion.

**Measurement note.** The `/out:obj\Debug\` search must be issued with single backslashes. A first
attempt built the needle inside a PowerShell double-quoted string as `"/out:obj\\Debug\\"`;
PowerShell does not treat the backslash as an escape, so the needle carried doubled backslashes and
matched 0 lines against a log in which the token is present 36 times. The needle was rebuilt from
`[char]92` and the count re-taken. The false zero is recorded here because it is exactly the shape
of a gate that reports clean for the wrong reason.

## Gate Rule 14 Note

This is the **post-merge** analyzer baseline. It supersedes any figure measured at `794d34f02`,
per decision **D6**: the merge of `origin/main` brought C# changes this branch had never built.

## Output Summary

Analyzer rebuild exited 0 with 0 warnings and 0 errors. Zero `CS0006` lines. 36 lines carrying
`/out:obj\Debug\` across 18 distinct assemblies, so the build was non-vacuous. Outlook was closed
throughout.
