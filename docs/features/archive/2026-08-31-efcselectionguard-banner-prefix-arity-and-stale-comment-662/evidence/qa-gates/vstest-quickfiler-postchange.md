# QuickFiler.Test Full-Assembly Post-Change Run (P2-T7)

Timestamp: 2026-09-01T16-24

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=quickfiler-postchange.trx" /ResultsDirectory:docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/p2-t7`

EXIT_CODE: 0

Output Summary:

`<Counters ... />` line from the produced TRX:

```
<Counters total="1287" executed="1287" passed="1287" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

- total: 1287
- executed: 1287
- passed: 1287
- failed: 0

Console summary, transcribed:

```
Test Run Successful.
Total tests: 1287
     Passed: 1287
 Total time: 13.3681 Seconds
```

## Gate evaluation

The P0-T11 baseline recorded `failed="0"` for this assembly. This run's `failed`
attribute is 0, which does not exceed the baseline, so this step does not fail
and the Phase 2 loop does not restart from P2-T1 on account of it.

The `passed` count rose from the baseline's 1286 to 1287. The increment of
exactly one is the new test
`BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` added by
P1-T7; no pre-existing test was lost.

## Two runs recorded

This task was executed twice. Both are recorded rather than only the passing
one.

**Run 1 — hung, terminated, no TRX produced.**

Run 1 was launched with the identical command span. Its transcript stopped
growing at 1328 lines and did not advance for more than ten minutes. Process
sampling over a 60-second window showed the `testhost` process CPU moving only
from 24.05 to 24.08 seconds, that is, essentially idle rather than working. The
run was therefore hung rather than slow. The two processes belonging to this run
(`vstest.console` and `testhost`, started 16:02:29 and 16:02:30) were terminated;
no process belonging to any other session was touched. No TRX was written, so
run 1 has no `<Counters ... />` element and contributes no `failed` figure to the
gate.

Before termination, run 1's transcript recorded 15 failing tests, every one of
them a 60000 ms timeout. The 15 distinct test names were:

```
BuildPumpHarness_DoesNotCreateTheWebViewChildHandles
BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread
CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults
InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException
Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread
Transaction_DisposedTwice_DoesNotOverReleaseTheGate
Transaction_SecondCallerCannotInstallUntilTheFirstRestores
```

All 15 are `WinFormsPumpHost` harness tests or `UiThread` dispatcher-scope
tests. None of them exercises any of the four files this change edits:
`EfcSelectionGuard.cs`, `EfcFormController.cs`, `FolderSuggestionTree.cs`, and
`EfcSelectionGuardTests.cs`. Their failure mode was uniformly a wall-clock
timeout rather than an assertion failure, which is a scheduling symptom rather
than a behavioural one.

**Run 2 — the recorded run above.** All 15 of those tests passed, the assembly
completed in 13.4 seconds, and `failed` is 0.

**Characterisation.** The P0-T11 baseline ran the byte-identical command against
this same assembly and completed in 14.5 seconds with 1286/1286 passed and 0
failed, before any edit existed. Run 1's failures are therefore not attributable
to this change: the same tests pass both before the change (P0-T11) and after it
(run 2), and they failed only in the single hung run, all by timeout. The
failures are an environmental scheduling flake in the WinForms pump-host and
dispatcher tests, consistent with those tests' known load sensitivity. Run 2 was
not a silent retry-until-green: it was one re-run taken to characterise a
suspected flake, and both runs are recorded here.

## Staleness guard

- The results directory `.../evidence/qa-gates/p2-t7` was deleted before the run
  with `if (Test-Path $dir) { [System.IO.Directory]::Delete((Resolve-Path $dir).Path, $true) }`,
  where `$true` is the recursive flag.
- Produced TRX `LastWriteTime`: `Tuesday, September 1, 2026 4:24:15 PM`
  (16:24:15).
- P2-T1's `Timestamp:` for the current (final) loop pass: `2026-09-01T15-59`.
- 16:24:15 is later than 15:59, so the TRX belongs to the current pass. The
  BLOCKED branch does not arise, and P2-T19 may read this file as the final
  pass's counters.

## Invocation notes

The runsettings is the repository-root `TaskMaster.runsettings`, not the
`scripts\vscode` CLI variant, because `/EnableCodeCoverage` activates the Code
Coverage collector and only the repository-root file supplies that collector's
`Deedle` and `FSharp.Core` module exclusions. `/InIsolation` was supplied, per
Decisions Record D10.
