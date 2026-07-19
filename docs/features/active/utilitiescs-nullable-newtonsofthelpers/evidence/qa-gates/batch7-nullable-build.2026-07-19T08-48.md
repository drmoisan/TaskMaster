# Batch 7 Nullable Build Verification (P7-T6)

- Timestamp: 2026-07-19T08-48
- Opted-in files (3): `ScDictionaryConverter.cs`, `ScoDictionaryConverter.cs`, `PeopleScoConverter.cs` (in-scope copy only)

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in the 3 Batch 7 converters. Re-run after CSharpier reformatted the WriteJson signatures — still EXIT 0.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked). Executed in full at P9-T3.

## Edits applied (annotation-only; registered cross-module contracts)

- `ScDictionaryConverter.cs` (generic `JsonConverter<TDerived>`): `#nullable enable`; `existingValue` -> `TDerived?`; `ReadJson` return -> `TDerived?` (recorded registered cross-module contract; body `wrapper?.ToDerived()`); `WriteJson` `value` -> `TDerived?` with behavior-preserving `ToComposition(value!)`. `where TDerived : ScDictionary<...>` makes `TDerived?` valid. Non-null positions (`reader`, `writer`, `serializer`, `typeToConvert`) unchanged.
- `ScoDictionaryConverter.cs`: BOTH surfaces matched. Generic `JsonConverter<TDerived>` — `existingValue`/`value`/`ReadJson` return -> `TDerived?`, `ToComposition(value!)`. Inner non-generic `JsonConverter` — `existingValue`/`value`/`ReadJson` return -> `object?`, behavior-preserving `value!.GetType()` on the `WriteJson` deref. Namespace `UtilitiesCS.NewtonsoftHelpers.Sco` unchanged.
- `PeopleScoConverter.cs` (in-scope only, `JsonConverter<PeopleScoDictionaryNew>`): `#nullable enable`; `existingValue`/`value` -> `PeopleScoDictionaryNew?`; `ReadJson` return -> `PeopleScoDictionaryNew?` (finalized against the P7-T1 confirmation that this in-scope copy is the live/registered type); `ToComposition(value!)`. The out-of-scope commented-out `ToDoModel/Data Model/People/PeopleScoConverter.cs` was left untouched.
