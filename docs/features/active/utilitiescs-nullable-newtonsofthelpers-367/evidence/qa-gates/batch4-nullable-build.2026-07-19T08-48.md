# Batch 4 Nullable Build Verification (P4-T6)

- Timestamp: 2026-07-19T08-48
- Opted-in files (4): `KnownTypesBinder.cs`, `AppGlobalsConverter.cs`, `PeopleScoRemainingObjectConverter.cs`, `NonRecursiveConverter.cs`

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in the 4 Batch 4 files (including the NonRecursiveConverter pragma normalization). CS86xx remains fatal, so EXIT 0 proves nullable-clean. Re-run after CSharpier normalized a modifier order in NonRecursiveConverter.cs — still EXIT 0.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked; edits confined to `NewtonsoftHelpers/`). Executed in full at P9-T3.

## Edits applied (annotation-only)

- `KnownTypesBinder.cs` (`ISerializationBinder`): `#nullable enable`; `KnownTypes` -> `IList<Type>?` (caller-populated); `BindToType` keeps NON-null `Type` return with behavior-preserving `!` and `// why` comment (a `Type?` return would be CS8766 against the interface; body returns `SingleOrDefault(...)` which is null on no match); `assemblyName` in-param -> `string?`; `BindToName` `out string? assemblyName` (set to null); `typeName` stays non-null (set to `serializedType.Name`).
- `AppGlobalsConverter.cs` (`JsonConverter<IApplicationGlobals>`): `#nullable enable`; `existingValue`/`value` -> `IApplicationGlobals?`; `ReadJson` return stays non-null `IApplicationGlobals` (body returns ctor-injected `_globals`, recorded deliberate); ctor parameter stays non-null.
- `PeopleScoRemainingObjectConverter.cs` (non-generic `JsonConverter`): `#nullable enable`; `existingValue`/`value` -> `object?`; `ReadJson` return -> `object?` (body `ToObject<PeopleScoRemainingObject>(serializer)` is nullable). Namespace `ToDoModel.Data_Model.People` unchanged.
- `NonRecursiveConverter.cs`: moved the existing mid-file `#nullable enable` (was line 27) to the TOP of the file so the whole file is opted in; the `object?` overrides are unchanged. CSharpier subsequently normalized `override sealed` -> `sealed override` (cosmetic, nullable-irrelevant). Pragma-move only; no new annotations.
