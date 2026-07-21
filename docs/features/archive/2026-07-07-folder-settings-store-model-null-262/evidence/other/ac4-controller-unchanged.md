# AC4 — Controller Unchanged, Populated Model => Ready (P5-T3)

Timestamp: 2026-07-08T00-08

## Controller is byte-for-byte unchanged
Per P5-T2 (scope-lock-confirmation.md), `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
does not appear in `git diff` or `git status` against the baseline commit
(8bd91d1d5db08400a47e04b141bf4a2c4c4a9a82). It is unmodified. AC4 is satisfied without any controller
change, as the spec requires.

## Populated model => Ready mapping
The defect was that `Globals.Ol.StoresWrapper` was left null on recoverable paths, so
`StoreWrapperController.EvaluateLaunchReadiness()` returned `ModelUnavailable`/`StoresUnavailable`
and `Launch()` showed "Store settings are not available yet."

After the fix, on both recoverable paths the model is populated:
- Path 1 (config missing) and Path 2 (null deserialize) now fall through to
  `StoresWrapper = BuildFreshStoresWrapper()` = `new StoresWrapper(_globals).Init()`, which
  materializes `Stores` from the live Outlook stores.
- The direct-coverage test `BuildFreshStoresWrapper_WhenLiveStoresAvailable_ReturnsInitializedWrapper`
  (P3-T3 pass-after) confirms the seam returns a non-null `StoresWrapper` with a populated `Stores`
  list, and the Path 1 / Path 2 regression tests (P3-T3) confirm `StoresWrapper` is assigned the
  fresh model on those paths.

Because the readiness guard (unchanged) reports `Ready` when the model is non-null with populated
`Stores`, `Launch()` now opens the Folder Settings dialog with a populated model and no longer shows
"not available yet" on recoverable paths. Evidence references: P3-T3 pass-after-262.md (Path 1/Path 2
produce a populated model; BuildFreshStoresWrapper returns an initialized wrapper). AC4 satisfied.

Note (Path 3, genuine failure): `StoresWrapper` intentionally remains null, the readiness guard still
reports `ModelUnavailable`, and the existing dialog remains the single user-facing surface (AC3) — no
new dialog is added.
