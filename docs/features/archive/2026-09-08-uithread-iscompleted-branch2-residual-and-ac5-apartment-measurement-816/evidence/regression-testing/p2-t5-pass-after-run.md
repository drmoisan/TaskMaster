# P2-T5 — Pass-after run against the hardened predicate

Timestamp: 2026-09-13T23-28

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t5-pass-after.trx" /ResultsDirectory:coverage\trx\p2-t5 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

Output Summary:

The run printed `Test Run Successful.` and completed in 32.9902 seconds.

| Count | Value | Read or derived |
|---|---|---|
| Total | 4907 | Read, from the `Total tests:` line |
| Passed | 4907 | Read, from the `Passed:` line |
| Failed | 0 | Derived: the summary carries no `Failed:` line. Corroborated by the TRX `Counters` `failed` attribute, which is 0 |
| Skipped | 0 | Derived: Total minus Passed minus Failed. The summary carries no `Skipped:` line, and the TRX `notExecuted` attribute is hard-coded to 0 by the TRX logger so it is not used |

TRX `Counters` element for cross-reference: `total=4907 executed=4907 passed=4907 failed=0 notExecuted=0`.

### The seven named outcomes

| # | Fully qualified test | Recorded outcome |
|---|---|---|
| 1 | `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | **Passed** |
| 2 | `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | **Passed** |
| 3 | `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` | **Passed** |
| 4 | `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` | **Passed** |
| 5 | `UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | **Passed** |
| 6 | `UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests.Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` | **Passed** |
| 7 | `UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` | **Passed** |

All seven are Passed, so the FAIL condition is not met. The seven named outcomes, not the
whole-assembly exit code, are this task's gate; the exit code happens also to be zero.

No test in the assembly was recorded Failed, so the second FAIL condition — any test other than
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
being among the Failed set — is vacuously not met, and that non-deterministic test was itself
recorded Passed in this run.

### Standard output of the measurement test

```
MTA_GUARD_APARTMENT: MTA
MTA_INITIALIZE_OUTCOME: COMPLETED
```

This is the section P3-T1 reads the two recorded values from.

The run document is written under the gitignored coverage directory. It is neither committed nor
named as an evidence artifact.
