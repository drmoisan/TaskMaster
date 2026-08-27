# [P3-T4] — All Fifteen Feature Tests Green

Timestamp: 2026-08-27T20-47

Command:
```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"<fifteen FullyQualifiedName~ clauses joined with |>" "/Logger:trx;LogFileName=p3-t4-all-feature-tests-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p3-t4
```

The fifteen clauses, in the order supplied:
`CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException`,
`CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException`,
`EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException`,
`IsCoreInitialized_HasAnExplicitBackingField`,
`PostMessageJson_PostsExactlyOnceToTheUiContext`,
`NavigateToString_PostsExactlyOnceToTheUiContext`,
`SecondHost_DetachesThePredecessorAndTakesOwnership`,
`PredecessorDetach_ToleratesNullCoreWebView2`,
`ControlDisposed_DetachesTheHost`,
`InitializeAsync_InstallsUiDispatcherFromUiSyncContext`,
`InitializeAsync_PreservesAnInjectedDispatcher`,
`PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload`,
`WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption`,
`WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers`,
`WebView2CoreInitializer_ExemptsOnlyTheSdkForwards`.

EXIT_CODE: 0

## Output Summary

**15 tests discovered, 15 passed, 0 failed.** `Test Run Successful.` / `Total tests: 15` /
`Passed: 15`. TRX `<Counters>`: `total="15" executed="15" passed="15" failed="0" error="0"
timeout="0" aborted="0"`.

| # | Test | Result | Duration |
| --- | --- | --- | --- |
| 1 | `IsCoreInitialized_HasAnExplicitBackingField` | Passed | 36 ms |
| 2 | `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | Passed | 41 ms |
| 3 | `WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` | Passed | < 1 ms |
| 4 | `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | Passed | 2 ms |
| 5 | `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | Passed | < 1 ms |
| 6 | `WebView2CoreInitializer_ExemptsOnlyTheSdkForwards` | Passed | < 1 ms |
| 7 | `WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers` | Passed | 62 ms |
| 8 | `PostMessageJson_PostsExactlyOnceToTheUiContext` | Passed | 164 ms |
| 9 | `NavigateToString_PostsExactlyOnceToTheUiContext` | Passed | 4 ms |
| 10 | `SecondHost_DetachesThePredecessorAndTakesOwnership` | Passed | 4 ms |
| 11 | `PredecessorDetach_ToleratesNullCoreWebView2` | Passed | 4 ms |
| 12 | `ControlDisposed_DetachesTheHost` | Passed | 3 ms |
| 13 | `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | Passed | 47 ms |
| 14 | `InitializeAsync_PreservesAnInjectedDispatcher` | Passed | 3 ms |
| 15 | `PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` | Passed | 3 ms |

The rows are in the order vstest reported them, which is not the filter order: the run uses
class-level parallelization, so the execution order differs between runs. All fifteen passing under a
non-deterministic order is itself evidence of order independence; `[P5-T37]` re-runs them in reversed
filter order as an explicit check.

The extraction of the four SDK forwards in `[P3-T2]` and `[P3-T3]` therefore introduced no
behavioural regression: the eleven Phase 1 tests, the inline-fallback test from `[P2-T9]`, and the
three contract tests added in this phase all pass together.
