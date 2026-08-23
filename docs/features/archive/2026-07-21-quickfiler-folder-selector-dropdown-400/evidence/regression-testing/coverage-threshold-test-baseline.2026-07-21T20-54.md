# Coverage Threshold Test Baseline

Timestamp: 2026-07-21T20-54Z
Command: `Test-Path QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs`; exact `Select-String` include counts/inventory in `QuickFiler.Test/QuickFiler.Test.csproj`; `Get-Content` line counts; and `Get-FileHash -Algorithm SHA256` for every existing `QuickFiler.Test/Viewers/BreadcrumbDropDown*Tests.cs`, `BreadcrumbSelectorCoordinatorTests.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, and `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`
EXIT_CODE: 0
Output Summary: The revised-plan blocker state matches exactly. The authorized coverage-threshold test file and include are absent, the integration test is exactly 500 lines, all existing-test and production hashes are captured, and the four complete source-member gaps from the superseded 20:38 coverage result are recorded.

## Superseded coverage diagnostic

Source: `evidence/qa-gates/final-pass-superseded.2026-07-21T20-44.md`

| Source member | Covered/valid | Coverage | Uncovered source lines |
|---|---:|---:|---|
| `CompleteOpenAsync` | 20/29 | 68.9655% | 190-198 |
| `OpenCoreAsync` | 35/40 | 87.5000% | 220, 230, 246, 249, 256 |
| `WaitForReadinessAsync` | 6/7 | 85.7143% | 376 |
| `NormalizeFactory`, including returned lambda | 14/17 | 82.3529% | 462-464 |

## Authorized-file absence and line count

- `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` exists: No
- Exact coverage-threshold project include count: 0
- `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`: 500 lines

## Existing-test whole-file hashes

| File | Lines | SHA-256 |
|---|---:|---|
| `BreadcrumbDropDownHostTests.cs` | 499 | `8d02e8b9e8c68c9d197e22787c2f82e724e8fc7b7e07d0ffb354af9dd1928d5c` |
| `BreadcrumbDropDownIntegrationTests.cs` | 500 | `455a0b76ac2606fda73fb0cf715fc370194cbce5d5760a3da99fb305538affdb` |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 379 | `386126ef040d87e72091322d000c5a3e607911d71a53215050bebda14ae0e0ab` |
| `BreadcrumbDropDownLifecycleTests.cs` | 277 | `d35570def5bb0aec362aff5e8a977414119c9eee490ca812aa76f261d9fffd72` |
| `BreadcrumbDropDownReadinessTests.cs` | 305 | `69e8b09fc4cd7f656bc39d594b8079e071af3b26cf4c114c014d4b33420b9610` |
| `BreadcrumbSelectorCoordinatorTests.cs` | 369 | `fd9475a1ca8bfc9c002c9f2882802ee555dfa13f71b3f59e93cf78968a22a2fe` |

## Production whole-file hashes

| File | Lines | SHA-256 |
|---|---:|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 484 | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | 118 | `4f840dfb2ea96c462e57c5f93d6d88ec9a156251751ce2459c71696b27f767a3` |

## Breadcrumb project include inventory

```text
62|<Compile Include="Viewers\BreadcrumbSelectorCoordinatorTests.cs" />
65|<Compile Include="Viewers\BreadcrumbDropDownHostTests.cs" />
66|<Compile Include="Viewers\BreadcrumbDropDownReadinessTests.cs" />
67|<Compile Include="Viewers\BreadcrumbDropDownLifecycleConcurrencyTests.cs" />
68|<Compile Include="Viewers\BreadcrumbDropDownLifecycleTests.cs" />
69|<Compile Include="Viewers\ItemViewerBreadcrumbDropDownContractTests.cs" />
70|<Compile Include="Viewers\BreadcrumbDropDownIntegrationTests.cs" />
71|<Compile Include="Controllers\QfcItemControllerBreadcrumbDropDownTests.cs" />
```

P4-T5 result: PASS. No blocker-state mismatch requires another revision.
