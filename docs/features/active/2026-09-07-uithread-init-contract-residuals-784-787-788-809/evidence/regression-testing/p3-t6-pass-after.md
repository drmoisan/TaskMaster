# [P3-T6] Pass-after evidence for the Phase 2 regression tests

Timestamp: 2026-09-08T02-08

Command: identical to [P2-T10] except for the log file name and the results directory.

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p3t6b.trx' '/ResultsDirectory:TestResults\809-p3t6b' '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitApartmentContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests'
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 22
     Passed: 22
```

TRX selected: `p3t6b.trx`, `LastWriteTimeUtc` `2026-09-08T04:52:22.2023361Z`.

TRX `ResultSummary/Counters`: `total` 22, `executed` 22, `passed` 22, `failed` 0.

SKIPPED_DERIVED: 0

The count of rows whose outcome is `Failed` is **0**.

## One row per test method, same table shape as the fail-before artifact

| Fully-qualified test | Outcome |
|---|---|
| `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` | Passed |
| `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` | Passed |
| `UiThreadInitApartmentContract_Tests.Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields` | Passed |
| `UiThreadInitApartmentContract_Tests.Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` | Passed |
| `UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | Passed |
| `UiThreadInitRetryContract_Tests.Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` | Passed |
| `UiThreadInitRetryContract_Tests.AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` | Passed |
| `UiThreadInitRetryContract_Tests.Init_CalledConcurrentlyFromTwoStaThreads_InvokesTheFactoryExactlyOnce` | Passed |
| `UiThreadInitRetryContract_Tests.Init_WithMonitorUiThreadEnabled_ConstructsAndRunsTheThreadMonitorWithTheInjectedTimeProvider` | Passed |
| `UiThreadInitRetryContract_Tests.UiSyncContext_ReadWithNullBackingFieldFromStaThread_InitializesThroughTheLazyPath` | Passed |
| `SynchronizationContextAwaiter_Tests.Constructor_NullContext_ThrowsArgumentNullException` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenContextIsNotCurrent_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenContextMatchesCurrent_ReturnsTrue` | Passed |
| `SynchronizationContextAwaiter_Tests.GetResult_DoesNotThrow` | Passed |
| `SynchronizationContextAwaiter_Tests.OnCompleted_PostsCallbackToContext` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue` | Passed |

## The six fail-before rows are each present with outcome `Passed`

1. `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` — Passed
2. `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` — Passed
3. `UiThreadInitApartmentContract_Tests.Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` — Passed
4. `UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` — Passed
5. `UiThreadInitRetryContract_Tests.AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` — Passed
6. `SynchronizationContextAwaiter_Tests.IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` — Passed

The fail-before evidence of record for all six is the re-measured pass recorded under the heading `Correction and re-measurement (recorded during [P3-T6])` in `p2-t10-fail-before.md`. An earlier attempt at this task, logged as `p3t6.trx`, reported 20 passed and 2 failed; the two failures were the defective ambient-apartment premise in the two `Init_OnMtaThread_...` methods, not a failure of the fix, and that finding is what drove the correction recorded there.
