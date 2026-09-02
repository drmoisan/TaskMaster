# Phase 0 — baseline MSTest coverage run (P0-T8)

Timestamp: 2026-09-01T22-06

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
EXIT_CODE: 0

`-SearchRoot .` was supplied, as the task requires. The runner discovered 9 test assemblies and
invoked one `vstest.console.exe` under `dotnet-coverage collect`, carrying
`/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation` and
`/TestCaseFilter:TestCategory!=LiveOutlook`. No bare `vstest.console.exe` invocation was used.

## Output Summary

The byte-identical command was run twice. Both runs are recorded here; the second is the baseline of
record. This is a characterisation of a known environmental flake, not a silent retry-until-green.

### Attempt 1 — HUNG, not completed

Started 21:27, produced 1277 test results in roughly eight minutes, then stopped producing output.
Diagnosed as hung rather than slow by the documented method: the transcript line count stayed frozen
at 1277 results for 35 minutes while the `testhost` process CPU counter moved 26.45 -> 26.50 -> 26.73
-> 27.03 CPU-seconds, that is by hundredths of a second per sampling window, and the log file's last
write time stayed at 21:35:20 while wall-clock reached 21:57:46.

Attempt 1 recorded **17 failures, every one a 60000 ms `[Timeout]` expiry** and every one in the
`WinFormsPumpHost` harness or `UiThread` dispatcher-scope cluster:

```
BuildPumpHarness_DoesNotCreateTheWebViewChildHandles
BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread
CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt
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

Every one of the 17 failed by wall-clock timeout, none by assertion. No `Done. Coverage artifact:`
line was printed, and no coverage document was produced.

Remediation: the `dotnet-coverage` -> `vstest.console` -> `testhost` chain owned by this run was
terminated by PID (102284, 28032, 130332). Two unrelated `vstest.console.exe` processes (PIDs 24692
and 96760, parent 62344, started the previous day) are Visual Studio TestWindow hosts, were present
during both attempts, and were deliberately **not** terminated.

No file in the worktree was changed between the two attempts. The re-run is therefore not a
toolchain-loop restart: it is the identical command against the identical tree.

### Attempt 2 — the baseline of record

```
Test Run Successful.
Total tests: 6938
     Passed: 6938
 Total time: 26.9720 Seconds
Code coverage results: <worktree>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <worktree>\coverage\coverage.cobertura.xml
```

- The run **did** print the literal `Done. Coverage artifact:`. That line is emitted only after
  post-processing and the on-disk write both succeed, so the report on disk is post-processed and
  Derivation D4 is not required for the baseline side.
- Total: **6938**
- Passed: **6938**
- Failed: **0**
- Skipped: **0** (vstest printed no `Skipped:` line, which it emits only for a non-zero count)
- Zero timeouts. All 17 tests that timed out in attempt 1 passed in attempt 2.

## BASELINE_FAILURE_SET

```
(empty set)
```

The baseline failing set is empty. Later suite gates assert the post-change failing set is a subset
of this set, which for an empty baseline means the post-change failing set must also be empty.

## Interpretation of attempt 1

The 17 timeout-only failures are the known load-flaky `WinFormsPumpHost` / STA-pumping class,
amplified by coverage instrumentation and by `TaskMaster.cli.runsettings` requesting one worker per
logical processor at `ClassLevel`. They are an environmental scheduling flake and not a property of
the tree: the same 17 tests pass on the identical command with no intervening file change. They are
recorded here so that if any of them fails in the P2-T5 post-change run, it is attributable to this
class rather than treated as a regression caused by the change. That attribution does not lower the
P2-T5 gate: the subset assertion is against the empty set of record, so any post-change failure must
be characterised the same way before it can be dismissed.
