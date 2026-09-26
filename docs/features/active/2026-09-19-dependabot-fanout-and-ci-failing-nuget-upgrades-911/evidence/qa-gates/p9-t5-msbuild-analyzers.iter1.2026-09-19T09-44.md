# P9-T5 — C# QA step 2, solution-wide analyzer rebuild (iteration 1)

Timestamp: 2026-09-20T09-44

Command: CMD-OUTLOOK, then CMD-MSBUILD-ANALYZERS.

```
pwsh -NoProfile -Command 'Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count'

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"
```

`msbuild` resolved to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` and was
launched with the execution worktree as the working directory, set by `Set-Location` per gate
rule 16.

EXIT_CODE: 0

OUTLOOK-CLOSED: true

`Get-Process outlook` returned **0** immediately before the rebuild. Outlook was already closed and
was **not** terminated by this task, per the CMD-OUTLOOK rule that Outlook must be closed by the user
and never killed.

## Terminal output, tail

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.63
```

## Log measurements

The file logger wrote `coverage/analyzers.msbuild.log`, 11930 lines, at `Verbosity=normal`. The path
is under `coverage/`, which `.gitignore:144` covers, so the log never reaches a commit.

| Measurement | Value |
|---|---|
| Lines containing `CS0006` | **0** |
| Lines containing `/out:obj\Debug\` | **36** |

`/t:Rebuild` is used rather than `/t:Build` because MSBuild's up-to-date check does not invalidate on
a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzer at all.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `OUTLOOK-CLOSED: true` recorded | yes | recorded above | PASS |
| Lines containing `CS0006` in `coverage/analyzers.msbuild.log` | exactly 0 | 0 | PASS |
| Lines containing `/out:obj\Debug\` | at least 18, exact count recorded | **36** | PASS |

The `/out:` count is the non-vacuity observation AC25 requires. A build whose compile targets were
skipped reports zero errors just as a real one does, so the error count alone cannot distinguish the
two; the count of compiler command lines can.

The 36 matching lines are 36 distinct lines, not duplicates. They divide into two kinds, 18 of each,
one pair per compiled project:

| Line kind | Count |
|---|---|
| The `csc.exe` invocation line, beginning with the Roslyn compiler path | 18 |
| The `BuildResponseFile = '...'` echo of the same argument list | 18 |

18 projects compiled, which is the figure the floor of 18 was written against. No line carries an
`N>` node prefix in this log, so the `/m` node-prefix hazard that defeats anchored target counts does
not affect this unanchored search.
