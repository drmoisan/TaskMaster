# [P5-T6] Full `QuickFiler.Test` suite after the issue #473 defect 2 fix

Timestamp: 2026-08-26T10-40

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p5-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p5-t6
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 946
     Passed: 946
 Total time: 10.7060 Seconds
```

TRX `<Counters>` (`p5-t6.trx`):

```
total="946" executed="946" passed="946" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Failed count | exactly 0 | **0** |

### Count progression

| Gate | Total | Passed | Failed |
|---|---|---|---|
| P0-T14 baseline (`QuickFiler.Test` only) | 938 | 938 | 0 |
| P1-T8 | 938 | 938 | 0 |
| P2-T11 | 939 | 939 | 0 |
| P3-T6 | 941 | 941 | 0 |
| P4-T8 | 943 | 943 | 0 |
| P5-T6 (this run) | **946** | **946** | **0** |

The `+3` over P4-T8 is exactly the three issue #473 defect 2 tests added by P5-T1, P5-T2 and P5-T4.
Cumulatively Phases 2 through 5 added eight tests to a 938-test baseline, and no pre-existing test
was removed, renamed, or newly failed.

### First attempt: the same nine environment-induced timeouts

The first execution of this command returned `EXIT_CODE 1` with `946 total / 937 passed / 9 failed`
in 7.3400 minutes. Its TRX is retained alongside the passing one as `p5-t6-attempt1-flaky.trx`.

The nine failures are the *same nine tests*, with the same `Test '<name>' timed out after 60000ms`
message form, that failed on the first P2-T11 attempt: `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`,
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`,
`InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`,
`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`,
`InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`,
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`,
`CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing`,
`InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`, and
`CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController`.

Every one drives `QfcItemController` through the WinForms pump host. This phase changed
`TryMoveEmailByGroupAsync` and `TryMoveEmailByGroupIndexAsync` on `QfcCollectionController`; it
touched no `QfcItemController` code path and no pump host. `9 x 60000ms` again accounts for almost
exactly the gap between the first attempt's 7.3400 minutes and the passing run's 10.7060 seconds,
and they passed on re-run with the source tree unchanged. The machine hosts unrelated concurrent
work and still carries seventeen orphaned MSBuild worker processes from an earlier crashed run,
which the environment constraint in force forbids terminating. This is the same recurring
load-sensitivity documented against P2-T11, not a regression from this change.

The build precondition returned `EXIT_CODE 0` with `0 Error(s)` before both attempts, so neither was
blocked by a locked `obj/` or `bin/` output.

Host-identifier sanitisation was applied to both TRX files exactly as recorded in the P2-T6
artifact. A post-substitution scan of each for the bare account name, the machine name in either
casing, the workspace absolute path, and the user-profile path returns zero hits. The empty
`/InIsolation` deployment scratch directories, whose names embed the account and machine name, were
removed.

Result: PASS.
