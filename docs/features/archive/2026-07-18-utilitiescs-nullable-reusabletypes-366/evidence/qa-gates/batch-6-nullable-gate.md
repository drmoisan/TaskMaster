# Batch 6 — Nullable Pragma Gate (P6-T3 / P6-T4)

Timestamp: 2026-07-19T19-37

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (clean; only the 8 in-flight
   source files modified, no unrelated churn).
2. Pre-build vendored `SVGControl.csproj` clean (no TWAE) so `SVGControl.dll` is up-to-date —
   EXIT_CODE 0.
3. Pragma gate (isolated-compile methodology per P0-T5):
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
   (WITHOUT `/p:Nullable=enable`).

EXIT_CODE: 1 (whole-assembly build; nonzero is caused entirely by the same PRE-EXISTING,
out-of-scope non-nullable warnings-as-errors documented at baseline P0-T5, decomposed below).

## Output Summary

Batch 6 (5 files: `ConcurrentObservableBag`, `ConcurrentObservableCollection`,
`ConcurrentObservableCollection.Serialization`, `ConcurrentObservableDictionary`,
`ObservableDictionary`) cluster diagnostics:
- CS86xx (nullable) count attributed to `ReusableTypeClasses/`: 0 (AC1 for Batch 6)
- CS8714 count attributed to the cluster: 0

Ratified `where TKey : notnull` constraint applied in this batch (P6-T2 RATIFIED):
- `ConcurrentObservableDictionary<TKey, TValue>` (the sole Batch 6 dictionary base).
- `ConcurrentObservableBag`/`ScBag` (`ConcurrentBag<T>`-based) were NOT constrained.

Cross-child scope waiver (epic-authorized, Option A extended to two files): exactly one
`where TKey : notnull` line was added to EACH of the two #367-owned NewtonsoftHelpers consumers
that otherwise emit CS8714 once `ConcurrentObservableDictionary` is constrained, and nothing else:
- `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` — `WrapperScoDictionary<TDerived, TKey, TValue>`.
- `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` — `ScoDictionaryConverter<TDerived, TKey, TValue>`.

Cross-child cascade bound (verified): after the constraint + the two waiver lines, the whole-assembly
isolated compile emits ZERO CS8714 anywhere in UtilitiesCS. No THIRD cross-child consumer surfaced;
`PeopleScoConverter.cs` and `WrapperPeopleScoDictionaryNew.cs` are unaffected and were not edited.

Whole-assembly error decomposition (unchanged from P0-T5 baseline; all pre-existing / out of scope):
- `error CS0618` (obsolete-API usage): 28 occurrences — pre-existing non-cluster files.
- `error CS0168` (unused variable): 2 occurrences — pre-existing non-cluster files.
- Zero errors and zero warnings originate in any `ReusableTypeClasses/` Batch 6 file or the two
  waiver files. No `System.Diagnostics.CodeAnalysis` post-condition attribute was added.
- `/p:Nullable=enable` was NOT passed.
