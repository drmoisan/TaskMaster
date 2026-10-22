# P1-T12 — Behaviour-neutrality gate over the open and close lifecycle suites

Timestamp: 2026-09-07T14-26
Task: [P1-T12]
Issue: #796
Channel used: A

Command: the P1-T11 command form with the results directory
`TestResults\796\p1-t12`, the log file name `p1-t12.trx`, and the four-class filter:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests" /ResultsDirectory:TestResults\796\p1-t12 "/Logger:trx;LogFileName=p1-t12.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## Run summary, verbatim

```
Test Run Successful.
Total tests: 59
     Passed: 59
 Total time: 2.5469 Seconds
```

## Acceptance

| Condition | Observed |
|---|---|
| `EXIT_CODE: 0` | 0 |
| No test recorded as Failed | none; the run printed no `Failed:` line and no `Failed` result line |
| Recorded Total of at least 1 | 59 |

The Total of 59 is well above 1, so the gate did not pass on an empty population.
Phase 1 adds no test to any of these four classes.

## Tests that directly exercise the relocated handler

Four of the 59 assert on the native-close path that P1-T2 moved into the diagnostics
part, and all four passed:

```
  Passed NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications [< 1 ms]
  Passed FinishClose_DropDownClosedPath_PredicateFalse_DoesNotFocusAnchor [2 ms]
  Passed NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce [46 ms]
  Passed AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce [109 ms]
```

These are the assertions that would have detected a behavioural change in the move:
the handler is still bound through the `DropDown.Closed += OnDropDownClosed;`
subscription that stayed in the main part, its guard still suppresses a repeat close,
and its scheduled continuation still reaches `FinishClose` with the Uncommitted
reason.

Phase 1 changes no behaviour these suites exercise, so a failure here would have
meant the instrumentation phase changed behaviour and the phase would have had to be
reverted rather than accepted. There was none.

## Raw output

The TRX is written to the gitignored path TestResults/796/p1-t12/p1-t12.trx and is
never committed.
