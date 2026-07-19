# Batch S1 Nullable Gate (P7-T3)

Timestamp: 2026-07-19T14-20

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for the 6 Batch S1 files (AC1).

## Files remediated (6): StoreIdentity, StoreLaunchReadinessEvaluator, StoreFilterAttribution,
## StoreLockupAttribution, StoreWrapperInitClock, StoreWrapperInitProbe
- StoreIdentity.Resolve(string? displayName, string? filePathFallback = null); Resolve(Outlook.Store) locals
  nullable; `new StoreIdentity(displayName!/filePathFallback!)` forgiven inside IsNullOrWhiteSpace guards
  (net481 IsNullOrWhiteSpace lacks [NotNullWhen]).
- StoreFilterAttribution.Decide documented-nullable params (`string? storeId/displayName/filePath`,
  `IReadOnlyCollection<string>? excludedStoreIds`, `IList<string>?` token lists); `filePath!.IndexOf` forgiven in
  the two IsNullOrWhiteSpace-guarded FilePath rules; FormatLine displayName nullable.
- StoreLockupAttribution.FormatLine(string? identity, ...); StoreWrapperInitProbe FormatLine/EmitLine
  storeDisplayName nullable. StoreLaunchReadinessEvaluator and StoreWrapperInitClock needed only the pragma.
- No post-condition attributes; no record/init.
