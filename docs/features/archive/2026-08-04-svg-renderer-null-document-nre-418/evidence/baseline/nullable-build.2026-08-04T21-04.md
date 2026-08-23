# [P0-T8] Baseline Nullable / Type-Check State — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T8]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)
MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, `18.8.2+ce25c0108 for .NET Framework`

## Command (plan-commanded, authoritative)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
```

EXIT_CODE: 0

## Output Summary

Build succeeded. **0 errors, 5 warnings.** Elapsed 00:00:00.92.

The VSTO premise holds: **zero `CS0234` and zero `MSB3245`** occurrences in any project. The failure
mode recorded in `nullable-build.2026-08-04T14-36.md` (four `CS0234` in
`TaskMaster/ThisAddIn.Designer.cs` plus `MSB3245`) does not reproduce on this host.

### Diagnostic inventory — plan-commanded incremental run

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) `System.Reactive.PackagesConfigCheck.targets(31,5)` | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` |
| — | error | none | — |

Distinct diagnostic codes: one code-less MSBuild warning from
`System.Reactive.PackagesConfigCheck.targets`. Zero `CS86xx` nullable diagnostics were emitted.

### Incrementality caveat — this EXIT_CODE 0 is not a statement about nullable cleanliness

The plan-commanded run used MSBuild target `Build` and executed **0** `CoreCompile` targets: every
project was already up to date from the preceding analyzer build. Legacy non-SDK up-to-date checks
are timestamp-based, not property-based, so `/p:Nullable=enable /p:TreatWarningsAsErrors=true` did
not trigger a recompile and no source file was re-analyzed. The `EXIT_CODE: 0` above is therefore a
true record of what the plan-commanded command returns in this tree state, but it is **not** evidence
that the solution is free of nullable diagnostics.

A supplementary run with the **identical property set** and target `Rebuild` was executed solely to
produce the complete inventory that tasks P1-T7 and P2-T5 need as a comparison basis. It is
supplementary evidence; it is not the plan command and does not replace the result above.

Supplementary command:
```
MSBuild.exe TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /m
```
Supplementary EXIT_CODE: 1 — **Build FAILED. 196 errors, 2 warnings.** Elapsed 00:00:03.59.

Complete diagnostic inventory at full-recompile scope:

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 130 | error | `CS8766` | `UtilitiesCS.csproj` |
| 23 | error | `CS8618` | `UtilitiesCS.csproj` |
| 12 | error | `CS8625` | `UtilitiesCS.csproj` |
| 9 | error | `CS8600` | `UtilitiesCS.csproj` |
| 8 | error | `CS8601` | `UtilitiesCS.csproj` |
| 7 | error | `CS8604` | `UtilitiesCS.csproj` |
| 3 | error | `CS8602` | `UtilitiesCS.csproj` |
| 2 | error | `CS8603` | `UtilitiesCS.csproj` |
| 1 | error | `CS8714` | `UtilitiesCS.csproj` |
| 1 | error | `CS8630` | `SVGControl.Test.csproj` |
| 2 | warning | `MSB3061` | `TaskMaster.csproj` |

Totals: `UtilitiesCS.csproj` 195 errors, `SVGControl.Test.csproj` 1 error, sum 196 — matching the
MSBuild summary count exactly.

Distinct diagnostic codes present in the baseline at full-recompile scope: `CS8766`, `CS8618`,
`CS8625`, `CS8600`, `CS8601`, `CS8604`, `CS8602`, `CS8603`, `CS8714`, `CS8630`, `MSB3061`.

Projects reported `FAILED` in the supplementary run (cascade from `UtilitiesCS` and
`SVGControl.Test`): `TaskMaster.sln`, `UtilitiesCS`, `SVGControl.Test`, `Tags`, `TaskTree`,
`TaskVisualization`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and the seven dependent test projects
`TaskTree.Test`, `QuickFiler.Test`, `VBFunctions.Test`, `UtilitiesCS.Test`, `TaskMaster.Test`,
`ToDoModel.Test`, `TaskVisualization.Test`.

### The one `SVGControl.Test` baseline diagnostic — material for task P1-T7

```
error CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3. Please use language version '8.0' or greater.
```

Emitted by `SVGControl.Test.csproj`. This error is **present in the baseline** and is a property of
the project's C# language version, not of any Phase 1 edit. It is not "newly introduced by bringing
`SVGControl.Test` into the solution gate", because `SVGControl.Test` is already a solution member at
this baseline (commit `0162567d`, task P1-T1). Task P1-T7 must not attribute `CS8630` to its own
changes.

### `UtilitiesCS` nullable debt is pre-existing and out of scope

The 195 `UtilitiesCS` errors are pre-existing repository nullable debt tracked outside issue #418.
Nothing in the issue #418 Scope Lock permits editing `UtilitiesCS`. A full-recompile
`Nullable=enable` + `TreatWarningsAsErrors` solution build therefore cannot reach `EXIT_CODE: 0` at
this baseline, independent of any change this feature makes.

Consequence for tasks P1-T7 and P2-T5: the plan-commanded `Build`-target command is the gate, and
its baseline result is `EXIT_CODE: 0`. Any `CS86xx` code appearing in `SVGControl` or
`SVGControl.Test` after the Phase 1 edits, **other than `CS8630` in `SVGControl.Test`**, was absent
from this baseline and must be treated as newly introduced. Any `CS86xx` code in `UtilitiesCS` from
the list above was present at baseline.
