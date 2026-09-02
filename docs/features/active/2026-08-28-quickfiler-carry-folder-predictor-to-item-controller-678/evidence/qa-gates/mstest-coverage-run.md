# P2-T5 — Post-change MSTest coverage run

Timestamp: 2026-09-01T23-12

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
EXIT_CODE: 0

The command was run unconditionally. It discovered 9 test assemblies and invoked one
`vstest.console.exe` under `dotnet-coverage collect`, carrying
`/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation` and
`/TestCaseFilter:TestCategory!=LiveOutlook`.

## Output Summary

The byte-identical command was run twice. Both runs are recorded; the second is the result of
record. This is a characterisation of the same environmental flake P0-T8 recorded, not a silent
retry-until-green.

### Attempt 1 — HUNG, not completed

Produced 1286 test results, then stopped producing output. Diagnosed as hung rather than slow by two
CPU samples taken roughly eight minutes apart: the transcript result count stayed frozen at 1286 and
the `testhost` process CPU counter moved **24.109 -> 24.297** CPU-seconds, that is by 0.19 seconds
across the whole window.

Attempt 1 recorded **16 failures, every one a 60000 ms `[Timeout]` expiry**, and every one in the
`WinFormsPumpHost` harness or `UiThread` dispatcher-scope cluster:

```
BuildPumpHarness_DoesNotCreateTheWebViewChildHandles
BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread
CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults
InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink
InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException
Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread
Transaction_DisposedTwice_DoesNotOverReleaseTheGate
Transaction_SecondCallerCannotInstallUntilTheFirstRestores
```

**All 16 are a subset of the 17 that timed out in the P0-T8 baseline attempt 1**, on the identical
tree at the base ref before any change was made. The one baseline name absent from this list,
`EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt`, timed out at baseline and
did not here, which is itself evidence of the class's nondeterminism. **Not one of the 16 is a test
this change touched, added or is named by any acceptance criterion.** Every one failed by wall-clock
timeout, none by assertion. No `Done. Coverage artifact:` line was printed and no coverage document
was produced.

Remediation: the `dotnet-coverage` -> `vstest.console` -> `testhost` chain owned by this run was
terminated by PID. Two unrelated `vstest.console.exe` processes (parent 62344, started the previous
day) are Visual Studio TestWindow hosts, were present during every run in this plan including the
passing ones, and were deliberately **not** terminated. No file in the worktree was changed between
the two attempts, so the re-run is the identical command against the identical tree and is not a
toolchain-loop restart.

### Attempt 2 — the result of record

```
Test Run Successful.
Total tests: 6946
     Passed: 6946
 Total time: 27.2090 Seconds
Code coverage results: <worktree>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <worktree>\coverage\coverage.cobertura.xml
```

## Acceptance conditions

### 1. `EXIT_CODE:` recorded

`EXIT_CODE: 0`.

### 2. Whether the run printed the literal `Done. Coverage artifact:`

**It did.** That line is emitted only after post-processing and the on-disk write both succeed, so
the report on disk is post-processed and Derivation D4 is not required for the post-change side
either. Both sides of the coverage comparison therefore came from the same path.

### 3. Total, passed, failed and skipped counts, recorded numerically

| Count | Value |
|---|---:|
| Total | **6946** |
| Passed | **6946** |
| Failed | **0** |
| Skipped | **0** |

Failed is 0 by direct measurement: a scan of the transcript for lines beginning `  Failed ` returned
**0** matches. Skipped is 0 because vstest printed no `Skipped:` line, which it emits only for a
non-zero count.

### 4. The failing set is a subset of `BASELINE_FAILURE_SET` and contains no test from the four named files

`BASELINE_FAILURE_SET` is the empty set (P0-T8). The post-change failing set is also **empty**, and
the empty set is a subset of the empty set, so the condition holds in its strongest form rather than
by the subset escape.

Consequently it contains no test declared in
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`,
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` or
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`.

### Discovery control

"Name X is absent from the failure list" is also satisfied by X never running, so the task requires
a discovery control in addition.

| Measurement | Value |
|---|---:|
| P0-T8 baseline total discovered | 6938 |
| `[TestMethod]` declarations added by P1-T3, P1-T8 and P1-T9 | **4** |
| `[TestMethod]` declarations added by P1-T6 (leg B) | 4 |
| Total added by this plan | 8 |
| Required minimum (6938 + 4) | 6942 |
| Post-change total discovered | **6946** |

6946 >= 6942, so the condition holds. The stated integer for the three tasks the condition names is
**4**: one from P1-T3, one from P1-T8 and two from P1-T9. The plan's condition names only those
three tasks; P1-T6 added four more, which is why the observed total exceeds the required minimum by
exactly four. The full accounting 6938 + 8 = 6946 matches the independent `[TestMethod]` census in
`test-reconciliation.md` (1276 -> 1284, also +8), so no test was silently lost.

### Each of the four named tests is present in the executed-test list by name

Recorded by name rather than merely absent from the failure list, transcribed from the run
transcript:

```
  Passed LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore [< 1 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory [6 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory [971 ms]
  Passed AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder [2 ms]
```

- `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` — P1-T3, re-run
  green by P1-T7. **Present, Passed.**
- `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory` —
  P1-T8. **Present, Passed.**
- `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` — P1-T9.
  **Present, Passed.**
- `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore` — P1-T7. **Present,
  Passed.** This is the source-text test that reads
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` from disk and asserts five string
  literals against it; it passed after the P2-T1 reformat, so no asserted literal was moved or
  reflowed.
