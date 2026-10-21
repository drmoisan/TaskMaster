# Remediation QA Gate — Full Nine-Assembly Suite Run

Timestamp: 2026-08-23T19-21

Command:
```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' `
    QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    SVGControl.Test\bin\Debug\SVGControl.Test.dll `
    Tags.Test\bin\Debug\Tags.Test.dll `
    TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
    TaskTree.Test\bin\Debug\TaskTree.Test.dll `
    TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
    ToDoModel.Test\bin\Debug\ToDoModel.Test.dll `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
    /EnableCodeCoverage /InIsolation /Logger:trx `
    /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/r1-p3-t6 `
    /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Run from the worktree root, invoked through `pwsh -NoProfile` and launched per the Phase 3
long-running command mechanic: a detached `pwsh -NoProfile` runner invoked `Start-Process -PassThru`
with `-RedirectStandardOutput coverage\suite-remediation.log` and `-RedirectStandardError
coverage\suite-remediation.err.log`, recorded the child PID, then polled to completion. The recorded
exit code is taken from the returned process object's `ExitCode` property.

EXIT_CODE: 0

Output Summary:

| Measure | Value | Required |
| --- | --- | --- |
| Launched PID (`vstest.console.exe` child) | **61900** | recorded |
| Exit code (from the process object's `ExitCode`) | **0** | recorded |
| Total tests | **6459** | at least 6,000 |
| Passed | **6459** | — |
| Failed | **0** | — |
| Skipped / not executed | **0** | — |
| TRX files in the results subdirectory | **1** | exactly 1 |
| `QuickFiler.Test` failed count | **0** | exactly 0 |
| Wall time | 51.7069 s | — |
| Log file | `coverage\suite-remediation.log` (6,473 lines) | — |

vstest summary block, verbatim:

```
Test Run Successful.
Total tests: 6459
     Passed: 6459
 Total time: 51.7069 Seconds
```

TRX `ResultSummary/Counters`, verbatim:

```
<Counters total="6459" executed="6459" passed="6459" failed="0" error="0" timeout="0"
          aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0"
          notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

## TRX path

`docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16_net481.trx`

The leading two segments of the filename are the default `vstest.console.exe` TRX naming, which
embeds the account and machine name. They are redacted here as `<account>` and `<host>` per the
repository's host-identifier hygiene rule. The whole `r1-p3-t6/` subdirectory is excluded by the
evidence `.gitignore` line `r1-p*-t*/` appended by P0-T9, and its contents are deleted by P4-T10.

## All nine assemblies loaded

The TRX `TestDefinitions` reference all nine expected assemblies, confirming none was silently
dropped:

| Assembly | Passed | Failed |
| --- | --- | --- |
| `QuickFiler.Test.dll` | 925 | 0 |
| `SVGControl.Test.dll` | 75 | 0 |
| `Tags.Test.dll` | 65 | 0 |
| `TaskMaster.Test.dll` | 367 | 0 |
| `TaskTree.Test.dll` | 51 | 0 |
| `TaskVisualization.Test.dll` | 163 | 0 |
| `ToDoModel.Test.dll` | 122 | 0 |
| `UtilitiesCS.Test.dll` | 4690 | 0 |
| `VBFunctions.Test.dll` | 1 | 0 |
| **Total** | **6459** | **0** |

The mass-failure signature that indicates a missing `/InIsolation` — roughly 1,695 failures with
empty messages and sub-millisecond durations — did not occur. `/InIsolation` was present and the
run needed no correction or re-run.

## The four owned named tests

| Test | Outcome |
| --- | --- |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | Passed |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | Passed |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | Passed |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` | Passed |

## Sibling-assembly failures

**None.** The `UtilitiesCS.Test` assembly reported 4,690 passed and 0 failed in this run, so the
three pre-existing flakes tracked as issue #594 did not fire. No failure outside `QuickFiler.Test`
needed to be listed or attributed. The suite was run on an otherwise idle machine, which is the
condition under which those flakes do not reproduce; they remain a real, separately tracked risk
under load and are not claimed repaired by this cycle.

## Acceptance conditions

1. `EXIT_CODE:` taken from the process object's `ExitCode` — met, value 0.
2. The subdirectory holds exactly one TRX file — met.
3. Total is at least 6,000, confirming all nine assemblies loaded — met, 6,459 across nine
   assemblies.
4. `QuickFiler.Test` failed count is exactly 0 — met.
5. All four owned named tests recorded as passed — met.
6. Every failure outside `QuickFiler.Test` listed and attributed to issue #594 — vacuously met,
   there were none.
