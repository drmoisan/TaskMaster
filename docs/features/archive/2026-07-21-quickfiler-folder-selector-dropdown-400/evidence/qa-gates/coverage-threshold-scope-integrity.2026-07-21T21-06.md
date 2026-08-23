# Coverage Threshold Scope Integrity

Timestamp: 2026-07-21T21-06Z
Command: PowerShell `Get-Content`, `Get-FileHash -Algorithm SHA256`, exact-regex `Select-String`, `git diff -- coverage.config`, `git diff -- QuickFiler.Test/QuickFiler.Test.csproj`, and `git diff --check` checks against the P4-T5 baseline
EXIT_CODE: 0
Output Summary: The post-blocker corrective diff is limited to one 395-line test file and one adjacent legacy-project include. All protected production, existing-test, and assertion hashes match their recorded baselines. No scope mismatch requires another plan revision.

## Corrective Diff Boundary

The P4-T5 baseline recorded the coverage-threshold test and its project include as absent. The only changes since that baseline are:

- New `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs`: 395 lines, SHA-256 `2627b112a53efc3fa358af1cdee0d60dc7cbee350b84e015c5124df4cdcffd91`.
- Exactly one `<Compile Include="Viewers\BreadcrumbDropDownCoverageThresholdTests.cs" />` at `QuickFiler.Test/QuickFiler.Test.csproj` line 71.

The include is adjacent to the existing breadcrumb test inventory:

```text
70|<Compile Include="Viewers\BreadcrumbDropDownIntegrationTests.cs" />
71|<Compile Include="Viewers\BreadcrumbDropDownCoverageThresholdTests.cs" />
72|<Compile Include="Controllers\QfcItemControllerBreadcrumbDropDownTests.cs" />
```

The new file contains exactly seven `[TestMethod]` declarations and is below the 500-line limit. `git diff --check` exited 0.

## P4-T5 Whole-File Hash Comparison

| File | Lines | P4-T5 SHA-256 | Current SHA-256 | Result |
|---|---:|---|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | 499 | `8d02e8b9e8c68c9d197e22787c2f82e724e8fc7b7e07d0ffb354af9dd1928d5c` | `8d02e8b9e8c68c9d197e22787c2f82e724e8fc7b7e07d0ffb354af9dd1928d5c` | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | `455a0b76ac2606fda73fb0cf715fc370194cbce5d5760a3da99fb305538affdb` | `455a0b76ac2606fda73fb0cf715fc370194cbce5d5760a3da99fb305538affdb` | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 379 | `386126ef040d87e72091322d000c5a3e607911d71a53215050bebda14ae0e0ab` | `386126ef040d87e72091322d000c5a3e607911d71a53215050bebda14ae0e0ab` | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs` | 277 | `d35570def5bb0aec362aff5e8a977414119c9eee490ca812aa76f261d9fffd72` | `d35570def5bb0aec362aff5e8a977414119c9eee490ca812aa76f261d9fffd72` | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 305 | `69e8b09fc4cd7f656bc39d594b8079e071af3b26cf4c114c014d4b33420b9610` | `69e8b09fc4cd7f656bc39d594b8079e071af3b26cf4c114c014d4b33420b9610` | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 369 | `fd9475a1ca8bfc9c002c9f2882802ee555dfa13f71b3f59e93cf78968a22a2fe` | `fd9475a1ca8bfc9c002c9f2882802ee555dfa13f71b3f59e93cf78968a22a2fe` | PASS |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 484 | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` | PASS |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | 118 | `4f840dfb2ea96c462e57c5f93d6d88ec9a156251751ce2459c71696b27f767a3` | `4f840dfb2ea96c462e57c5f93d6d88ec9a156251751ce2459c71696b27f767a3` | PASS |

`BreadcrumbDropDownIntegrationTests.cs` remains exactly 500 lines.

## P0-T9 Protected Assertion Comparison

The P0-T9 hashing method was repeated over ordered exact source lines matching `.Should(` or `Assert.`.

| Protected file | Assertions | P0-T9 SHA-256 | Current SHA-256 | Result |
|---|---:|---|---|---|
| `BreadcrumbDropDownReadinessTests.cs` | 51 | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | PASS |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 81 | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | PASS |
| `BreadcrumbDropDownHostTests.cs` | 52 | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | PASS |
| `BreadcrumbDropDownLifecycleTests.cs` | 34 | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | PASS |

## Test-to-Source and Acceptance Mapping

| New test | Source seam exercised | Plan row | Acceptance support |
|---|---|---|---|
| `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` | `CompleteOpenAsync`, lines 190-198 | P4-T7 | AC-14 lifecycle cleanup; AC-15 deterministic initialization failure; AC-16 deterministic MSTest; AC-18 measurable-member coverage |
| `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface` | `OpenCoreAsync`, line 220 | P4-T7 | AC-13 focus/open readiness; AC-14 reset/disposal; AC-15 deterministic lifecycle edge; AC-16; AC-18 |
| `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus` | `OpenCoreAsync`, line 246 | P4-T8 | AC-13 bounded focus behavior; AC-14 no stale callback after reset; AC-15; AC-16; AC-18 |
| `OpenAsync_FocusCallbackResetsLifecycle_StopsBeforeSuccess` | `OpenCoreAsync`, line 249 | P4-T8 | AC-13 pending-focus lifecycle; AC-14 reset safety; AC-15; AC-16; AC-18 |
| `OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle` | `OpenCoreAsync`, line 256 | P4-T8 | AC-14 stale completion cannot overwrite current lifecycle; AC-15 deterministic failure; AC-16; AC-18 |
| `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface` | `WaitForReadinessAsync`, line 376 | P4-T9 | AC-14 reset/disposal without callback; AC-15 deterministic repeated-lifecycle edge; AC-16; AC-18 |
| `OpenAsync_LegacyFactoryReturnsNull_ReportsNoSurfaceAndRollsBack` | `NormalizeFactory`, lines 462-464 including the returned lambda | P4-T9 | AC-14 lazy factory boundary; AC-15 initialization-failure handling; AC-16; AC-18 |

The tests use injected delegates, in-memory fakes, and controlled tasks. The prohibited-pattern scan found no sleep, timed delay, blocking wait, filesystem access, network access, Outlook automation, or live WebView2 initialization.

## Excluded Scope Checks

- Package references and dependencies: unchanged by the corrective batch.
- Repository or project settings: unchanged by the corrective batch.
- `coverage.config`, coverage filters, thresholds, and exclusions: unchanged; `git diff -- coverage.config` is empty.
- Public production signatures: unchanged by the corrective batch.
- Production files: both P4-T5 hashes match.
- Existing test files: every P4-T5 hash matches.
- Other project entries: the baseline include inventory is unchanged except for the single authorized line 71 include.

P4-T14 result: PASS. Phase 5 may restart from P5-T1.
