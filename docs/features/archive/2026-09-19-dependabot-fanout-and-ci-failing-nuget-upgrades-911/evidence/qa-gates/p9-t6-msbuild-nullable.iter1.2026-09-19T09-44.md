# P9-T6 — C# QA step 3, solution-wide nullable rebuild (iteration 1)

Timestamp: 2026-09-20T09-44

Command: CMD-OUTLOOK, then CMD-MSBUILD-NULLABLE.

```
pwsh -NoProfile -Command 'Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count'

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"
```

`msbuild` resolved to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` and was
launched with the execution worktree as the working directory, set by `Set-Location` per gate
rule 16.

`/p:Nullable=enable` was **not** added and `/t:Build` was **not** substituted. Both omissions are
load-bearing and are recorded in `CLAUDE.md` section C#1.3: the property is a solution-wide opt-in
that conscripts every file which has never adopted the pragma, and a warm `/t:Build` returns exit 0
with `CoreCompile` skipped on every project, so the gate could not fail.

EXIT_CODE: 0

OUTLOOK-CLOSED: true

`Get-Process outlook` returned **0** immediately before the rebuild. Outlook was already closed and
was **not** terminated by this task.

## Terminal output, tail

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.42
```

## Log measurements

The file logger wrote `coverage/nullable.msbuild.log`, 12154 lines, at `Verbosity=normal`. The path
is under `coverage/`, which `.gitignore:144` covers, so the log never reaches a commit.

| Measurement | Value |
|---|---|
| Lines containing `/out:obj\Debug\` | **36** |
| Of which `csc.exe` invocation lines | 18 |
| Of which `BuildResponseFile = '...'` echoes | 18 |
| Lines matching a `CS86xx` nullable diagnostic | 0 |

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `OUTLOOK-CLOSED: true` recorded | yes | recorded above | PASS |
| Lines containing `/out:obj\Debug\` | at least 18, exact count recorded | **36** | PASS |

The 36 matching lines are 36 distinct lines, one `csc.exe` invocation and one response-file echo per
compiled project, so 18 projects compiled. The count is the non-vacuity observation: a rebuild whose
compile targets were skipped would report the same zero errors and would show no compiler command
line at all.

Zero `CS86xx` diagnostics appear, which is the expected result of a per-file opt-in nullable regime
in which this change adds no `#nullable enable` directive and modifies no `.cs` file. The figure is
recorded as an observation and is not an acceptance clause of this task.
