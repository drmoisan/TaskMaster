# [P2-T2] — Authoritative Fail-Before Record, All Eleven Tests

Timestamp: 2026-08-27T20-21

Command:
```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException|FullyQualifiedName~CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException|FullyQualifiedName~EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException|FullyQualifiedName~IsCoreInitialized_HasAnExplicitBackingField|FullyQualifiedName~PostMessageJson_PostsExactlyOnceToTheUiContext|FullyQualifiedName~NavigateToString_PostsExactlyOnceToTheUiContext|FullyQualifiedName~SecondHost_DetachesThePredecessorAndTakesOwnership|FullyQualifiedName~PredecessorDetach_ToleratesNullCoreWebView2|FullyQualifiedName~ControlDisposed_DetachesTheHost|FullyQualifiedName~InitializeAsync_InstallsUiDispatcherFromUiSyncContext|FullyQualifiedName~InitializeAsync_PreservesAnInjectedDispatcher" "/Logger:trx;LogFileName=p2-t2-fail-before-all-tests.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t2
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- **11 tests discovered, 11 failed, 0 passed.** `Total tests: 11` / `Failed: 11` /
  `Test Run Failed.` No `Passed:` line was emitted.
- **Zero build errors.** The immediately preceding `[P2-T1]` build recorded `0 Error(s)` and
  `EXIT_CODE=0`, so every failure below is an assertion-time or runtime failure against the unfixed
  production behaviour, not a compile error.
- The results directory holds **exactly one** TRX, `p2-t2-fail-before-all-tests.trx`, whose
  `<Counters>` reads `total="11" executed="11" passed="0" failed="11" error="0" timeout="0"
  aborted="0"`.

## Per-test failure record

| # | Test | Failure message |
| --- | --- | --- |
| 1 | `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | `Expected a <System.ArgumentNullException> to be thrown ... but no exception was thrown.` |
| 2 | `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | `Expected System.ArgumentException ... but no exception was thrown.` |
| 3 | `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | `Expected a <System.ArgumentNullException> to be thrown ... but found <System.NullReferenceException>` |
| 4 | `IsCoreInitialized_HasAnExplicitBackingField` | `Expected explicitField not to be <null> because IsCoreInitialized must be backed by an explicit private field so Volatile.Read and Volatile.Write can be applied to it.` |
| 5 | `PostMessageJson_PostsExactlyOnceToTheUiContext` | `Expected recording.PostCount to be 1 ... but found 0 (difference of -1).` |
| 6 | `NavigateToString_PostsExactlyOnceToTheUiContext` | `Test method ... threw exception: System.InvalidOperationException: The instance of CoreWebView2 is uninitialized and unusable.` (thrown from `WebView2BreadcrumbHostTests.cs:line 105`, the un-marshalled inline call) |
| 7 | `SecondHost_DetachesThePredecessorAndTakesOwnership` | `Expected first.IsAttached to be False ... but found True.` |
| 8 | `PredecessorDetach_ToleratesNullCoreWebView2` | `Expected first.IsAttached to be False because the predecessor must still be detached even when it never completed initialization, but found True.` |
| 9 | `ControlDisposed_DetachesTheHost` | `Expected subject.IsAttached to be False because a disposed control must leave no attached host and no registry entry behind, but found True.` |
| 10 | `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | `Expected subject.HasUiDispatcher to be True because InitializeAsync must build the dispatcher from its uiSyncContext argument, but found False.` |
| 11 | `InitializeAsync_PreservesAnInjectedDispatcher` | `Expected recording.PostCount to be 1 ... but found 0 (difference of -1).` |

Every failure is attributable to the specific defect its test targets:

- Rows 1-3 (#477 defect 2): no argument validation exists, so the null and whitespace arguments are
  forwarded to the SDK or dereferenced.
- Row 4 (#476 defect 2): `IsCoreInitialized` is still the plain auto-property at
  `WebView2BreadcrumbHost.cs:54`, so no explicit backing field exists.
- Rows 5, 6, 11 (#476 defect 1): the SDK is touched inline on the caller's thread, so the recording
  context observes zero posts. Row 6 additionally reaches the SDK and throws, because
  `WebView2.NavigateToString` rejects an uninitialized `CoreWebView2` — direct evidence that the
  un-marshalled call really does reach the control.
- Rows 7-9 (#458): nothing ever detaches a host, because the constructor-side `-=` is bound to the
  instance under construction and matches no subscription.
- Row 10 (#476 defect 1, variant V1): `InitializeAsync` installs no dispatcher.

No behavioural fix precedes this record. This artifact is the authoritative fail-before evidence for
the whole eleven-test set; the compile-red artifacts from `[P1-T3]` through `[P1-T8]` are
supplementary.

## Artifact hygiene

TRX written with an explicit `LogFileName=`. Host identifiers embedded by vstest were replaced in
place (`REPO-ROOT`, `USER`, `HOST`); `<Counters>` and every failure record are unmodified. The empty
`Deploy_*` and per-result directories vstest created were removed.
