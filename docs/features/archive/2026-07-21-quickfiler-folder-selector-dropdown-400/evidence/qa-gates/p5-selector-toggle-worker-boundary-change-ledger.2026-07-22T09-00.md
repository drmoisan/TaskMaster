# P5 selector-toggle worker-boundary change ledger

Timestamp: `2026-07-22T09-00`

Command: `$test='QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs'; $production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs'); $protected=@('QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj','QuickFiler/packages.config','QuickFiler.Test/packages.config','coverage.config','scripts/vscode/TaskMaster.cli.runsettings'); foreach($path in @($test)+$production+$protected){"$path|$((Get-Content $path).Count)|$((Get-FileHash -Algorithm SHA256 $path).Hash)"}; Select-String -Path $test -Pattern '^\s*public void (WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary|PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary|PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession|PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle)\('; "TEST_METHODS=$((Select-String -Path $test -Pattern '^\s*\[TestMethod\]').Count)"; Select-String -Path $test -Pattern 'const string toggle|Task worker = Task\.Run\(\(\) => InvokeAmbientNull|context\.WaitForPost\(\)|worker\.GetAwaiter\(\)\.GetResult\(\)|PostCount => ReadLocked\(\(\) => _pending\.Count \+ _executedThreads\.Count\)|BeGreaterThan\(postsBeforeToggle\)|callbackContexts\.Should\(\)\.BeEmpty|DrainUntil\(toggleDispatch\)|ExceptionSnapshot\.Should\(\)\.BeEmpty|callbackContexts\.Should\(\)\.ContainSingle|IsSelectorOpen\.Should\(\)\.BeTrue|provider\.VerifyAll'; Select-String -Path $test -Pattern 'PostCount\+\+|Thread\.Sleep|Task\.Delay|SpinWait|CreateTemp|GetTemp|Process\.Start|System\.Diagnostics\.Process|File\.|Directory\.'; git diff --check -- $test; git status --short -- $production $protected; git status --short -- '**/*.Designer.cs'`

EXIT_CODE: `0`

Output Summary: `PASS. The only corrected source since P5-T91 is BreadcrumbSelectorToggleUiBoundaryTests.cs. It changed from SHA-256 5FD7983359427300F589C0D6A2E80FC00F028DB07613F8948465EB675E1D9AFC to 98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104, remains 480 lines, and retains exactly four tests and every strict assertion. All P5 production and protected-support hashes remain at their P5-T82 values.`

## Authorized test change

`QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` is the only source corrected in P5-T92/P5-T93.

- P5-T91 SHA-256: `5FD7983359427300F589C0D6A2E80FC00F028DB07613F8948465EB675E1D9AFC`.
- P5-T94 SHA-256: `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104`.
- Physical lines: `480`.
- MSTest methods: `4`.

The four original test names remain unchanged:

1. `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`
2. `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary`
3. `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession`
4. `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle`

The corrected test retains the strict post-count increase, empty-before-drain callback assertion, creator-thread drain, same-context single callback, empty captured-exception assertion, selector-open assertion, and provider verification. `InvokeAmbientNull` remains exception-safe. `PostCount` is now the synchronized derived observation `ReadLocked(() => _pending.Count + _executedThreads.Count)`, and the separate `PostCount++` write is absent.

Static inspection and `git diff --check` found no compressed statements, collapsed assertions, sleeps, delays, spin waits, timing thresholds, live UI/WebView execution, temporary-file access, external-process access, or whitespace error introduced by this correction.

## P5 production hash preservation

| Production source | Physical lines | P5-T94 SHA-256 | Result |
|---|---:|---|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` | P5-T82 preserved |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` | P5-T82 preserved |
| `BreadcrumbPopupUiOperations.cs` | 480 | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | P5-T82 preserved |
| `BreadcrumbDropDownHost.cs` | 470 | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` | P5-T82 preserved |
| `ItemViewer.Breadcrumb.cs` | 480 | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` | P5-T82 preserved |
| `BreadcrumbDropDownOpenLifetime.cs` | 437 | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | P5-T82 preserved |
| `BreadcrumbMessengerHub.cs` | 456 | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | P5-T82 preserved |
| `BreadcrumbCollapsedSurfaceController.cs` | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | P5-T82 preserved |

## Protected surfaces

| Protected file | SHA-256 | Result |
|---|---|---|
| `QuickFiler/QuickFiler.csproj` | `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829` | P5-T82 preserved |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB` | P5-T82 preserved |
| `QuickFiler/packages.config` | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` | P5-T82 preserved |
| `QuickFiler.Test/packages.config` | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` | P5-T82 preserved |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | P5-T82 preserved |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` | P5-T82 preserved |

No other test, project, package, runsettings, coverage configuration, exclusion, or designer file was corrected in this batch.
