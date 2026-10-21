# Post-merge toolchain verification — QuickFiler.Test

Timestamp: 2026-08-28T00-47
Task: post-merge verification (mandated before [P5-T1]; not a numbered plan task)
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=postmerge-quickfiler-test.trx" /ResultsDirectory:<temp>\efc464-trx-postmerge` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1

## Result

```
Total tests: 1137
     Passed: 1123
     Failed: 14
Test Run Failed.
 Total time: 7.3202 Minutes
```

| Metric | Phase 0 baseline | After batch A | Post-merge | Delta vs batch A |
|---|---|---|---|---|
| Total executed | 1099 | 1111 | 1137 | +26 |
| Passed | 1099 | 1111 | 1123 | +12 |
| Failed | 0 | 0 | 14 | +14 |

The +26 total is the tests the merged siblings brought in. The 14 failures are analysed below.

## The 14 failures are base-introduced load-driven flakiness, not a feature regression

Every one of the 14 failures lives in a `QfcItemController.*` test file this feature does not own and
does not touch. Distinct failing result names, with their owning file:

| Failing test | File |
|---|---|
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` |
| `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` |
| `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` |
| `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` |
| `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` |
| `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` |
| `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` |

Three files, all `QfcItemController.*`, all outside constraint C1's owned set. Every failure reports a
duration of approximately one minute, which is the WinFormsPumpHost / dispatcher-fixture timeout, not an
assertion failure.

### Isolation re-run — all pass

Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BuildPumpHarness|FullyQualifiedName~CreateAsync_WithFaultingWebViewSeam|FullyQualifiedName~CreateSequentialAsync_WithInjectedSeams|FullyQualifiedName~EnsureDispatcher_|FullyQualifiedName~InitializeAsync_ThroughThePumpHost|FullyQualifiedName~InitializeBool_ThroughThePumpHost|FullyQualifiedName~InitializeGraphicsAsync_ThroughThePumpHost|FullyQualifiedName~InitializeNineArgOverload_ThroughThePumpHost|FullyQualifiedName~InitializeSequentialAsync_ThroughThePumpHost|FullyQualifiedName~Install_CalledTwiceOnTheSameTransaction|FullyQualifiedName~Transaction_DisposedTwice|FullyQualifiedName~Transaction_SecondCallerCannotInstall"`

EXIT_CODE: 0

```
Test Run Successful.
Total tests: 15
     Passed: 15
 Total time: 3.0961 Seconds
```

The same tests that timed out at ~60 s each under the full-assembly run complete in 56-73 ms each when
scoped. That is the load-driven WinFormsPumpHost/dispatcher-fixture flakiness class, not a behavioural
defect and not attributable to this feature's diff.

## Classification

**BASE-INTRODUCED. Not fixed here.** Per the ownership constraints, `QfcItemController.*Tests.cs` files
are explicitly forbidden to this feature by constraint C1 and are LIVE under sibling #489. Recording and
continuing, as directed.

Output Summary: 1137 executed, 1123 passed, 14 failed. All 14 failures are load-driven timeouts in three
`QfcItemController.*` test files outside this feature's owned set; all 15 of those tests pass in an
isolated scoped re-run (EXIT_CODE 0, 3.1 s). Classified base-introduced; not remediated here.
