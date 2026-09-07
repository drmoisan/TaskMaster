# Phase 4 — Final type-check gate ([P4-T4])

Timestamp: 2026-09-01T23-06

Command, character-for-character the type-check gate command quoted in the plan's Toolchain command forms
section, with the `LogFile=` name the plan assigns to this task:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\663-nullable.msbuild.log;Verbosity=detailed"'
```

The console stream was redirected to `coverage\663-nullable.console.txt` for the console-line searches;
that file is under the gitignored `coverage` directory and was deleted after the readings were taken.

EXIT_CODE: 0

## Acceptance reading 1 — exit code and the error clauses

- Exit code: **0**, as required.
- Console lines matching `^\s*0 Error\(s\)$`: **1**. The summary line is present.
- Console lines matching `: error [A-Z]+[0-9]+:`: **0**.

BASELINE_TYPECHECK_EXIT recorded by `[P0-T10]` is 0, so the primary three clauses apply and the
carry-forward disposition does not.

## Acceptance reading 2 — `Nullable=enable` is absent from the recorded command

The `Command:` value transcribed above contains **no occurrence of `Nullable=enable`**. The switches
passed are `/t:Rebuild`, `/m`, `/p:Configuration=Debug`, `/p:Platform=Any CPU`,
`/p:TreatWarningsAsErrors=true` and the `/flp:` file-logger switch.

`/p:Nullable=enable` must never be added. No project in this repository carries a `<Nullable>` element and
there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file
which has never adopted the `#nullable enable` pragma. Omitting it loses no enforcement over any file that
has opted in.

## Acceptance reading 3 — `Task "Csc"` occurrence count

Occurrence count of the literal `Task "Csc"` in `coverage\663-nullable.msbuild.log`: **36**.

Greater than zero, so `CoreCompile` ran on this invocation.

## No warning clause is asserted on this gate

`/p:TreatWarningsAsErrors=true` promotes every compiler and analyzer warning to an error, so an exit-0 run
has by construction emitted no such warning anywhere in the solution. The exit-code, `^\s*0 Error\(s\)$`
and `: error [A-Z]+[0-9]+:` clauses already carry the whole assertion, and a warning clause here would
return the same value whatever was written.

## Build summary

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the codeless `System.Reactive.PackagesConfigCheck.targets` MSBuild-target notices.
They carry no diagnostic identifier, which is why `/p:TreatWarningsAsErrors=true` does not promote them:
that switch promotes compiler and analyzer warnings, not warnings raised by an MSBuild target's
`<Warning>` task. The same five appear in the `[P0-T10]` baseline.

## Log disposition

`coverage\663-nullable.msbuild.log` byte size before deletion: **10,600,961 bytes**.
`coverage\663-nullable.console.txt` byte size before deletion: **3,321,004 bytes**.

Both were deleted after these readings were taken.

Output Summary: The final type-check gate exited 0, printed a `0 Error(s)` summary, and produced zero
console lines matching `: error [A-Z]+[0-9]+:`. The recorded command line contains no occurrence of
`Nullable=enable`. The detailed log contains 36 occurrences of the literal `Task "Csc"`. The type-check
stage of the Phase 4 toolchain loop passes and rewrote no file.
