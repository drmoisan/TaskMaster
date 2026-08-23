# P5 collapsed-readiness disposal-ownership change ledger

Timestamp: `2026-07-22T08:40:53.0153960+00:00`

Command: `$production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs'); $protected=@('QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj','QuickFiler/packages.config','QuickFiler.Test/packages.config','coverage.config','scripts/vscode/TaskMaster.cli.runsettings'); $test='QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs'; foreach($path in $production+$protected+$test){"$path|$((Get-Content $path).Count)|$((Get-FileHash -Algorithm SHA256 $path).Hash)"}; Select-String -Path $test -Pattern '^\s*public (?:async )?(?:Task|void) (AttachAsync_|Reset_|Dispose_|LaterNavigation_|ViewerAttachment_|NavigationReadiness_)'; (Select-String -Path $test -Pattern '\[TestMethod\]').Count; Select-String -Path $test -Pattern 'Thread\.Sleep|Task\.Delay|SpinWait|Retry|WebView2|CreateTemp|GetTemp|Process\.Start|System\.Diagnostics\.Process|File\.|Directory\.'; Select-String -Path 'QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj' -Pattern 'BreadcrumbMessengerHub.cs|BreadcrumbCollapsedSurfaceReadinessTests.cs'; git diff --check -- 'QuickFiler/Viewers/BreadcrumbMessengerHub.cs' $test; git status --short -- $production $protected $test`

EXIT_CODE: `0`

Output Summary: `PASS. BreadcrumbMessengerHub.cs is the only P5 production source whose P5-T79 hash changed. It is 456 physical lines. BreadcrumbCollapsedSurfaceController.cs remains 308 lines with the required SHA-256 92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE. The existing readiness test remains byte-for-byte unchanged at 486 lines with ten tests and all strengthened assertions. Protected projects, packages, runsettings, coverage configuration, exclusions, and designer surfaces were not changed by this correction.`

## Production hash comparison

| Source | P5-T79 SHA-256 | P5-T82 SHA-256 | Lines | Result |
|---|---|---|---:|---|
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` | 270 | Preserved |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` | 253 | Preserved |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | 480 | Preserved |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` | 470 | Preserved |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` | 480 | Preserved |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | 437 | Preserved |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | `8721539FB1CE08181F2AD616A061FE70DCC3CF8D6F20796188FFABCC5CA1BC53` | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | 456 | Authorized change only |
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | 308 | Preserved exactly |

The production diff removes the attachment's duplicate post-transfer readiness and messenger disposal. It adds no compressed statement and changes no other behavior. The controller remains the sole post-transfer cleanup owner.

## Readiness-test inventory

The test file remains 486 physical lines with SHA-256 `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3`, unchanged from P5-T79. It retains exactly ten test methods:

1. `AttachAsync_PendingAndUnrelatedNavigation_DefersReadyPublicationUntilExactSuccess`
2. `AttachAsync_ExactNavigationFailure_LeavesNoReadyMessenger`
3. `Reset_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
4. `Dispose_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
5. `LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger`
6. `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`
7. `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`
8. `NavigationReadiness_UnrelatedCompletionCannotReleaseExactNavigation`
9. `NavigationReadiness_SynchronousSuccessDetachesBeforeNavigationReturns`
10. `NavigationReadiness_FailureAndSynchronousExceptionDetachEveryPath`

The unchanged test hash proves that no test name, assertion, non-saturating disposal observation, or exact `DisposeCount.Should().Be(1)` expectation was removed or weakened. Static inspection found no sleeps, delays, retries, live WebView use, temporary-file use, or external-process use. `git diff --check` reported no error.

## Protected supporting surfaces

| File | P5-T79 SHA-256 | P5-T82 SHA-256 | Result |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829` | `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829` | Preserved |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB` | `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB` | Preserved |
| `QuickFiler/packages.config` | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` | Preserved |
| `QuickFiler.Test/packages.config` | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` | Preserved |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | Preserved |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` | Preserved |

The existing compile includes remain present exactly once at `QuickFiler.csproj:394` and `QuickFiler.Test.csproj:73`. No project, package, runsettings, coverage configuration, exclusion, or designer edit was introduced by P5-T80/P5-T81.
