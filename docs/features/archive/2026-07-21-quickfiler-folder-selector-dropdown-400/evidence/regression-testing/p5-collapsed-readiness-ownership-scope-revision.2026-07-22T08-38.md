# P5 collapsed-readiness disposal-ownership scope revision

Timestamp: `2026-07-22T08:38:55.3262401+00:00`

Command: `$failure='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/p5-collapsed-readiness-duplicate-disposal-fail-before.2026-07-22T08-28.md'; $paths=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs','QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs'); Get-Content -Raw $failure; foreach($path in $paths){$hash=(Get-FileHash -Algorithm SHA256 $path).Hash; $count=(Get-Content $path).Count; "$path|$count|$hash"}; git status --short -- $paths 'QuickFiler/QuickFiler.csproj' 'QuickFiler.Test/QuickFiler.Test.csproj' 'QuickFiler/packages.config' 'QuickFiler.Test/packages.config' 'coverage.config' 'scripts/vscode/TaskMaster.cli.runsettings'; Select-String -Path 'QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs' -Pattern '\[TestMethod\]' | Measure-Object`

EXIT_CODE: `0`

Output Summary: `PASS. The recorded failure is internally consistent: exactly 70 cases were discovered, 69 passed, one failed, and zero were skipped. The sole failure observed two disposals where the retained exact-once assertion requires one. All baseline hashes and physical-line counts match the approved P5-T79 tuple. The correction scope is limited to BreadcrumbMessengerHub.cs and the already-modified readiness test; BreadcrumbCollapsedSurfaceController.cs and all other sources remain immutable.`

## Failure-first reconciliation

- Recorded command exit code: `1`.
- Discovered: `70`.
- Passed: `69`.
- Failed: `1`.
- Skipped: `0`.
- Failed test: `BreadcrumbCollapsedSurfaceReadinessTests.ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`.
- Observed value: `resetSurface.DisposeCount == 2`.
- Retained expectation: `resetSurface.DisposeCount.Should().Be(1)`.
- The non-saturating disposal tracker is valid failure-first evidence and must not be weakened.

## Immutable P5 production scope ledger

| Source | P5 task provenance | Physical lines | SHA-256 | P5-T80 disposition |
|---|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | P5-T10 | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` | Preserve |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | P5-T10 | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` | Preserve |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | P5-T10, P5-T18, P5-T29 | 480 | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | Preserve |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | P5-T18, P5-T36, P5-T49 | 470 | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` | Preserve |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | P5-T18, P5-T56 | 480 | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` | Preserve |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | P5-T19, P5-T36, P5-T49 | 437 | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | Preserve |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | P5-T79 addition | 462 | `8721539FB1CE08181F2AD616A061FE70DCC3CF8D6F20796188FFABCC5CA1BC53` | Sole authorized production change |
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` | P5-T79 ownership boundary | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | Preserve; sole post-transfer cleanup owner |

P5-T43 and P5-T49 also name `BreadcrumbDropDownCoverageThresholdTests.cs`; that test source is outside this correction batch and remains unchanged.

## Authorized test scope

| Source | Physical lines | SHA-256 | Test inventory | Disposition |
|---|---:|---|---:|---|
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 486 | `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3` | 10 `[TestMethod]` declarations | Sole authorized existing test source; retain all names and strengthened assertions |

No test change is currently required. If implementation requires another production or test source, the batch must stop for replanning.

## Protected supporting-file baseline

| File | SHA-256 |
|---|---|
| `QuickFiler/QuickFiler.csproj` | `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB` |
| `QuickFiler/packages.config` | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` |
| `QuickFiler.Test/packages.config` | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` |

Project files, packages, runsettings, `coverage.config`, exclusions, designer files, and completed evidence are protected from this batch.

## Replacement-sequence status

- P5-T73 through P5-T75: historical pre-correction command evidence only; superseded and unchecked until P5-T89 maps passing replacement evidence one-to-one.
- P5-T76: valid failure-first result represented by the 70/69/1/0 evidence above; superseded and unchecked until P5-T89.
- P5-T77 and P5-T78: not run.
- P5-T67 and P5-T68: pending authoritative complete numeric coverage and final Phase 5 audit.
