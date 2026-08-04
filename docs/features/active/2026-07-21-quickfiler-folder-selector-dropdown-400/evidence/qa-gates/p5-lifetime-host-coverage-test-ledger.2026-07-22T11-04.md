# P5 Lifetime and Host Coverage Test Ledger

Timestamp: 2026-07-22T11:04:43Z

Revalidated Timestamp: 2026-07-22T11:12:49Z

Command: A deterministic read-only PowerShell inventory recorded the new test SHA-256, physical and projected CSharpier line counts, TestMethod/DataTestMethod/DataRow counts and names; verified the one project include and in-memory include reversal against the completed batch-K project hash; checked target production, project, package, runsettings, coverage configuration, and designer hashes; enumerated target ExcludeFromCodeCoverage declarations; scanned for live WebView, timing, temporary-file, network, process, and other external resources; and ran `git diff --check` on the batch-L code tuple.

EXIT_CODE: 0

Output Summary: PASS. Batch L changes exactly one new test source and one adjacent `QuickFiler.Test.csproj` Compile include, with zero production files. `BreadcrumbDropDownLifecycleCoverageTests.cs` contained 441 physical lines before formatting and contains 468 stable CSharpier-formatted lines after the final deterministic schedule-fault correction, within the 480-line hard cap. It contains exactly 12 non-data-row TestMethod cases and one TestCleanup method. The new include occurs once immediately after the completed batch-K boundary-coverage include; removing only this LF-terminated include in memory restores the exact batch-K project SHA-256. The 12 deterministic cases cover every P5-T100 below-90 OpenLifetime/Host member and requested sequence through injected operations, delegates, a controlled synchronization context, and in-memory WinForms controls. The existing `BreadcrumbDropDownHostTests` retains exactly 13 cases for the P5-T145 composition. Production, exclusion, package, runsettings, coverage configuration, designer, filter, and threshold surfaces are unchanged. The prohibited-resource scan and `git diff --check` both returned zero matches/errors.

## Authorized batch

| Path | Action | Lines | Tests | SHA-256 |
|---|---|---:|---:|---|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | Added | 468 stable formatted | 12 | `70D700C6F4EF145B106FDDA5058FDCAEA99471CE229D43448DC9917923F2B9D3` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | One adjacent Compile include | 454 | n/a | `BF5D92B819F14301151410A7E470C851FAA148BFA0092B79C95696409A04BB66` |

The include occurs exactly once at line 81, immediately after `BreadcrumbPopupBoundaryCoverageTests.cs`. Removing only the lifecycle-coverage include in memory restores completed batch-K SHA-256 `8BC4A7EEF3B03E82A0FA36DBE443778F924E0183655B94E37EBAFE695AA3E7D0`.

## Exact case inventory and member matrix

| Case | Explicit target |
|---|---|
| `OpenLifetime_SharedOpenWithoutPlacement_CompletesFalseAndCleansSurface` | Shared pending `OpenAsync`, no-placement completion, no show/focus, exact surface and messenger cleanup, and closed host state |
| `OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults` | `Schedule(Action)` and `Schedule(Func<Task>)` success and fault paths, exact execution counts, and exact reported exceptions |
| `OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules` | Idempotent OpenLifetime `Dispose` and both schedule-after-dispose suppression paths |
| `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained` | Factory primary failure, focus/cancel rollback failures, throwing reporter containment, exact callbacks, and primary-exception retention |
| `OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce` | Stale surface retention rejection, failed ready callback, host reset, exact control/messenger cleanup, and closed state |
| `Host_FourForwardingConstructors_CreateWithoutInvokingSurfaceAdapters` | All four P5-T100 below-90 forwarding Host constructors, controlled synchronization context, and zero adapter invocation/errors |
| `Host_InstalledMessengerAndAlreadyOpenPath_ReuseAndFocusCurrentSurface` | Installed messenger getter, current popup messenger, already-open `OpenAsync`, one factory/show, and repeated pending focus |
| `Host_CloseFalseTrueReasonsAndRepeatedClose_HaveExactCallbacks` | Pre-open false close, true explicit/uncommitted close reasons, both `CompleteClose` callback branches, repeated-close false paths, exact focus/cancel/native-close counts |
| `Host_SetTheme_ValidAndBlankValues_FollowExactContract` | Valid theme application and blank-theme `ArgumentException` parameter contract |
| `Host_DisposeAndUseAfterDispose_FollowDeterministicContract` | Host idempotent disposal, exact surface/messenger disposal, disposed dropdown, `ThrowIfDisposed` paths for reset/theme/open, and false close after disposal |
| `Host_NativeClosedCallback_CancelsOnceAndIgnoresRepeatedNotification` | Native Closed callback and scheduled completion, exact cancel/focus behavior, and repeated native notification suppression |
| `Host_CoreConstructorNullDependencies_UseExactParameterContracts` | OpenLifetime constructor guards and all nine core Host constructor null guards with exact parameter names |

Every case is a distinct `[TestMethod]`; the file contains zero `[DataTestMethod]` and zero `[DataRow]` declarations. The shared fixture is independently constructed per MSTest instance and disposed by one `[TestCleanup]` method. The cases use no live WebView, `EnsureCoreWebView2Async`, sleep/delay, temporary file, filesystem I/O, network I/O, process launch, dialog, application loop, or display discovery.

## Protected surfaces

| Path | Lines | SHA-256 | Result |
|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 437 | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | Unchanged |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 472 | `17E186B7EE7F684A2310BD06A9787D29884F3CE6B4D25BD83EDB3000EC718C4A` | Unchanged from completed coordinator-preservation gate |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 480 | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | Unchanged |
| `QuickFiler/QuickFiler.csproj` | 588 | `1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E` | Unchanged |
| `coverage.config` | 24 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | Unchanged |
| `scripts/vscode/TaskMaster.cli.runsettings` | 9 | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` | Unchanged |
| `QuickFiler/packages.config` | 110 | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` | Unchanged |
| `QuickFiler.Test/packages.config` | 168 | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` | Unchanged |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6224 | `0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F` | Unchanged |

`BreadcrumbDropDownOpenLifetime.cs` and `BreadcrumbDropDownHost.cs` contain no `ExcludeFromCodeCoverage` declarations. `BreadcrumbPopupUiOperations.cs` retains exactly seven pre-existing direct-adapter exclusions at lines 97, 377, 380, 387, 394, 421, and 431. Batch L changes no exclusion, configuration, filter, threshold, package, runsettings, designer, production, or existing-test source. No coverage threshold is weakened.
