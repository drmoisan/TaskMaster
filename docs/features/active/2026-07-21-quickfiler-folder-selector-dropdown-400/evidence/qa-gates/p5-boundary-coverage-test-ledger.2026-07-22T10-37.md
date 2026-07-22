# P5 Boundary Coverage Test Ledger

Timestamp: 2026-07-22T10:37:00Z

Revalidated Timestamp: 2026-07-22T10:42:21Z

Command: One deterministic read-only PowerShell inventory recorded the new test SHA-256, physical lines, TestMethod/DataTestMethod/DataRow counts and names; verified the one project include and in-memory include reversal against the P5-T122/J1 project hash; checked target production, project, package, runsettings, coverage configuration, and designer hashes; enumerated all target ExcludeFromCodeCoverage declarations; scanned for direct excluded adapters and prohibited timing, temporary-file, network, and process resources; and ran git diff --check on the batch-K tuple.

EXIT_CODE: 0

Output Summary: PASS. Batch K changes exactly one new test source and one adjacent QuickFiler.Test.csproj Compile include. BreadcrumbPopupBoundaryCoverageTests.cs is 479 physical lines with exactly 18 non-data-row TestMethod cases after the analyzer-required C# 7.3 correction. The new include occurs once; removing only that LF-terminated include in memory restores the exact J1 test-project SHA-256. Dispatcher, Factory, Popup operations, production project, packages, runsettings, coverage configuration, designer, threshold, and exclusion surfaces retain their J1 values. Every named P5-T100 below-90 boundary and sequence has an explicit deterministic case. The prohibited-resource/direct-adapter scan returned zero matches, and git diff --check returned zero.

## Authorized batch

| Path | Action | Lines | Tests | SHA-256 |
|---|---|---:|---:|---|
| QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs | Added | 479 | 18 | D537569CE3C7917739008BD0138297438474649864C5C3BFF0E92D098F57848E |
| QuickFiler.Test/QuickFiler.Test.csproj | One adjacent Compile include | 453 | n/a | 8BC4A7EEF3B03E82A0FA36DBE443778F924E0183655B94E37EBAFE695AA3E7D0 |

The include occurs exactly once after BreadcrumbDropDownIntegrationTests.cs. Removing only this include in memory restores J1 SHA-256 59DC70BC44CE50E9556738A1BB80B280977576E693D9F23B29943188B2AC96FC.

## Member and sequence matrix

| Case | Explicit target |
|---|---|
| Dispatcher_NullInputsAndThrowingSink_AreHandledByContract | Dispatcher context/error-sink/action/value-action/report null guards and contained throwing error sink |
| Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction | Owner-thread-only worker branch, one report, zero action execution |
| Dispatcher_PostedFailure_ReportsOnceAndRestoresBoundary | Posted action catch/ReportOnce, creator-thread execution, and executing-dispatcher restoration |
| ProductionFactoryCreate_ControlledContext_CapturesWithoutInvokingAdapters | Production Factory Create overload and controlled CaptureCurrent path without invoking an adapter |
| InjectedFactory_Success_UsesOwnerBoundaryAndReturnsReadySurface | CreateSurfaceAsync create/init/core/navigation/readiness return and exact owner-thread operations |
| InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup | Create-control failure and null-owned-surface cleanup branch |
| InjectedFactory_InitializationFailure_DisposesControlOnce | External initialization failure reporting and exact control cleanup |
| InjectedFactory_CoreFailure_DisposesControlOnce | Read-core failure reporting and exact control cleanup |
| InjectedFactory_NavigationFailure_DisposesControlOnce | Begin-navigation failure reporting and exact control cleanup |
| InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure | Failure cleanup suppression, primary exception preservation, and no duplicate report |
| Readiness_ConstructorGuardsBlankNameAndNullDetach | Blank surface-name and null detach-handler guards |
| Readiness_BeginNavigationGuardsNullDuplicateAndTerminalRequests | Null navigate, duplicate request, terminal/disposed request, cancel, and single detach |
| Readiness_UnrelatedAndDuplicateNotifications_CompleteCapturedSuccessOnce | Pre-request start, first captured ID, duplicate start, unrelated completion, success, terminal duplicate, and one detach |
| Readiness_Failure_NormalizesNullAndBlankStatuses | Failed completion for null and whitespace statuses, Unknown normalization, and exact detach |
| Readiness_CancelAndDispose_AreIdempotent | Cancel, repeated cancel, Dispose, post-terminal notifications, canceled task, and one detach |
| Readiness_DetachFailure_IsContainedAndCompletionSucceeds | Detach-handler exception catch/log path and successful completion |
| CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries | Both ternary branches: ambient-null owner-only tests and controlled-context CaptureCurrent |
| NormalizeFactory_SuccessAndNullResultPaths_PreserveContract | Normalized legacy success tuple/completed readiness and null-result InvalidOperationException path |

All injected factory cases assert direct operation thread IDs. Success asserts one control and one messenger disposal with zero reports. Each failure case asserts exact control/messenger disposal and exactly one primary report. Dispatcher and readiness cases assert exact action, report, completion, and detach counts.

## Protected surfaces

| Path | SHA-256 | Result |
|---|---|---|
| QuickFiler/Viewers/BreadcrumbUiDispatcher.cs | 64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42 | J1 unchanged |
| QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs | 28726E027BEEF4FE4633BA5BBF00AF6DA7E6C0D59CF6093657545997B3D574C9 | J1 unchanged |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396 | J1 unchanged |
| QuickFiler/QuickFiler.csproj | 1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E | J1 unchanged |
| coverage.config | B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943 | Unchanged |
| scripts/vscode/TaskMaster.cli.runsettings | 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 | Unchanged |
| QuickFiler/packages.config | 8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485 | Unchanged |
| QuickFiler.Test/packages.config | 869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83 | Unchanged |
| QuickFiler/Viewers/ItemViewer.Designer.cs | 0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F | Unchanged |

BreadcrumbUiDispatcher.cs and BreadcrumbWebViewSurfaceFactory.cs remain unexcluded. BreadcrumbPopupUiOperations.cs retains exactly seven exclusions at lines 97, 377, 380, 387, 394, 421, and 431, with the same adapter method names and bodies. No exclusion, configuration, filter, threshold, package, runsettings, designer, production, or existing-test change occurred in batch K.
