# Batch S2b Nullable Gate — StoresWrapper partial pair (P8-T5)

Timestamp: 2026-07-19T15-05

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for both StoresWrapper.cs and StoresWrapper.Filtering.cs (AC1, AC7 — both
  partial-class parts remediated together in the single task P8-T4).

## Key annotation decisions
- `Globals` (IApplicationGlobals, not default-initialized) nullable; `Stores` (List<StoreWrapper>) nullable
  (null until Init/Rewire; guarded by `Stores ??= []`). `GetFilteredStores` derefs `Globals!` (set by the
  globals ctor / deserialization before Init runs).
- Nullable primitive-read locals (`string? displayName/storeId/filePath`) in ShouldIncludeStore /
  ShouldIncludeStoreInstrumented (all try/catch fail-open reads); `filePath!.IndexOf` forgiven inside the
  IsNullOrWhiteSpace-guarded FilePath rules.
- Static `StoreIsIncluded` overload: `string? storeId = null`, `IReadOnlyCollection<string>? excludedStoreIds
  = null`, `string? filePath`, `filePath!.IndexOf` — consistent with the instance overload.
- External oblivious `SmartSerializable<T>` and `CurrentStoreContext` consumed at call sites only (not edited).
  No post-condition attributes; no record/init.
