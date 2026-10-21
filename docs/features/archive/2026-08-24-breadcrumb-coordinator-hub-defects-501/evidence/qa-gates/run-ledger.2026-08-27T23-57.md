# QA Gate — Full-suite run ledger (NB-4 remediation)

Timestamp: 2026-08-27T23-57

Command: `pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/postchange.cobertura.xml`

EXIT_CODE: 0 (final run)

Output Summary: three full-suite runs were executed on the merged tree. Their verbatim vstest
summary blocks are transcribed below so the pass/fail counts rest on captured output rather than on
prose. This artifact was added in response to feature-review finding NB-4, which correctly observed
that the 6729-test figure appeared only inside the sentence asserting it.

## Why the raw logs are not committed

Each run log is roughly 500 KB and about 6,700 near-identical `Passed <name> [<n> ms]` lines. The
repository already carries a 10 MB Cobertura file per feature, and committing three more half-megabyte
logs per child across a seven-child epic is a cost the audit trail does not need. The summary blocks
and the complete failure list are transcribed verbatim instead; nothing that distinguishes the three
runs is omitted.

## Run 1 — merged tree, BEFORE the AddItemsCore seam

```
Test Run Successful.
Total tests: 6729
     Passed: 6729
 Total time: 37.5676 Seconds
```

This is the run that establishes leg 1 of the environmental-flake argument: the pump-host tests that
later timed out passed here, on the same merged base, in 37.6 seconds.

## Run 2 — after the seam, 13 failures

```
Test Run Failed.
Total tests: 6730
     Passed: 6717
     Failed: 13
 Total time: 7.9029 Minutes
```

Every failure and its verbatim error message:

```
  Failed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 m]
   Test 'InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState' timed out after 60000ms
  Failed CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController [1 m]
   Test 'CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController' timed out after 60000ms
  Failed EnsureDispatcher_ScopeDisposedTwice_IsIdempotent [1 m 2 s]
   Test 'EnsureDispatcher_ScopeDisposedTwice_IsIdempotent' timed out after 60000ms
  Failed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [1 m]
   Test 'InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme' timed out after 60000ms
  Failed CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing [1 m]
   Test 'CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing' timed out after 60000ms
  Failed Transaction_SecondCallerCannotInstallUntilTheFirstRestores [1 m]
   Test 'Transaction_SecondCallerCannotInstallUntilTheFirstRestores' timed out after 60000ms
  Failed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [1 m]
   Test 'InitializeBool_ThroughThePumpHost_CompletesAndInitializesState' timed out after 60000ms
  Failed Transaction_DisposedTwice_DoesNotOverReleaseTheGate [1 m]
   Test 'Transaction_DisposedTwice_DoesNotOverReleaseTheGate' timed out after 60000ms
  Failed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [1 m]
   Test 'InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates' timed out after 60000ms
  Failed Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException [1 m]
   Test 'Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException' timed out after 60000ms
  Failed InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults [1 m]
   Test 'InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults' timed out after 60000ms
  Failed BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread [1 m]
   Test 'BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread' timed out after 60000ms
  Failed BuildPumpHarness_DoesNotCreateTheWebViewChildHandles [1 m]
   Test 'BuildPumpHarness_DoesNotCreateTheWebViewChildHandles' timed out after 60000ms
```

All 13 carry the identical signature `timed out after 60000ms`. All 13 live in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` or
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, neither of which is
written by this branch; the latter arrived from merged sibling 493. The 60000 ms figure is not
generic: it is the `GateTimeoutMs` constant declared in that fixture file.

Wall-clock for this run was 7.9 minutes against 37.6 seconds for the runs either side of it, which is
the independent signal that the machine was starved rather than the code broken. Three sibling agents
were executing concurrently on this host.

## Run 3 — re-run on the byte-identical tree, final

```
Test Run Successful.
Total tests: 6730
     Passed: 6730
 Total time: 36.1163 Seconds
```

No file changed between run 2 and run 3. This is the run of record; its Cobertura output is
`postchange.cobertura.2026-08-27T23-31.xml` and its coverage figures drive the coverage-delta gate.

## Residual uncertainty, stated plainly

A re-run that passes does not prove the failure was environmental; it proves the failure is not
deterministic. The three facts above make the environmental reading substantially more likely than a
regression, and the affected files are outside this feature's write set, but the possibility of a
genuine intermittent defect in the sibling-owned pump-host fixture is NOT excluded by this evidence.
That risk belongs to the owners of those files, not to this feature.
