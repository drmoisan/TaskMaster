# Phase 0 — Nullable Type-Check Baseline (P0-T7)

Timestamp: 2026-09-03T01-21
Task: [P0-T7]
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

MSBuild version 18.9.1+a81b43525 for .NET Framework, resolved through vswhere.

`/p:Nullable=enable` is deliberately NOT passed. This repository opts into nullable analysis per
file with `#nullable enable`; the solution-wide property conscripts every unannotated file and
produces hundreds of errors that CI does not see. `/t:Rebuild` is required because MSBuild's
up-to-date check does not invalidate on a command-line property change.

## Trailing counts printed by MSBuild

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.29
```

- Baseline warning count: **5**
- Baseline error count: **0**

P4-T6 is compared against these two numbers.

## Composition of the 5 warnings

Identical to the analyzer baseline: five instances of the System.Reactive `packages.config`
advisory, one each from `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`,
`UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj`. No `CS86xx` nullable-flow diagnostic is present
at baseline, so any that appears in the P4-T6 run is new and attributable to this change.

The counts are MSBuild's own trailing summary figures, not a raw whole-log grep count; the file
logger prints every warning twice, once inline and once in the summary.

Output Summary: Nullable rebuild succeeded with EXIT_CODE 0, 5 warnings and 0 errors. All five are
the System.Reactive packages.config advisory; there are no CS86xx nullable diagnostics in the
baseline.
