# Baseline — Nullable TreatWarningsAsErrors Build State (Issue #181) — PROTECTED NO-REGRESSION REFERENCE

Timestamp: 2026-06-08T12-27
Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 1

Output Summary:
- Build FAILED. EXIT_CODE 1. 84 errors total, 0 warnings (all warnings promoted to errors under TreatWarningsAsErrors=true).
- ALL 84 errors are confined to the two VENDORED projects (excluded from this plan's scope):
  - UtilitiesSwordfish.NET.General.csproj: 50 errors
  - SVGControl.csproj: 34 errors
- ZERO errors in any of the 15 first-party projects.
- Error code distribution: CS8625 x26, CS8618 x26, CS8603 x9, CS8600 x8, CS8602 x6, CS8601 x5, CS0649 x2, CS8619 x1, CS8604 x1.
- These are pre-existing nullable-reference-type errors in vendored code; forcing /p:Nullable=enable on legacy vendored projects that were not authored under nullable context produces them. A full Rebuild is required to surface them (incremental build does not recompile cached assemblies).

NO-REGRESSION DEFINITION FOR THIS PLAN:
- The protected reference is: EXIT_CODE 1, 84 errors, all in UtilitiesSwordfish.NET.General.csproj (50) and SVGControl.csproj (34), zero first-party errors.
- A regression is ANY new error appearing in a first-party project, or any increase in error count attributable to first-party projects, after analyzer wiring (Phase 4). The vendored baseline error count is the fixed reference; the plan does not modify vendored projects.
- Because the analyzer rules added by this plan are set to `severity = suggestion` in .editorconfig before any analyzer wiring, they cannot be promoted to errors by TreatWarningsAsErrors=true and therefore cannot regress this gate.

Tool used: MSBuild 18.6.3 (.NET Framework).
