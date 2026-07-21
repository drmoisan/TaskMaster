# Final Nullable / Type-Check Gate (Issue #270)

Timestamp: 2026-07-07T22-50

Command (solution-level): `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (VS18 Community MSBuild 18.7.8)

EXIT_CODE: 1 (identical to the P0-T5 baseline; pre-existing vendored debt only)

## Baseline comparison (authoritative pass criterion)

Per the known pre-existing baseline (`evidence/baseline/typecheck-baseline.md`), forcing `Nullable=enable` on a global Rebuild fails at the vendored projects before the touched projects are reached. This change's gate passes because the vendored error set is byte-identical to the baseline and the touched files add zero new diagnostics.

Solution-level error tally (this run) — 84 errors, all attributed to vendored projects only:

| Error code | Count | Baseline count |
|---|---|---|
| CS8625 | 26 | 26 |
| CS8618 | 26 | 26 |
| CS8603 | 9 | 9 |
| CS8600 | 8 | 8 |
| CS8602 | 6 | 6 |
| CS8601 | 5 | 5 |
| CS0649 | 2 | 2 |
| CS8619 | 1 | 1 |
| CS8604 | 1 | 1 |
| **Total** | **84** | **84** |

Attribution: `SVGControl.csproj` = 34; `UtilitiesSwordfish.NET.General.csproj` = 50. Zero errors cite `AppEvents.ReadinessHookup.cs` or any `AppEventsTests` file. This is identical to the captured baseline.

## Touched-project verification (method 2)

Because the solution Rebuild halts at the vendored projects, a forced full nullable Rebuild of `TaskMaster.csproj` (`-t:Rebuild -p:Nullable=enable -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false`, against restored Debug dependency outputs) was run to surface any diagnostic on the touched production file. TaskMaster's project-wide genuine nullable debt (165 diagnostics across many files; the project has never been nullable-clean, consistent with the documented repo baseline) is unrelated to this change. Only two diagnostics cite `AppEvents.ReadinessHookup.cs`:

- `AppEvents.ReadinessHookup.cs(20,27): CS8625` — `OlToDoItems = null;`
- `AppEvents.ReadinessHookup.cs(21,27): CS8625` — `OlReminders = null;`

Both are inside the pre-existing `Unhook()` method (lines 18-23), which is entirely OUTSIDE this change's diff (the #270 hunk begins at line 60, confirmed via `git diff`). They are pre-existing debt, not introduced by #270.

Targeted incremental nullable build of `TaskMaster.csproj` and `TaskMaster.Test.csproj` (`-p:Nullable=enable -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false`, against restored Debug dependency DLLs): 0 CS errors each, EXIT_CODE 0.

## Conclusion

The touched files (production `AppEvents.ReadinessHookup.cs`, tests `AppEventsTests.cs` / `AppEventsTests.Helpers.cs` / `AppEventsCoverageExpansionTests.cs`) introduce zero new nullable diagnostics. The injectable-delegate seam's `?`-annotated properties are correct under nullable analysis. Vendored error set identical to baseline. Type-check stage passes for this change. Debug outputs were restored (`-t:Build -p:Configuration=Debug`) after the nullable Rebuild.
