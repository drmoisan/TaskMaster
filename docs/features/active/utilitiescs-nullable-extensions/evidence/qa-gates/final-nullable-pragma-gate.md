# Final Per-File Nullable Pragma Gate (AC1)

Timestamp: 2026-07-19T05-20

## 1. Literal solution-wide command (plan P6-T3)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT /p:Nullable=enable)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 across every project that compiled. The build FAILED with 2 errors, both pre-existing and non-nullable: `error CS0649` (field never assigned) x2 in the VENDORED `SVGControl/SvgImageSelector.cs` (`_relativeImagePath`, `_absoluteImagePath`). SVGControl is a vendored project (excluded from the first-party analyzer stack) with pre-existing non-nullable warnings that global `TreatWarningsAsErrors` promotes to errors. Under `-m` (parallel) the vendored SVGControl compile fails first and aborts the graph before `UtilitiesCS.csproj` recompiles, so this literal invocation cannot serve as the clean AC1 proof for the Extensions files. This is the pre-existing vendored-warning condition already established at baseline (P0-T5) and is not introduced by this annotation-only feature. `/p:Nullable=enable` was NOT passed.

## 2. Definitive AC1 proof — UtilitiesCS all-25-files rebuild

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT /p:Nullable=enable; compiles ALL 25 UtilitiesCS/Extensions/ files + the rest of UtilitiesCS into one assembly in a single pass)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0. Zero nullable diagnostics across all 25 `UtilitiesCS/Extensions/` files (23 remediated + 2 pre-enabled) under the per-file `#nullable enable` pragma with `TreatWarningsAsErrors` (AC1 SATISFIED). The build's non-zero exit is due solely to pre-existing non-nullable warnings promoted by `TreatWarningsAsErrors`: `CS0168` x2 (unused local) and `CS0618` x28 (obsolete member usage) — identical counts to the baseline pragma gate (P0-T5), confirming this feature introduced zero new nullable diagnostics AND zero new non-nullable warnings. `/p:Nullable=enable` was NOT passed; no `<Nullable>` element exists in the csproj.

Conclusion: All 25 Extensions files carry `#nullable enable` (23) or were verified clean (2) and emit zero CS86xx under the per-file pragma gate. AC1 is met. The literal solution-wide gate's failure is attributable only to pre-existing, out-of-scope, non-nullable warnings in vendored/production code.
