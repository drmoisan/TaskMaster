# P3-T5 — Pump-Hosted Consumer Tests

Timestamp: 2026-08-22T10-36

Command:
```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx `
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p3-t5 `
  /TestCaseFilter:"FullyQualifiedName~ThroughThePumpHost|FullyQualifiedName~WithFaultingWebViewSeam|FullyQualifiedName~WithInjectedSeams"
```

EXIT_CODE: 0

Output Summary:

TRX: `evidence/regression-testing/p3-t5/2026-08-22_10_36_21_net481.trx`

TRX `<Counters>` verbatim:

```
total="8" executed="8" passed="8" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
```

| # | Test | Outcome | Duration |
| --- | --- | --- | --- |
| 1 | `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | Passed | 1 s |
| 2 | `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | Passed | 132 ms |
| 3 | `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | Passed | 146 ms |
| 4 | `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | Passed | 164 ms |
| 5 | `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | Passed | 515 ms |
| 6 | `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` | Passed | 108 ms |
| 7 | `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` | Passed | 111 ms |
| 8 | `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` | Passed | 128 ms |

Acceptance: recorded failed count is **0**; recorded executed count is **8**, which is at least 8.

## Note on the anticipated side effect

The task anticipates that forcing the handle flips `Theme.cs:433` `_lblItemNumber.InvokeRequired`
and `ViewerSetup.cs:361` `_itemViewer.InvokeRequired` from `false` to `true` on off-pump evaluation,
so those paths marshal instead of running inline.

That flip is not observable as a behavioural change here, and the measurement in
`webview-child-handle-measurement.2026-08-21T18-10.md` explains why: the viewer's window handle was
already being created before the Phase 2 change, as a side effect of the two WebView2 children's
`ISupportInitialize.EndInit()` calls during `ItemViewer` construction. `InvokeRequired` on an
off-pump thread was therefore already `true` for a harness viewer, and those paths were already
marshalling. What the Phase 2 change removes is the dependency on that third-party side effect
occurring, not the marshalling behaviour itself.

All eight tests pass either way, so no consumer regression is recorded.
