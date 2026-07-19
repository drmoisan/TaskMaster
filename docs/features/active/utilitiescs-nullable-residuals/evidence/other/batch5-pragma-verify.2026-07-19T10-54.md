# Batch 5 Pragma Verification (P6-T5)

Timestamp: 2026-07-19T10-54

Batch 5 opted-in files (4, EmailIntelligence data types):
1. UtilitiesCS/EmailIntelligence/FilterEntry.cs — `_description = null!` (the (name, folders, categoryList)
   ctor does not assign it; behavior-preserving, keeps the current null).
2. UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs — `Config { ... } = null!`; `LastResourceTimingBreakdown` → `string?`;
   filtered KVP value `null` → `null!`; `DeserializeLoaderAsync(string? serializedLoader)` (see deviation).
   `ResourceTimingRow` left as the existing plain `readonly struct` (no record/init).
3. UtilitiesCS/EmailIntelligence/Evaluation/FolderPredictorEvaluator.cs — `PredictTop` return `string?`;
   justified `!` at guaranteed-non-null sites (`trueLeaf!` on `leaves.Add`/`Increment` x3, `example!.Tokens`),
   because `string.IsNullOrEmpty` does not refine null-state on net481.
4. UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs — `Globals = null!`, `_prefix = null!`;
   `AddMissingEntry` and `RefineValidateCategory` returns → `string?`; existing `[ExcludeFromCodeCoverage]`
   members unchanged. See CS8644 deviation.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. First
pass surfaced 2 CS8602, 4 CS8604 and 22 CS8644; all resolved annotation-only as described below.

## Deviations from the plan's suggested annotation
1. IntelligenceConfig `DeserializeLoaderAsync` parameter changed to `string?` (plan listed only
   Config/LastResourceTimingBreakdown/filtered-KVP). Reason: the resource-map value `kvp.Value` is
   genuinely nullable and is passed to `DeserializeLoaderAsync` unconditionally (line 104 already
   null-checks it for the size measurement). `string?` is the accurate, behavior-preserving
   annotation; the downstream `SmartSerializableLoader.DeserializeAsync` tolerates it (build clean).
   No behavior change.
2. PeopleScoDictionaryNew.AddMissingEntry local `var newPerson` changed to `string? newPerson`
   (mechanically required because `RefineValidateCategory` now returns `string?` and is assigned to
   it, and the existing `if (newPerson is null) return null;` path is preserved). Behavior-preserving.
3. PeopleScoDictionaryNew class-declaration line placed in a `#nullable disable` region (member bodies
   remain `#nullable enable`). Reason: 22 CS8644 "interface implemented by the base type doesn't match"
   arose from deriving the still-oblivious #366 base `ScoDictionaryNew<,>`/`ConcurrentObservableDictionary<,>`
   while re-listing `IPeopleScoDictionaryNew` in a nullable context. CS8644 is not resolvable with
   `?`/`!`/`= null!`, and #366's base classes are out of scope for this child (spec.md Maintainer
   Decisions item 5, undeclared #366 edge). Using only `#nullable enable`/`#nullable disable` region
   pragmas — the same inline-island technique the plan sanctions for MeetingItemHelper (P9-T2) and the
   epic's per-region architecture — keeps the inherited interface check oblivious while the class's own
   members (Globals/_prefix CS8618, AddMissingEntry/RefineValidateCategory CS8603, etc.) stay fully
   nullable-checked. No warning-ID suppression, no `WarningsNotAsErrors` change, no behavior change.
