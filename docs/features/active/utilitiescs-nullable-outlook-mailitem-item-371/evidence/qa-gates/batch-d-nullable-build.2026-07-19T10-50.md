# Batch D — Pragma-Only Nullable Build Verification (P4-T9)

- Timestamp: 2026-07-19T10-50
- Task: [P4-T9]
- Files opted in (Batch D, OutlookItem reflection-wrapper family, 6 files): `Item/OutlookItem.cs`, `Item/OutlookItemExtensions.cs`, `Item/OutlookItemFlaggable.cs`, `Item/OutlookItemTry.cs`, `Item/OutlookItemTryGet.cs`, `Item/OutlookItemFlaggableTry.cs`
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`) — solution build halts on pre-existing out-of-scope SVGControl CS0649 (see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO TWAE, NO `/p:Nullable=enable`)
- EXIT_CODE (isolated authoritative build): 0

## Deliberate unconstrained-generic contract (consistent across the family)

- `OutlookItem.GetPropertyValueIfExists<T>` -> `T?`; `TryGetPropertyInfo` -> `PropertyInfo?`. `GetPropertyValue<T>`/`SetPropertyValue<T>`/`CallMethod` throw (never return null) so their non-null returns are unchanged. Fields `_item`/`_type`/`_typeOlObjectClass` -> nullable (genuinely nullable via the parameterless ctor and the `OutlookItemFlaggable(IOutlookItem)` ctor's `_item = item.InnerObject`/`_type = ...?.GetType()`); `_args` given an inline non-null default. Getters `Item`/`ItemType`/`InnerObject` -> nullable. Reflection derefs of `ItemType`/`_type` use a justified `!` (preserving the original NRE-caught behavior); the error-log-string deref uses `?.`.
- `OutlookItemExtensions`: `TryGet<T>`/`TryCall<T>` -> `T?`; `TryGetPropertyValue`/`TryGetPropertyValue<T>`/`TryCallMethod` -> `object?`; `TryGetPropertyInfo` -> `PropertyInfo?`; base `TrySetPropertyValue(...,object)` -> `object?` propertyValue; `item.ItemType!` at reflection sites.
- `OutlookItemTry` (try/catch-swallowing decorator over `IOutlookItem`): `TryGet<T>`/`TryCall<T>` -> `T?`; reference-type public members annotated nullable (they return `default(T)` on swallowed exception); string get/set properties -> `string?` with `TrySet<string?>`. Value-type members unchanged. Decorator seam preserved exactly.
- `OutlookItemTryGet` (bool + `out` decorator): `TryGet<T>`/`TryCall<T>` -> `out T?`; reference-type `out` params on public methods -> `out TYPE?`. Value-type `out` params unchanged.
- `OutlookItemFlaggable`: nullable locals (`dueDate`, `work`, `startDate`) fed by nullable `TryGetPropertyValue`; `!` before unboxing (preserving the original unbox-or-throw / catch-return-default behavior); base-class contract consumed consistently.
- `OutlookItemFlaggableTry`: string members `TaskSubject`/`GetUdfString` -> `string?`; value-type members unchanged; `IOutlookItemFlaggable` decorator seam preserved.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** for the 6 opted-in Batch D files (consistent unconstrained-generic contract across the family).
- No new diagnostics elsewhere.
- `OutlookItem.cs` remains a single file at 504 lines (not split; pre-existing 500-line breach flagged in maintainer-flags P4-T2).
