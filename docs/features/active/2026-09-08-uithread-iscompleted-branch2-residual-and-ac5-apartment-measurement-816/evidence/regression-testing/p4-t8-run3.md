# P4-T8 — Repetition 3 of the full two-assembly test run

Timestamp: 2026-09-13T23-47

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p4-t8-run3.trx" /ResultsDirectory:coverage\trx\p4-t8 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

Output Summary:

The run printed `Test Run Successful.` and completed in 53.4724 seconds.

| Count | Value | Read or derived |
|---|---|---|
| Total | 6336 | Read, from the `Total tests:` line |
| Passed | 6336 | Read, from the `Passed:` line |
| Failed | 0 | Derived: the summary carries no `Failed:` line. Corroborated by the TRX `Counters` `failed` attribute, which is 0 |
| Skipped | 0 | Derived: Total minus Passed minus Failed. No `Skipped:` line is printed, and the TRX `notExecuted` attribute is hard-coded to 0 by the TRX logger so it is not used |

TRX `Counters` for cross-reference: `total=6336 executed=6336 passed=6336 failed=0 notExecuted=0`.

### Named outcomes

| Fully qualified test | Outcome |
|---|---|
| `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` | **Passed** |
| `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` | **Passed** |
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | **Passed** |
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | **Passed** |
| `UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` | **Passed** |
| `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` | **Passed** |

No test in either assembly was recorded Failed, so neither FAIL condition is met. The
non-deterministic-test clause was not triggered: no re-run was performed for this repetition and no
`ExpectedExitCode` is declared.

The blame collector wrote no `Sequence_*.xml` file under `coverage\trx\p4-t8` (count: 0), so the run
did not stall.

Standard output of the measurement test in this repetition:

```
MTA_GUARD_APARTMENT: MTA
MTA_INITIALIZE_OUTCOME: COMPLETED
```

The run document and the binary coverage attachment are written under the gitignored coverage
directory. Neither is committed and neither is named as an evidence artifact.
