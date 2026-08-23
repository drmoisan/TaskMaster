# P0-T5 — Nullable Type-Check Baseline

Issue: #230
Task: [P0-T5]

## Plan-specified command (the gate of record)

- Timestamp: 2026-08-07T21-45
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  (invoked as `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -v:m`
  with the VS 18 Community full-framework MSBuild.)
- EXIT_CODE: 0
- Output Summary: Build succeeded. **0 errors.** All 20 projects report output,
  including `QuickFiler.dll`, `QuickFiler.Test.dll` and `TaskMaster.Test.dll`.
  This is the exact command form specified by CLAUDE.md § C# Toolchain step 3
  and by plan tasks P0-T5 / P8-T4, and it is the baseline the Phase 8 gate is
  compared against.

## Supplementary diagnostic probe (recorded for transparency)

Before running the plan command, a forced `-t:Rebuild` variant of the same
nullable gate was run to enumerate the diagnostics that the `/t:Build`
up-to-date check does not re-surface. It is recorded here as pre-existing
merge-base state, not as the gate result.

- Timestamp: 2026-08-07T21-40
- Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -v:m`
- EXIT_CODE: 1
- Output Summary: 195 nullable diagnostics promoted to errors, **all 195 confined
  to `UtilitiesCS.csproj`** (0 in `QuickFiler`, `QuickFiler.Test`, or any other
  project). Because `UtilitiesCS` is the first link in the dependency chain, its
  failure short-circuits the forced rebuild before downstream projects compile.

Breakdown by diagnostic:

| Count | Code |
|---|---|
| 130 | CS8766 (nullability of return type doesn't match implemented member) |
| 23 | CS8618 (non-nullable field uninitialized) |
| 12 | CS8625 (cannot convert null literal to non-nullable reference type) |
| 9 | CS8600 (converting null literal or possible null value) |
| 8 | CS8601 (possible null reference assignment) |
| 7 | CS8604 (possible null reference argument) |
| 3 | CS8602 (dereference of a possibly null reference) |
| 2 | CS8603 (possible null reference return) |
| 1 | CS8714 (type cannot be used as type parameter; `notnull` constraint) |

This is pre-existing `UtilitiesCS` nullable debt on the merge base. It is
**outside #230's scope** (#230 touches only `QuickFiler` and `QuickFiler.Test`)
and is not addressed by this feature. The restorative plain-Debug build was
re-run afterward (EXIT_CODE 0) so the binaries left on disk are the normal
Debug outputs, not the partial products of the failed forced rebuild.

## Interpretation for the Phase 8 gate

P8-T4 runs the same `/t:Build` form. Because `QuickFiler` and `QuickFiler.Test`
contribute 0 nullable diagnostics at baseline, any nullable error attributable to
this feature's edits will surface in P8-T4 and must be fixed before the loop can
exit.
