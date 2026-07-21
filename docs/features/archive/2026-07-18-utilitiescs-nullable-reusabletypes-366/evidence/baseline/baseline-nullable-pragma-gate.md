# Phase 0 — Baseline Per-File Nullable Pragma Gate (P0-T5)

Timestamp: 2026-07-19T08-54

Command (plan's exact gate command):
`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`.)

Platform token note: the plan writes `/p:Platform="Any CPU"`, which is the SOLUTION-level platform
name. `UtilitiesCS.csproj`'s own PropertyGroup condition is `Debug|AnyCPU` (no space). A standalone
csproj build therefore requires `/p:Platform=AnyCPU`; passing `"Any CPU"` yields
`error : The BaseOutputPath/OutputPath property is not set` (no matching Configuration|Platform).
The solution build maps "Any CPU" -> "AnyCPU" internally; a direct csproj build does not. `AnyCPU`
is used here and in all per-batch gates.

EXIT_CODE: 1 (whole-build; see decomposition below)

## Output Summary

CS86xx (nullable) diagnostics attributable to `UtilitiesCS/ReusableTypeClasses/` at baseline: 0.
CS8714 diagnostics attributable to the cluster at baseline: 0.
This is expected: no ReusableTypeClasses file carries `#nullable enable` at baseline (greenfield),
so the per-file pragma emits no nullable diagnostics for the cluster.

The nonzero whole-build EXIT_CODE is caused entirely by PRE-EXISTING, out-of-scope, NON-nullable
warnings promoted to errors by `/p:TreatWarningsAsErrors=true`. None are in the ReusableTypeClasses
cluster. Decomposition:

1. Vendored `SVGControl` project (a `ProjectReference` of UtilitiesCS): 2x `CS0649`
   (`SvgImageSelector._relativeImagePath`, `_absoluteImagePath` never assigned). `/t:Rebuild`
   cascades Clean+Build to project references, recompiling SVGControl under TWAE. SVGControl builds
   clean WITHOUT TWAE. Pre-existing on `main`; out of scope (annotation-only feature does not touch
   vendored SVGControl).

2. UtilitiesCS's own non-cluster files: 14 unique `CS0168`/`CS0618` (unused variable; obsolete-API
   usage) in `AutoFile.cs`, `BayesianClassifierGroup.cs`, `BayesianSerializationHelper.cs`,
   `EmailFiler.cs`, `FolderExtraction.cs`, `IAsyncEnumerableExtensions.cs`, `IntelligenceConfig.cs`,
   `ManagerAsyncLazy.cs`, `SortEmail.cs`, `Triage.cs`. Zero in `ReusableTypeClasses/`. These surface
   only when UtilitiesCS actually compiles (i.e., when SVGControl is skipped); pre-existing;
   out of scope (fixing CS0168/CS0618 would be behavior/code changes, not nullable annotation).

## Measurement methodology for per-batch gates (mechanically necessary)

Because `/t:Rebuild` with the default `BuildProjectReferences=true` fails fast on SVGControl BEFORE
UtilitiesCS compiles, the plan's literal command yields NO CS86xx signal for the cluster. To obtain
the cluster CS86xx measurement the task acceptance requires, per-batch gates:
  (a) pre-build `SVGControl.csproj` clean (no TWAE) so `SVGControl.dll` is up-to-date, then
  (b) run `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
      so UtilitiesCS compiles and emits its diagnostics, then
  (c) filter the build log for diagnostics whose path is under `ReusableTypeClasses/`.
Success criterion per plan acceptance: zero CS86xx (and, for dictionary bases, zero CS8714)
attributed to the batch's ReusableTypeClasses files.

Baseline isolated-compile confirmation: `UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true
/p:BuildProjectReferences=false` compiled UtilitiesCS with 0 CS86xx / 0 CS8714 in the cluster
(only the 14 pre-existing non-cluster CS0168/CS0618 above).
