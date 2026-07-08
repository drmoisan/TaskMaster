# Phase 8 — File Size and csproj Wiring Audit (P8-T1, P8-T2)

Timestamp: 2026-07-08T04-40

## P8-T1 — File size (<= 500-line repo limit)

| File | Lines | <= 500 |
|---|---|---|
| UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs | 179 | Yes |
| UtilitiesCS/OutlookObjects/Store/DisabledStoreRow.cs | 26 | Yes |
| UtilitiesCS/OutlookObjects/Store/IDisabledStoresViewer.cs | 30 | Yes |
| UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.cs | 51 | Yes |
| UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.Designer.cs | 135 | Yes |
| UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs | 40 | Yes |
| UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs | 291 | Yes |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs (modified) | 382 | Yes |

All listed files are <= 500 lines.

## P8-T2 — csproj wiring audit

UtilitiesCS/UtilitiesCS.csproj:
- `<Compile Include="OutlookObjects\Store\DisabledStoresController.cs" />` — line 699. Present.
- `<Compile Include="OutlookObjects\Store\DisabledStoreRow.cs" />` — line 700. Present.
- `<Compile Include="OutlookObjects\Store\DisabledStoresViewer.cs">` with `<SubType>Form</SubType>` — lines 701-703. Present.
- `<Compile Include="OutlookObjects\Store\DisabledStoresViewer.Designer.cs">` with `<DependentUpon>DisabledStoresViewer.cs</DependentUpon>` — lines 704-705. Present.
- `<Compile Include="OutlookObjects\Store\IDisabledStoresViewer.cs" />` — line 707. Present.
- `<Compile Include="OutlookObjects\Store\StoreLaunchReadinessEvaluator.cs" />` — line 712. Present.
- `<EmbeddedResource Include="OutlookObjects\Store\DisabledStoresViewer.resx">` with `<DependentUpon>DisabledStoresViewer.cs</DependentUpon>` — lines 1141-1142. Present.

UtilitiesCS.Test/UtilitiesCS.Test.csproj:
- `<Compile Include="OutlookObjects\Store\DisabledStoresControllerTests.cs" />` — line 324. Present.

All 5 new production `.cs` files + `StoreLaunchReadinessEvaluator.cs` have `<Compile Include>`
entries; the new `.resx` has an `<EmbeddedResource ... DependentUpon>` entry; the Designer file
has `<DependentUpon>`; the test file has a `<Compile Include>` in the test csproj. Wiring complete
(verified additionally by the clean full-solution build in P7-T2/P7-T3).
