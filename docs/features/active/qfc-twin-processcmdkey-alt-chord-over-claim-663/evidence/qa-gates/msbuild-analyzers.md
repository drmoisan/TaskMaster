# Phase 4 — Final analyzer gate ([P4-T3])

Timestamp: 2026-09-01T23-03

Command, character-for-character the analyzer gate command quoted in the plan's Toolchain command forms
section, with the `LogFile=` name the plan assigns to this task:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\663-analyzers.msbuild.log;Verbosity=detailed"'
```

The console stream was redirected to `coverage\663-analyzers.console.txt` for the console-line searches;
that file is under the gitignored `coverage` directory and was deleted after the readings were taken.

EXIT_CODE: 0

## Acceptance reading 1 — exit code and the error clauses

- Exit code: **0**, as required.
- Console lines matching `^\s*0 Error\(s\)$`: **1**. The summary line is present.
- Console lines matching the MSBuild diagnostic form `: error [A-Z]+[0-9]+:`: **0**.

BASELINE_ANALYZER_EXIT recorded by `[P0-T9]` is 0, so the primary three clauses apply and the
carry-forward disposition does not.

A bare search for the word `error` is deliberately not used: a successful MSBuild run prints the
`/errorreport:prompt` token on every Csc command line and prints its own `0 Error(s)` summary, so that
search matches on a clean run and the gate could never pass.

## Acceptance reading 2 — `Task "Csc"` occurrence count

Occurrence count of the literal `Task "Csc"` in `coverage\663-analyzers.msbuild.log`: **36**.

Greater than zero, so `CoreCompile` ran and the gate is not vacuous. `/t:Rebuild` is load-bearing here:
MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would
return exit 0 with `CoreCompile` skipped on every project and would run no analyzers.

## Acceptance reading 3 — warning-pair comparison against BASELINE_WARNINGS

Console lines matching `: warning [A-Z]+[0-9]+:` anywhere in the console output: **0**.
Of those, lines naming `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs`: **0**.

Derived `(source file name, diagnostic identifier)` pair set for this run: **{ } (the empty set).**
BASELINE_WARNINGS pair set from `[P0-T9]`: **{ } (the empty set).**

The two sets are **equal**, using the same pair comparison `[P1-T3]` used.

The comparison is baseline-relative rather than absolute because
`.github/workflows/_build-analyzers.yml` lines 50 through 52 run this command with no
`/p:TreatWarningsAsErrors=true`, so an analyzer warning naming one of these files can already exist on
`origin/main`. BASELINE_WARNINGS is empty on this tree, so the two formulations coincide here.

## Build summary

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the codeless `System.Reactive.PackagesConfigCheck.targets` MSBuild-target notices
recorded in the `[P0-T9]` artifact. None carries a diagnostic identifier and none names any of the three
changed files.

## Log disposition

`coverage\663-analyzers.msbuild.log` byte size before deletion: **10,617,569 bytes**.
`coverage\663-analyzers.console.txt` byte size before deletion: **3,318,712 bytes**.

Both were deleted after these readings were taken.

Output Summary: The final analyzer gate exited 0, printed a `0 Error(s)` summary, and produced zero
console lines matching `: error [A-Z]+[0-9]+:`. The detailed log contains 36 occurrences of the literal
`Task "Csc"`, so compilation ran. The `(source file name, diagnostic identifier)` pair set for warnings
naming the three changed files is empty and equals BASELINE_WARNINGS. The lint stage of the Phase 4
toolchain loop passes and rewrote no file.
