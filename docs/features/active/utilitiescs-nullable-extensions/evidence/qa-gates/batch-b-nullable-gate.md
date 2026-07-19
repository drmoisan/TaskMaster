# Batch B Nullable Gate

Timestamp: 2026-07-19T02-20

Batch B files (6): StringExtensions.cs, JsonExtensions.cs, JsonSerializerExtensions.cs, ImageExtensions.cs, StreamExtensions.cs, LazyExtension.cs

Commands:
1. `dotnet tool run csharpier format UtilitiesCS/Extensions/` -> EXIT 0.
2. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (overall build FAILED only on pre-existing non-nullable warnings)

Output Summary:
- CS86xx (nullable) diagnostics: 0. All 6 Batch B files now carry `#nullable enable` and compile with zero nullable diagnostics (AC1 satisfied for Batch B).
- Non-nullable warnings-as-errors: CS0168 x2, CS0618 x28 — unchanged from baseline; Batch B introduced no new diagnostics.
- Annotation notes:
  - StringExtensions.cs: `IsNullOrEmpty(this string? str)` — the null-testing method now accepts null, matching `string.IsNullOrEmpty(string?)` (honest, widening, behavior-compatible contract). Other Split/PadToCenter/FirstDiffIndex members dereference their receiver and stay non-null.
  - JsonExtensions.cs: `Deserialize<T>` return changed to `T?` because `... as T` can yield null (honest contract; widening, behavior-compatible). `ToJsonText` keeps its existing null guard.
  - ImageExtensions.cs: `ToByte` uses `converter.ConvertTo(...)!` (justified, commented) because `TypeConverter.ConvertTo` is annotated nullable; preserves the non-null byte[] contract with no new guard.
  - StreamExtensions.cs, LazyExtension.cs: pragma only. The `where T : struct` overloads in LazyExtension.cs were left free of reference-nullable annotations, per plan.
- struct-constrained overloads were not given reference-nullable annotations; no post-condition attribute added.
