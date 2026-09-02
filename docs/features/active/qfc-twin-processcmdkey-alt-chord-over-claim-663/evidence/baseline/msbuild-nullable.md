# Phase 0 — Baseline type-check build ([P0-T10])

Timestamp: 2026-09-01T22-02

Command:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\663-nullable-baseline.msbuild.log;Verbosity=detailed"'
```

Only the `LogFile=` segment differs from the type-check gate command quoted in the plan's Toolchain command
forms section, as `[P0-T10]` authorises. The console stream was additionally redirected to
`coverage\663-nullable-baseline.console.txt` so the console-line searches below could be run over it; that
file lives under the gitignored `coverage` directory and was deleted after the readings were taken.

## Confirmation that `Nullable=enable` is absent

The command line recorded under `Command:` above contains **no occurrence of `Nullable=enable`**. The
switches passed are `/t:Rebuild`, `/m`, `/p:Configuration=Debug`, `/p:Platform=Any CPU`,
`/p:TreatWarningsAsErrors=true` and the `/flp:` file-logger switch. `/p:Nullable=enable` is deliberately
absent: no project in this repository carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property would be a solution-wide opt-in conscripting every file that has
never adopted the `#nullable enable` pragma.

EXIT_CODE: 0

## BASELINE_TYPECHECK_EXIT

**BASELINE_TYPECHECK_EXIT = 0**

Because it is zero, the `[P0-T10]` blocking branch does not apply. `[P4-T4]` retains its unmodified
exit-code-0 and `0 Error(s)` clauses, and no carry-forward disposition is created.
BASELINE_TYPECHECK_ERRORS is therefore not defined by this run.

## Non-vacuity observation — `Task "Csc"`

Occurrence count of the literal `Task "Csc"` in `coverage\663-nullable-baseline.msbuild.log`: **36**.

Greater than zero, so `CoreCompile` ran on this invocation rather than being skipped by incrementality.

## Error readings

Console lines matching `: error [A-Z]+[0-9]+:`: **0**.

MSBuild summary lines, verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the same codeless MSBuild-target warnings recorded in the `[P0-T9]` artifact — the
`System.Reactive.PackagesConfigCheck.targets` `packages.config` notice, emitted once per affected project.
They carry a bare `warning :` with no diagnostic identifier, which is why `/p:TreatWarningsAsErrors=true`
does not promote them: that switch promotes compiler and analyzer warnings, not warnings raised by an
MSBuild target's `<Warning>` task. No warning clause is asserted on this gate, per the plan.

## Log disposition

`coverage\663-nullable-baseline.msbuild.log` byte size before deletion: **10,697,463 bytes**.
`coverage\663-nullable-baseline.console.txt` byte size before deletion: **3,324,653 bytes**.

Both were deleted after these readings were taken.

Output Summary: The baseline type-check rebuild exited 0. BASELINE_TYPECHECK_EXIT is 0, so no
carry-forward disposition applies to `[P4-T4]`. The recorded command line contains no occurrence of
`Nullable=enable`. The detailed log contains 36 occurrences of the literal `Task "Csc"`. Zero console
lines match `: error [A-Z]+[0-9]+:` and the summary reports `0 Error(s)`. The detailed log and the console
capture were deleted after their byte sizes were recorded.
