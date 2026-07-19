# Batch 5 Nullable Build Verification (P5-T3)

- Timestamp: 2026-07-19T08-48
- Opted-in file (1): `UtilitiesCS/NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionary.cs`

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in the Batch 5 file. EXIT 0 under a gate where CS86xx is fatal proves nullable-clean.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked). Executed in full at P9-T3.

## Edits applied (annotation-only, no behavior change)

- `#nullable enable` at top.
- Resolved CS8618 on the four non-null auto-props not set on every ctor path (`ConcurrentDictionary`, `RemainingObject`, `AdditionalFields`, `AdditionalProperties`) with `= null!` and a `// why` comment (populated by `ToComposition`/`ToCompositionOld` before the derived-conversion members read them; preserves the existing non-null contract without adding consumer guards).
- Widened the reflection-populated `Dictionary<string, object>` fields (`AdditionalFields`, `AdditionalProperties`) to `Dictionary<string, object?>` (honest — `field.GetValue`/`property.GetValue` can return null), resolving the `.Add(..., objectValue)` CS8604.
- `(TDerived)Activator.CreateInstance(typeof(TDerived), true)` -> `...!` (CS8600 unbox/cast; behavior-preserving).
- `Activator.CreateInstance(newClassType)` -> `...!` (CS8600; `newClassType` is a valid emitted type).
- `typeBuilder.CreateTypeInfo().AsType()` -> `typeBuilder.CreateTypeInfo()!.AsType()` (CS8602; `CreateTypeInfo()` returns `TypeInfo?`).
- The `GetField`/`GetProperty` reflection results remain guarded by the existing `!= null` checks.
