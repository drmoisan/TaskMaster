# Phase 0 — Baseline analyzer build ([P0-T9])

Timestamp: 2026-09-01T22-00

Command:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\663-analyzers-baseline.msbuild.log;Verbosity=detailed"'
```

Only the `LogFile=` segment differs from the analyzer gate command quoted in the plan's Toolchain command
forms section, as `[P0-T9]` authorises. The console stream was additionally redirected to
`coverage\663-analyzers-baseline.console.txt` so that the console-line searches below could be run over
it; that file lives under the gitignored `coverage` directory and was deleted after the readings were
taken.

EXIT_CODE: 0

## BASELINE_ANALYZER_EXIT

**BASELINE_ANALYZER_EXIT = 0**

Because it is zero, the `[P0-T9]` blocking branch does not apply. `[P1-T3]` and `[P4-T3]` retain their
unmodified exit-code-0 clauses, and no carry-forward disposition is created. BASELINE_ANALYZER_ERRORS is
therefore not defined by this run.

## Non-vacuity observation — `Task "Csc"`

Occurrence count of the literal `Task "Csc"` in `coverage\663-analyzers-baseline.msbuild.log`: **36**.

The count is greater than zero, so `CoreCompile` actually ran rather than being skipped by MSBuild
incrementality, and the literal `Task "Csc"` is confirmed to be the one MSBuild emits at detailed
verbosity. No literal substitution into `[P0-T10]`, `[P1-T3]`, `[P4-T3]` or `[P4-T4]` is required; those
tasks use `Task "Csc"` unchanged.

## Error readings

Console lines matching `: error [A-Z]+[0-9]+:`: **0**.

MSBuild error summary line, verbatim:

```
    0 Error(s)
```

## BASELINE_WARNINGS

Console lines matching `: warning [A-Z]+[0-9]+:` anywhere in the console output: **0**.
Of those, lines naming `QfcFormKeyHandler.cs`, `QfcFormViewer.cs` or `QfcFormKeyHandlerTests.cs`: **0**.

**BASELINE_WARNINGS = { } (the empty set).**

Derived `(source file name, diagnostic identifier)` pairs: **{ } (the empty set).**

This is a measured reading, not a prediction. `[P1-T3]` and `[P4-T3]` each derive the same set from their
own console output and compare it against this empty set.

### The build's five warnings, recorded for completeness

The build's summary reports `5 Warning(s)`. None of them matches the pattern
`: warning [A-Z]+[0-9]+:`, because each carries a bare `warning :` with no diagnostic identifier, and none
names any of the three files this plan changes. All five are the same MSBuild-target warning, emitted once
per project, with the worktree root rendered as `<repo-root>`:

```
<repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [<repo-root>\QuickFiler\QuickFiler.csproj]
<repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : ... [<repo-root>\TaskMaster\TaskMaster.csproj]
<repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : ... [<repo-root>\ToDoModel\ToDoModel.csproj]
<repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : ... [<repo-root>\UtilitiesCS\UtilitiesCS.csproj]
<repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : ... [<repo-root>\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
```

The message body is identical on all five lines and is elided with `...` after the first, which is quoted
in full.

## Log disposition

`coverage\663-analyzers-baseline.msbuild.log` byte size before deletion: **9,813,685 bytes**.
`coverage\663-analyzers-baseline.console.txt` byte size before deletion: **2,632,180 bytes** (5,161 lines).

Both were deleted after these readings were taken, so no multi-megabyte machine artifact is committed.

Elapsed build time, verbatim: `Time Elapsed 00:00:13.35`.

Output Summary: The baseline analyzer rebuild exited 0. BASELINE_ANALYZER_EXIT is 0, so no carry-forward
disposition applies to `[P1-T3]` or `[P4-T3]`. The detailed log contains 36 occurrences of the literal
`Task "Csc"`, so compilation genuinely ran. Zero console lines match `: error [A-Z]+[0-9]+:` and the
summary reports `0 Error(s)`. BASELINE_WARNINGS is the empty set: no console line matching
`: warning [A-Z]+[0-9]+:` exists at all, and therefore none names any of the three changed files. The
detailed log and the console capture were deleted after their byte sizes were recorded.
