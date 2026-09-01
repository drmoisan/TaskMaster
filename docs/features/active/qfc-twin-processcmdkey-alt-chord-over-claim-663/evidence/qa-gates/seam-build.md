# Phase 1 — Seam analyzer build ([P1-T3])

Timestamp: 2026-09-01T22-37

This gate exists because routing the viewer through the new predicate removes the last compiled consumer
of `IsAltKeyCommand`, which could trip an unused-member diagnostic.

Command:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\663-analyzers-seam.msbuild.log;Verbosity=detailed"'
```

Only the `LogFile=` segment differs from the analyzer gate command quoted in the plan's Toolchain command
forms section, as `[P1-T3]` authorises. The console stream was additionally redirected to
`coverage\663-analyzers-seam.console.txt` for the console-line searches; that file is under the gitignored
`coverage` directory and was deleted after the readings were taken.

EXIT_CODE: 0

## Acceptance reading 1 — exit code

Exit code is **0**, satisfying the primary clause. BASELINE_ANALYZER_EXIT recorded by `[P0-T9]` is also 0,
so no carry-forward disposition applies and the primary clause is the one in force.

## Acceptance reading 2 — `Task "Csc"` occurrence count

Occurrence count of the literal `Task "Csc"` in `coverage\663-analyzers-seam.msbuild.log`: **36**.

Greater than zero, so compilation actually ran and the gate is not vacuous. The count matches the 36
recorded by `[P0-T9]`, which is expected: the project set is unchanged.

## Acceptance reading 3 — warning-pair comparison against BASELINE_WARNINGS

Console lines matching `: warning [A-Z]+[0-9]+:` anywhere in the console output: **0**.
Of those, lines naming `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs`: **0**.

Derived `(source file name, diagnostic identifier)` pair set for this run: **{ } (the empty set).**

BASELINE_WARNINGS pair set from `[P0-T9]`: **{ } (the empty set).**

The two sets are **equal**. The comparison is on pairs rather than on whole lines because `[P1-T2]`
shifts the line and column numbers a verbatim warning line would carry.

No new diagnostic appeared, so no root-diagnostic fix was required. `IsAltKeyCommand` was not deleted; it
retains four test consumers in `QuickFiler.Test`, and AC-8 requires it to survive unchanged.

## Error readings

Console lines matching `: error [A-Z]+[0-9]+:`: **0**.

MSBuild summary lines, verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the same codeless `System.Reactive.PackagesConfigCheck.targets` MSBuild-target
notices recorded in the `[P0-T9]` artifact. None carries a diagnostic identifier, so none matches the
`: warning [A-Z]+[0-9]+:` pattern, and none names any of the three changed files.

## Log disposition

`coverage\663-analyzers-seam.msbuild.log` byte size before deletion: **10,575,219 bytes**.
`coverage\663-analyzers-seam.console.txt` byte size before deletion: **3,332,560 bytes**.

Both were deleted after these readings were taken.

Output Summary: The seam analyzer rebuild exited 0 with 36 occurrences of `Task "Csc"` in its detailed
log, zero console lines matching `: error [A-Z]+[0-9]+:`, and a `0 Error(s)` summary. The
`(source file name, diagnostic identifier)` pair set derived from warning lines naming the three changed
files is empty and equals BASELINE_WARNINGS. Removing the last compiled consumer of `IsAltKeyCommand`
introduced no analyzer diagnostic.
