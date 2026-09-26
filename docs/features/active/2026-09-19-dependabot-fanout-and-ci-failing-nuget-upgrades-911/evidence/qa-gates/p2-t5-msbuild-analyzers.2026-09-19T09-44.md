# P2-T5 — Analyzer rebuild gate

Timestamp: 2026-09-19T15-18

Command: CMD-OUTLOOK, then CMD-MSBUILD-ANALYZERS.

```
pwsh -NoProfile -Command 'Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count'

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"
```

`msbuild` resolved to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, the path
`vswhere` reports, and was launched with the execution worktree as the working directory.

EXIT_CODE: 0

OUTLOOK-CLOSED: true

`Get-Process outlook` returned **0** immediately before the rebuild. Outlook was already closed and
was not terminated by this task, per the CMD-OUTLOOK rule.

## Terminal output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.59
```

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| `OUTLOOK-CLOSED: true` recorded | 0 Outlook processes | PASS |
| Exactly 0 lines containing `CS0006` in `coverage/analyzers.msbuild.log` | **0** | PASS |
| At least 18 lines containing `/out:obj\Debug\`, exact count recorded | **36** | PASS |

Log measured at `coverage/analyzers.msbuild.log`, 12311 lines. Lines containing `error CS` of any
number: **0**. Lines containing `: warning `: **0**.

## Non-vacuity

Per gate rule 7 the observation is taken from the echoed compiler command line, which MSBuild
writes under each project's `CoreCompile` heading at normal verbosity and which carries
`/out:obj\Debug\<Assembly>.dll`. `Task "Csc"` is a detailed-verbosity event and is not used.

The 36 matching lines resolve to **18 distinct assemblies**, which is every project in the
solution:

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

Each is echoed twice, which is the MSBuild file logger's known duplication of a message that also
reaches the console logger; the distinct count rather than the raw count is what establishes
coverage of the solution. The zero-error result therefore covers all 18 projects rather than a
subset, and a warm build that skipped every compile would have produced 0 such lines. `/t:Rebuild`
is used rather than `/t:Build` for exactly this reason: MSBuild's up-to-date check does not
invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile`
skipped everywhere and runs no analyzers.

The zero-warning result is the analyzer signal itself. `EnableNETAnalyzers` and
`EnforceCodeStyleInBuild` are both set, so a Roslyn or code-style diagnostic anywhere in the
solution would appear as a warning line; none does.

The log lands under `coverage/`, which `.gitignore:144` covers, so it never reaches a commit.

Output Summary: CMD-MSBUILD-ANALYZERS returned EXIT_CODE 0 with Outlook confirmed closed at 0
processes, `Build succeeded. 0 Warning(s) 0 Error(s)`. The 12311-line log carries **0** `CS0006`
lines and **36** `/out:obj\Debug\` lines resolving to all 18 solution assemblies, so the clean
analyzer result covers the whole solution rather than a skipped build.
