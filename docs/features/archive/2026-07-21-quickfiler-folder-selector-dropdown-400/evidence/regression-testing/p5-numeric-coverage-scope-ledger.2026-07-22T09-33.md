# P5 numeric coverage scope ledger

Timestamp: `2026-07-22T09-33`

Command: `$production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs'); $tests=@('QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs','QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs'); $protected=@('QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj','QuickFiler/Viewers/ItemViewer.cs','QuickFiler/Viewers/ItemViewer.Designer.cs','coverage.config','scripts/vscode/TaskMaster.cli.runsettings','QuickFiler/packages.config','QuickFiler.Test/packages.config'); $proposed=@('QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs','QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs'); foreach($path in $production+$tests+$protected){"FILE=$path|EXISTS=$([bool](Test-Path $path))|LINES=$((Get-Content $path).Count)|SHA256=$((Get-FileHash -Algorithm SHA256 $path).Hash)|TESTS=$(@(Select-String -Path $path -Pattern '^\s*\[TestMethod\]').Count)"}; foreach($path in $proposed){"PROPOSED=$path|EXISTS=$([bool](Test-Path $path))"}; Select-String -Path 'QuickFiler/Viewers/ItemViewer.cs' -Pattern 'ExcludeFromCodeCoverage' -Context 0,1; Select-String -Path 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs' -Pattern 'ExcludeFromCodeCoverage' -Context 0,1; Select-String -Path 'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs' -Pattern 'ExcludeFromCodeCoverage' -Context 0,1; "ITEM_EXCLUSIONS=$(@(Select-String -Path 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs' -Pattern 'ExcludeFromCodeCoverage').Count)"; "POPUP_EXCLUSIONS=$(@(Select-String -Path 'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs' -Pattern 'ExcludeFromCodeCoverage').Count)"; "COVERAGE_SHA256=$((Get-FileHash -Algorithm SHA256 'coverage.config').Hash)"; git diff --check -- $production $tests $protected; git status --short -- $production $tests $protected $proposed`

EXIT_CODE: `0`

Output Summary: `PASS. All baseline hashes, line counts, test inventories, project/configuration surfaces, and exclusion counts match the approved scope. ItemViewer.Breadcrumb.cs is 480 lines; all five proposed files are absent. J1 is bound to exactly five source rows plus one adjacent project include for each new file.`

## Production baseline

| Source | Lines | SHA-256 |
|---|---:|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` |
| `BreadcrumbPopupUiOperations.cs` | 480 | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` |
| `BreadcrumbDropDownOpenLifetime.cs` | 437 | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` |
| `BreadcrumbDropDownHost.cs` | 470 | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` |
| `BreadcrumbMessengerHub.cs` | 456 | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` |
| `BreadcrumbCollapsedSurfaceController.cs` | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` |
| `ItemViewer.Breadcrumb.cs` | 480 | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` |

## Applicable test baseline

| Test source | Lines | Tests | SHA-256 |
|---|---:|---:|---|
| `ItemViewerBreadcrumbDropDownContractTests.cs` | 100 | 4 | `447013403B6E31BF6550F9A832A7F8661FE025821AA31FB6200E9A4EC9332BF8` |
| `BreadcrumbSelectorOpenRetryTests.cs` | 473 | 4 | `46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5` |
| `BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | 4 | `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104` |
| `BreadcrumbDropDownIntegrationTests.cs` | 500 | 10 | `455A0B76AC2606FDA73FB0CF715FC370194CBCE5D5760A3DA99FB305538AFFDB` |
| `BreadcrumbDropDownCoverageThresholdTests.cs` | 479 | 7 | `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` |
| `BreadcrumbDropDownReadinessTests.cs` | 498 | 7 | `6C910ED246150F2E27BAA6C1EC422B64E5638FB81EFEB3F8B333B37D8B9AF32E` |
| `BreadcrumbCollapsedSurfaceReadinessTests.cs` | 486 | 10 | `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3` |
| `BreadcrumbPopupControlDispatchTests.cs` | 480 | 10 | `F21541FDB8F60D2F9123A6D4D471B2B5DB97FD55DA975BD326942F40EB294991` |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 | 9 | `E4BD60150636A83CE977681249E03C63A2FC7CA96C32C5F8EF5BBB760926E62E` |

## Five absent proposed files

All five paths are absent:

1. `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
2. `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`
3. `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`
4. `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs`
5. `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs`

The last three remain reserved for later validated batches and are not authorized in J1.

## Protected surfaces and exclusions

- `ItemViewer.cs` is 432 lines, SHA-256 `498D1781BE7DF3665D799A4DFC9837AD4F81D6A47B0DEC1CB1C0A84D025AB0E2`, with the pre-existing class exclusion at line 20.
- `ItemViewer.Designer.cs` is 6,224 generated lines, SHA-256 `0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F`.
- `ItemViewer.Breadcrumb.cs` has exactly two method exclusions, at lines 72 and 85, covering the existing collapsed WebView2 attachment/candidate adapters.
- `BreadcrumbPopupUiOperations.cs` has exactly seven method exclusions: `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, `DisposeProductionSurface`, and `NavigateToDocument`.
- `coverage.config` SHA-256 is `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- `TaskMaster.cli.runsettings` SHA-256 is `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`.
- Production project SHA-256 is `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829`.
- Test project SHA-256 is `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB`.
- Package, runsettings, configuration, threshold, exclusion, and designer changes are prohibited.

## J1 five-row binding

| Row | Authorized source action | Baseline |
|---:|---|---|
| 1 | Add `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | Absent |
| 2 | Modify `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 480 lines; hash above |
| 3 | Modify `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | 253 lines; hash above |
| 4 | Modify `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` only as needed for five cases | 100 lines; four tests; hash above |
| 5 | Add `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | Absent |

Exactly one adjacent compile include may be added for row 1 in `QuickFiler.csproj` and row 5 in `QuickFiler.Test.csproj`.

Nonnumeric accounting is restricted to the seven unchanged Popup direct adapters, exact direct ItemViewer WebView2/WinForms adapter lines, and minimal one-line coordinator delegation. No excluded host-neutral body is permitted. No exclusion may be added, removed, changed, or widened. `git diff --check` reported no scope error.
