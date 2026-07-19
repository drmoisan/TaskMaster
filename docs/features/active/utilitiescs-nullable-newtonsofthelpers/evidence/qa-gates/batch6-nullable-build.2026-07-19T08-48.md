# Batch 6 Nullable Build Verification (P6-T6)

- Timestamp: 2026-07-19T08-48
- Opted-in files (3): `WrapperScoDictionary.cs`, `WrapperScDictionary.cs`, `WrapperPeopleScoDictionaryNew.cs`

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` across all three wrappers. EXIT 0 under a gate where CS86xx is fatal proves nullable-clean.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked). Executed in full at P9-T3.

## Edits applied (annotation-only, per-file decisions preserved, NO split)

Common to all three wrappers:
- `#nullable enable` at top (after the file BOM).
- `RemainingObject` `[JsonProperty]` public `object` -> `= null!` (deliberate per-file contract decision: preserves the non-null (de)serialization contract consumers rely on; the code guards it with `RemainingObject.ThrowIfNull()` before use; `// why` comment).
- Reflection null surfaces: `Type[] type_arguments = null` -> `Type[]?`; `(FieldInfo)instruction.Operand` / `(MethodInfo)instruction.Operand` -> `...!` (ILInstruction.Operand is now `object?`; these Ldfld/Stfld/Callvirt branches carry a non-null operand token — behavior-preserving `!`).

Per-file (NOT unified):
- `WrapperScDictionary.cs`: `ModifySetMethod` KEEPS its non-null `MethodBuilder` return (it throws on a null setter) — unchanged.
- `WrapperScoDictionary.cs`: `ModifySetMethod` return -> `MethodBuilder?` (this file `return null;` on no setter; the `ReplicateProperty` caller already null-checks with `if (setMethod is not null)`); `NormalizeEmptyDiskFilePaths(NewSmartSerializableConfig config)` param -> `NewSmartSerializableConfig?` (the method already null-guards internally, so the annotation matches its actual null-tolerant contract and resolves the CS8604 at the `configValue` call site). The `RemainingObject is JObject` / `JToken?` config path is handled by its existing `is not null` guards.
- `WrapperPeopleScoDictionaryNew.cs`: `ModifySetMethod` return -> `MethodBuilder?` (this file `return null;`); behavior-preserving `!` on the `GetCurrentMethod()!.DeclaringType!` static-logger initializer; the nested `PeopleScoRemainingObject` DTO's `[JsonProperty]` props (`Globals`, `Config`, `Name`) -> `= null!` (deserialization targets, non-null contract preserved). GLOBAL/namespace unchanged (namespace `ToDoModel.Data_Model.People`).

500-line pre-existing violations are flagged (P6-T4), NOT fixed; files NOT split.
