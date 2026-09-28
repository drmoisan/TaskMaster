# P2-T6 — Nullable rebuild gate

Timestamp: 2026-09-19T15-24

Command: CMD-OUTLOOK, then CMD-MSBUILD-NULLABLE.

```
pwsh -NoProfile -Command 'Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count'

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"
```

`msbuild` resolved to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, launched
with the execution worktree as the working directory.

EXIT_CODE: 0

OUTLOOK-CLOSED: true

`Get-Process outlook` returned **0** immediately before the rebuild. Outlook was already closed and
was not terminated by this task.

## Terminal output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.11
```

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| `OUTLOOK-CLOSED: true` recorded | 0 Outlook processes | PASS |
| At least 18 lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log`, exact count recorded | **36** | PASS |

Log measured at `coverage/nullable.msbuild.log`, 12085 lines. Lines containing `error CS`: **0**.
Lines containing `CS86`: **0**, so no nullable-flow diagnostic was raised in any file carrying a
`#nullable enable` directive.

## Command shape

Two omissions in the command are load-bearing and were preserved exactly, per `CLAUDE.md` section
C#1.3 and the plan's Command Reference:

- **No `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and
  there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts
  every file which has never adopted the pragma. Nullable enforcement here is per-file opt-in via
  `#nullable enable`, and `/p:TreatWarningsAsErrors=true` promotes those files' `CS86xx`
  diagnostics to errors. CI omits the property deliberately.
- **`/t:Rebuild`, not `/t:Build`.** MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on
  every project and the gate cannot fail.

## Non-vacuity

Per gate rule 7 the observation is taken from the echoed compiler command line carrying
`/out:obj\Debug\<Assembly>.dll`, not from `Task "Csc"`. The 36 matching lines resolve to **18
distinct assemblies**, which is every project in the solution:

```
QuickFiler.dll            QuickFiler.Test.dll
SVGControl.dll            SVGControl.Test.dll
Tags.dll                  Tags.Test.dll
TaskMaster.dll            TaskMaster.Test.dll
TaskTree.dll              TaskTree.Test.dll
TaskVisualization.dll     TaskVisualization.Test.dll
ToDoModel.dll             ToDoModel.Test.dll
UtilitiesCS.dll           UtilitiesCS.Test.dll
VBFunctions.dll           VBFunctions.Test.dll
```

Each is echoed twice by the MSBuild file logger; the distinct count is what establishes that the
clean result covers the whole solution. This is the observation the `/t:Rebuild` requirement exists
to make possible: a skipped build would report 0 warnings, 0 errors and 0 such lines, and would be
indistinguishable from a passing gate on the exit code alone.

The log lands under `coverage/`, which `.gitignore:144` covers, so it never reaches a commit.

Output Summary: CMD-MSBUILD-NULLABLE returned EXIT_CODE 0 with Outlook confirmed closed at 0
processes, `Build succeeded. 0 Warning(s) 0 Error(s)`. The 12085-line log carries **36**
`/out:obj\Debug\` lines resolving to all 18 solution assemblies, and **0** lines matching `CS86`,
so every file that has opted into nullable analysis compiled clean under
`/p:TreatWarningsAsErrors=true`.
