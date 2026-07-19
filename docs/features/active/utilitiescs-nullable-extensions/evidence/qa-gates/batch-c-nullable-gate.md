# Batch C Nullable Gate

Timestamp: 2026-07-19T03-10

Batch C files (4): IEnumerableExtensions.cs, ArrayExtensions.cs, IListExtensions.cs, DictionaryExtensions.cs

Commands:
1. `dotnet tool run csharpier format UtilitiesCS/Extensions/` -> EXIT 0.
2. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (overall build FAILED only on pre-existing non-nullable warnings)

Output Summary:
- CS86xx (nullable) diagnostics: 0 (after one fix pass that cleared 5 CS8602 sites). All 4 Batch C files carry `#nullable enable` and compile with zero nullable diagnostics (AC1 satisfied for Batch C). `ArrayExtensions.cs` was NOT split (remains one 561-line file, pre-existing size condition).
- Non-nullable warnings-as-errors: CS0168 x2, CS0618 x28 — unchanged from baseline.
- Cross-module contract annotations (consumed by Batch E and Wave-1):
  - IListExtensions.cs: `Find<T>` -> `T?`; `TryFindMax` `out T? max`; `CompareTo` params `IList<T>?`; `IsNullOrEmpty(this IList<string>?)`; `Split` params `IList<T>?`/`IEqualityComparer<T>?`; `TryAddRange` params `IList<T>?`/`IEnumerable<T>?`.
  - DictionaryExtensions.cs: `UpdateOrRemove` `out TValue?`; `ContentEquals` params `Dictionary<TKey,TValue>?`.
  - IEnumerableExtensions.cs: `CastNullSafe` local `IEnumerable<TResult>?`; iterator `default(TResult)!` (intentional null-substitution, commented); `CompareTo`/`IsSubsetOf` params `IEnumerable<T>?`; `ToList` `Action<int>? onItemCompleted`; `WithProgressReporting` `Stopwatch? sw`; `SelectGroup` `x.Key!.Equals(key)` (justified).
  - ArrayExtensions.cs: `TryFlattenArrayTree` -> `T[]?`; internal `FlattenArrayTree(bool strict)` -> `List<T>?`; `ToString()` element derefs use justified `!` (commented); `default(T)!` null-substitution in the tree walk. `SliceColumn`/`To2D`/`ToStringArray` public signatures unchanged (pragma only where no diagnostic).
- No post-condition attribute added; unconstrained-generic null-state expressed via `out TValue?`/`T?`, not `[MaybeNullWhen]`.
