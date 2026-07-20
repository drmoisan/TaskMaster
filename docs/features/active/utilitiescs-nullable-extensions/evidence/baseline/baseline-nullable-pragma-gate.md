# Baseline Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T01-05

Command (as run): `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (VS18 amd64 MSBuild; dash-switch form under MSYS_NO_PATHCONV=1; WITHOUT /p:Nullable=enable)

Command deviation note: The plan text names `/p:Platform="Any CPU"`. A single legacy csproj built directly (not via the .sln) resolves its configuration under `Platform=AnyCPU` (no space); the solution maps "Any CPU" -> "AnyCPU". `/p:BuildProjectReferences=false` uses the already-built vendored dependency DLLs (SVGControl, etc.) instead of rebuilding them under `TreatWarningsAsErrors`, since vendored projects are out of scope (excluded from the analyzer stack) and carry pre-existing non-nullable warnings. The semantic gate is preserved: a full Rebuild of UtilitiesCS under `TreatWarningsAsErrors=true` without `/p:Nullable=enable`.

EXIT_CODE: 1

Output Summary:
- CS86xx (nullable) diagnostics: 0. The two already-`#nullable enable` files (IAsyncEnumerableExtensions.cs, NullExtensions.cs) emit zero nullable diagnostics at baseline. This is the AC1 signal and it is clean.
- Build overall status: FAILED, but ONLY on pre-existing NON-nullable warnings that `TreatWarningsAsErrors` promotes to errors in UtilitiesCS production code: `error CS0168` (unused local) x2 and `error CS0618` (obsolete member usage) x28. These pre-date this feature, are not nullable diagnostics, and are outside this annotation-only feature's scope.
- Interpretation for this feature: the operative per-file pragma-gate metric is the CS86xx count. Baseline CS86xx = 0. Per-batch and final gates are evaluated on the same CS86xx-count metric (target 0), with the pre-existing non-nullable warnings-as-errors noted as orthogonal context.
