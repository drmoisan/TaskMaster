# Batch A Nullable Gate

Timestamp: 2026-07-19T01-45

Batch A files (6): ExtToChar.cs, CompilerServicesExtensions.cs, DrawingExtensions.cs, QueueExtensions.cs, IControlExtensions.cs, ExceptionExtensions.cs

Commands:
1. `dotnet tool run csharpier format UtilitiesCS/Extensions/` -> EXIT 0 ("Formatted 25 files").
2. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (overall build FAILED only on pre-existing non-nullable warnings; see below)

Output Summary:
- CS86xx (nullable) diagnostics: 0. All 6 Batch A files now carry `#nullable enable` and compile with zero nullable diagnostics under the pragma gate (AC1 satisfied for Batch A).
- Non-nullable warnings-as-errors: CS0168 x2 and CS0618 x28 — identical counts to baseline (P0-T5), confirming Batch A introduced no new non-nullable warnings and no new nullable diagnostics.
- Annotation notes: only ExceptionExtensions.cs required a code-level annotation — `StackTrace.GetFrame(0)` is annotated nullable, so `frame!.GetFileLineNumber()` (justified null-forgiving, commented) preserves the original NRE-on-empty-frames behavior without adding a new guard/return path. The other 5 files needed only the pragma (pure struct math / generic passthrough / commented-out body / attribute polyfill).
