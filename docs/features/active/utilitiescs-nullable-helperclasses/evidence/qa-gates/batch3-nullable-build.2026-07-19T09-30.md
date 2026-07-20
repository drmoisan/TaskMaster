# Batch 3 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-30
- Task: [P3-T6]

## Opted-in files (4, CloningFunctions + reflection)

- `UtilitiesCS/HelperClasses/CloningFunctions/DeepCompare.cs` — `DeepDifferences<T>` element contract annotated `List<(string, object?, object?)>` (reflection `GetValue` results); behavior-preserving `!` on the `throwIfNotFound: true` GetType dereference.
- `UtilitiesCS/HelperClasses/CloningFunctions/ObjectCopier.cs` — deliberate downstream contract: `Clone<T>` returns `T?` (null-source `return default` path; `(T?)formatter.Deserialize(...)`).
- `UtilitiesCS/HelperClasses/CloningFunctions/DispatchUtility.cs` — public `GetType` returns `Type?` (null when `!throwIfNotFound`); both `Invoke` overloads return `object?`; the private `IDispatchInfo.GetTypeInfo` `out Type? typeInfo` reflects the only-set-when-count==1 behavior; annotation-only, marshaling unchanged.
- `UtilitiesCS/HelperClasses/ReflectionHelper.cs` — `ReflectionTypeLoadException.Types` (`Type?[]`) filtered/projected to non-null; `AssemblyName.Name!`; `CollectTypes(object? obj, ...)`; internal `Type? current` locals for `BaseType` walks (public extension signatures unchanged).

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 3.
- Result: PASS. All 4 Batch-3 opted-in files reach zero CS86xx. The deliberate `Clone<T>` `T?` and `GetType`/`Invoke` nullable-return contracts are recorded as downstream contract decisions; no opted-in consumer regressed (project-wide CS86xx = 0).
