# Test Independence and Reversed-Order Re-Run ([P5-T37])

Timestamp: 2026-08-27T23-30

Command:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' `
  'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  /TestCaseFilter:"<fifteen FullyQualifiedName~ clauses joined with | in reversed declaration order>" `
  /Logger:trx `
  /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p5-t37
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 15 / Passed: 15 / Total time: 1.3235 Seconds`. Fifteen passed,
zero failed, zero skipped. The filter carried exactly fifteen `FullyQualifiedName~` clauses joined
with `|`, and fifteen tests were discovered, so the filter is neither over- nor under-matching.

The TRX is at
`evidence/regression-testing/p5-t37/p5-t37-reversed-order.trx`. It is the only file in that
directory. Its account name and machine name were replaced with `USER` and `HOST` case-insensitively
before it was committed, matching the treatment of every earlier TRX in this feature's evidence tree;
a case-insensitive substitution is required because vstest writes the `storage=` attribute in
lower case.

## Reversed order

The filter clauses were supplied in the reverse of source-declaration order:

| Filter position | Test | Declaration position |
| --- | --- | --- |
| 1 | `WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers` | 15 |
| 2 | `WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` | 14 |
| 3 | `IsCoreInitialized_HasAnExplicitBackingField` | 13 |
| 4 | `PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` | 12 |
| 5 | `InitializeAsync_PreservesAnInjectedDispatcher` | 11 |
| 6 | `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | 10 |
| 7 | `ControlDisposed_DetachesTheHost` | 9 |
| 8 | `PredecessorDetach_ToleratesNullCoreWebView2` | 8 |
| 9 | `SecondHost_DetachesThePredecessorAndTakesOwnership` | 7 |
| 10 | `NavigateToString_PostsExactlyOnceToTheUiContext` | 6 |
| 11 | `PostMessageJson_PostsExactlyOnceToTheUiContext` | 5 |
| 12 | `WebView2CoreInitializer_ExemptsOnlyTheSdkForwards` | 4 |
| 13 | `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | 3 |
| 14 | `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | 2 |
| 15 | `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | 1 |

The observed execution order in the console output differs again from both the filter order and the
declaration order, because the runsettings enable class-level parallelism (`Workers: 24, Scope:
ClassLevel`). The first four results reported were `IsCoreInitialized_HasAnExplicitBackingField`,
`CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException`,
`WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` and
`CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException`, interleaving all three test
classes. A third distinct ordering passing as well is stronger evidence of order independence than
the reversed filter alone.

## Distinct control instance per test

The registry the fix introduces is process-wide (`private static readonly ConditionalWeakTable<WebView2, WebView2BreadcrumbHost> _owners`),
so two tests sharing one `WebView2` instance could couple through it. Every pump-hosted test in
`QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` constructs its own control. Measured by
counting `new WebView2()` occurrences within each method body:

| Test method | `new WebView2()` occurrences in the method body |
| --- | --- |
| `PostMessageJson_PostsExactlyOnceToTheUiContext` | 1 |
| `NavigateToString_PostsExactlyOnceToTheUiContext` | 1 |
| `SecondHost_DetachesThePredecessorAndTakesOwnership` | 1 |
| `PredecessorDetach_ToleratesNullCoreWebView2` | 1 |
| `ControlDisposed_DetachesTheHost` | 1 |
| `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | 1 |
| `InitializeAsync_PreservesAnInjectedDispatcher` | 1 |
| `PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` | 1 |

Every one of those controls is constructed on the pump thread through `host.InvokeAsync(...)`.

Eight methods, eight controls, no shared instance and no class-level or assembly-level fixture
holding one. `SecondHost_DetachesThePredecessorAndTakesOwnership` deliberately constructs two hosts
over its single control, which is the behaviour under test, not a shared-instance coupling: that
control is local to the method and each host is disposed with it in the method's `finally` block.

The three tests in `WebView2BreadcrumbHostContractTests.cs` and the four in
`WebView2CoreInitializerTests.cs` construct no `WebView2` at all; they assert by reflection or over a
freshly constructed `WebView2CoreInitializer`, so they cannot couple through the registry.

## Corroboration from the full-suite run

The same fifteen tests also passed inside the 6734-test full-suite run recorded in
`evidence/qa-gates/qa-4-tests-coverage.2026-08-27T23-17.md`, where they ran interleaved with the whole
`QuickFiler.Test` assembly under class-level parallelism rather than in a fifteen-test filtered run.
