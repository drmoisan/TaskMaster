# Baseline csproj Wiring (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: Grep for `<Compile Include` and `packages.config` in both csproj files.
EXIT_CODE: 0

Output Summary:
(a) `UtilitiesCS/UtilitiesCS.csproj` uses explicit `<Compile Include>` items (no glob). Confirmed present:
   - line 670: `<Compile Include="OutlookObjects\Store\StoresWrapper.cs" />`
   - line 671: `<Compile Include="OutlookObjects\Store\StoreWrapper.cs" />`
   - line 672: `<Compile Include="OutlookObjects\Store\StoreWrapperController.cs" />`
(b) `UtilitiesCS.Test/UtilitiesCS.Test.csproj` uses explicit `<Compile Include>` items (no glob), and references `packages.config` (line 433 `<None Include="packages.config" />`). Confirmed present:
   - line 288: `<Compile Include="OutlookObjects\Store\StoresWrapperTests.cs" />`
(c) THEREFORE the two NEW `.cs` files require explicit new `<Compile Include>` items to compile:
   - `<Compile Include="OutlookObjects\Store\StoreFilterAttribution.cs" />` in UtilitiesCS.csproj
   - `<Compile Include="OutlookObjects\Store\StoreFilterAttributionTests.cs" />` in UtilitiesCS.Test.csproj

Acceptance: both projects use explicit includes (no glob); two new `<Compile Include>` items are required.

## P0-T8 finding (virtual seam / test subclass check)

Examined `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`. Two private nested subclasses of `StoresWrapper` exist:
- `TestableStoresWrapper : StoresWrapper` — re-exposes `RewireOlObjectsAsync` via `new`; does NOT override `Init`/`GetFilteredStores`/`ShouldIncludeStore`.
- `AdapterObservingStoresWrapper : StoresWrapper` — overrides `RewireAfterDeserializeAsync()` only.

Existing virtual members on StoresWrapper: `Init()`, `RewireAfterDeserializeAsync()`. `GetFilteredStores()` is `private`; `ShouldIncludeStore` is `public` non-virtual; `StoreIsIncluded` is `public static`.

Conclusion: This plan adds a PRIVATE non-virtual instrumented helper (`ShouldIncludeStoreInstrumented`) and routes the private `GetFilteredStores` through it. No new `virtual`/`protected internal virtual` seam is introduced. Therefore NO override updates are required in StoresWrapperTests.cs (this discharges P2-T5's no-op branch). The existing `ShouldIncludeStore`/`StoreIsIncluded` public methods are retained so the existing `AssertInclusionDecision` tests continue to compile and pass.
