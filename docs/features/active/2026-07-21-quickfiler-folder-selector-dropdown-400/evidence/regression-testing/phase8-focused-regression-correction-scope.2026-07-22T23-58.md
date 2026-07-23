# Phase 8 focused-regression correction scope inventory

Timestamp: 2026-07-22T23:58:04.1873263-04:00

Command: `$editable=@('QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'); $protected=@('QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs','coverage.config'); $configuration=@('QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj','TaskMaster.sln','.editorconfig','Directory.Build.targets','scripts/vscode/Invoke-MSTestWithCoverage.ps1'); foreach($path in $editable+$protected+$configuration){$hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; $lines=(Get-Content -LiteralPath $path).Count; "$path|$hash|$lines"}; foreach($path in @($editable[1],$editable[2],$protected[0],$protected[1])){$raw=Get-Content -Raw -LiteralPath $path; $names=[regex]::Matches($raw,'(?s)\[TestMethod\]\s*public\s+(?:async\s+)?(?:Task|void)\s+([A-Za-z0-9_]+)\s*\(') | ForEach-Object {$_.Groups[1].Value}; "TESTS|$path|$($names.Count)|$($names -join ',')"}; 'FILTER|FullyQualifiedName~BreadcrumbDuplicateIdentityTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~FolderBreadcrumbRouterSelectionConcurrencyTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests'; 'THRESHOLDS|repository>=80|new-or-changed>=90'; ([xml](Get-Content -Raw -LiteralPath 'coverage.config')).Configuration.CodeCoverage.ModulePaths.Exclude.ModulePath | ForEach-Object {"EXCLUSION|$($_.'#text' ?? $_)"}`

EXIT_CODE: 0

Output Summary: The read-only inventory captured SHA-256 hashes, physical-line counts, compiled test names, project/configuration inputs, the exact 16-class preservation filter, coverage thresholds, and coverage exclusions before any P8 correction source edit.

## Authorized editable tuple

| File | Pre-correction SHA-256 | Physical lines |
|---|---:|---:|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `4566CA3383471E2DDC946309125930A8A08C140B924406507E68D10AD80F03E0` | 477 |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | `9C236CAFDDBD6E2465C7FD6B022817FC5B077B9FD33514515C555826A3A8C3DB` | 376 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | `A018588873F04CDA716CB2D37BFEED573EFD3EFE676AEF4C89A4167E20B15B8A` | 500 |

## Protected witnesses

| File | Required SHA-256 | Physical lines |
|---|---:|---:|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` | 479 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | `989BE280294875DCEFD2E936F6F48D65F3EAFED21B4AE4530D4E6288561AFC59` | 444 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | `326D190D5BB4B3634A0ABDE6A786A25354349A925B7DE8226AEB11990C8E3B01` | 309 |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | 24 |

All other production, test, project, configuration, filter, threshold, and exclusion sources are outside the editable tuple and remain protected.

## Test-name inventory

- `QfcItemControllerBreadcrumbDropDownTests.cs`: 6 `[TestMethod]` methods:
  - `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
  - `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`
  - `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
  - `Cleanup_ResetsInjectedHostForPooledViewerReuse`
  - `OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler`
  - `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`
- `BreadcrumbDropDownIntegrationTests.cs`: 10 `[TestMethod]` methods, including the preserved `InitializationFailure_CancelsSessionWithoutDuplicateClose`.
- `BreadcrumbDropDownCoverageThresholdTests.cs`: 7 `[TestMethod]` methods, including the protected `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`.
- `BreadcrumbDropDownOpenCoordinatorTests.cs`: 8 `[TestMethod]` methods, including the protected reason-specific `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen`.

## Project and configuration inventory

| File | Pre-correction SHA-256 |
|---|---:|
| `QuickFiler/QuickFiler.csproj` | `D05401B1F146FDF84D3B9323F2E05A97028DD511CE5FE7C6C3B438F52907F7BF` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `06663711C83A1FE5DE1B485D5B361DB9EDCE43501E0C37A5AF081DC0D0804FC7` |
| `TaskMaster.sln` | `8884A3C7B88B79C6A052FF7199D055297AF19B5BE3D5264DD32E8F9EAF7016E9` |
| `.editorconfig` | `E19340D3A51E6B2CF90CB2669FDB1B85A5AAB96900C0F2ABB925BC2CC4CA96AA` |
| `Directory.Build.targets` | `94D5CE3889BA4F018C4717AA841E2E021288A6DF167B03D6C469D0F1BA03C013` |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `4782C4E3F00CEA7F852AC884387AE9FDD15615F888F132CB7E71F2F1D9868E26` |

## Filter, thresholds, and exclusions

Exact P8-T1 preservation filter:

`FullyQualifiedName~BreadcrumbDuplicateIdentityTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~FolderBreadcrumbRouterSelectionConcurrencyTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests`

Coverage thresholds remain repository-wide line coverage `>= 80%` and new or changed code coverage `>= 90%`.

`coverage.config` exclusions remain:

- `.*Deedle.*`
- `.*FSharp.*`
- `.*Castle\.Core.*`
- `.*FluentAssertions.*`
- `.*Moq.*`
- `.*Microsoft\.Testing.*`
- `.*MSTest.*`
