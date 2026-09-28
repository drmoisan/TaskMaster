# P1-T8 — Fail-before run against the unmodified predicate [expect-fail]

Timestamp: 2026-09-13T23-21

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t8-fail-before.trx" /ResultsDirectory:coverage\trx\p1-t8 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

The run printed `Test Run Failed.` and completed in 37.2740 seconds. A failing run is the expected
outcome of this task: the hardening does not land until P2-T1.

| Count | Value | Read or derived |
|---|---|---|
| Total | 4907 | Read, from the `Total tests:` line |
| Passed | 4905 | Read, from the `Passed:` line |
| Failed | 2 | Read, from the `Failed:` line |
| Skipped | 0 | Derived: Total minus Passed minus Failed. The summary carries no `Skipped:` line, and the TRX `notExecuted` attribute is hard-coded to 0 by the TRX logger so it is not used for this value |

TRX `Counters` element for cross-reference: `total=4907 executed=4907 passed=4905 failed=2 notExecuted=0`.

The two tests the run recorded Failed are exactly the two negative regression tests; no other test
in the assembly was recorded Failed and no test was recorded with any outcome other than Passed or
Failed.

### The five named outcomes

| Fully qualified test | Recorded outcome |
|---|---|
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | **Failed** |
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | **Failed** |
| `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` | **Passed** |
| `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` | **Passed** |
| `UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` | **Passed** |

Neither negative test was recorded Passed, Skipped or not discovered, and the positive twin was
recorded Passed, so no FAIL condition is met.

### Failure message text of the two negative tests

Both carry the identical FluentAssertions message:

```
Expected observed to be False, but found True.
```

That is the defect this delivery fixes, observed directly: against the unmodified predicate the
captured-UI-context exit returns `true` on a thread that does not own the captured UI dispatcher,
and returns `true` again when no UI dispatcher was captured at all.

### Standard output of the measurement test

```
MTA_GUARD_APARTMENT: MTA
MTA_INITIALIZE_OUTCOME: COMPLETED
```

Recorded here because it is present in this run's document; the authoritative measurement record is
written by P3-T1 from the P2-T5 run.

The run document is written under the gitignored coverage directory. It is neither committed nor
named as an evidence artifact.
