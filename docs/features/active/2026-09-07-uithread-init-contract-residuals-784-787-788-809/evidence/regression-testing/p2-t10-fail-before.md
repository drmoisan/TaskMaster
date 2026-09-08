# [P2-T10] Fail-before evidence for the Phase 2 regression tests

Timestamp: 2026-09-08T01-44

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p2t10.trx' '/ResultsDirectory:TestResults\809-p2t10' '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitApartmentContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests'
```

EXIT_CODE: 1
ExpectedExitCode: 1

This task is tagged `[expect-fail]`. A failing run is the expected outcome for this task only; the formatting, analyzer and nullable gates in [P2-T9] all passed before it ran.

## Output Summary

```
Total tests: 22
     Passed: 16
     Failed: 6
Test Run Failed.
```

TRX selected: `p2t10.trx`, `LastWriteTimeUtc` `2026-09-08T04:44:09.3224896Z`.

TRX `ResultSummary/Counters`: `total` 22, `executed` 22, `passed` 16, `failed` 6.

SKIPPED_DERIVED: 0

Twenty-two is the count the plan derives: four in `UiThreadInitApartmentContract_Tests` from [P2-T2], six in `UiThreadInitRetryContract_Tests` from [P2-T3], [P2-T4] and [P2-T5], and twelve in `SynchronizationContextAwaiter_Tests`, being the five that existed before this delivery plus the seven [P2-T8] added.

## One row per test method discovered in the three classes

| Fully-qualified test | Outcome |
|---|---|
| `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` | Failed |
| `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` | Failed |
| `UiThreadInitApartmentContract_Tests.Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields` | Passed |
| `UiThreadInitApartmentContract_Tests.Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` | Failed |
| `UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | Failed |
| `UiThreadInitRetryContract_Tests.Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` | Passed |
| `UiThreadInitRetryContract_Tests.AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` | Failed |
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
| `SynchronizationContextAwaiter_Tests.IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` | Failed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` | Passed |
| `SynchronizationContextAwaiter_Tests.IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue` | Passed |

## The set of `Failed` rows is exactly the six the plan names

1. `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` — red because `Init()` carries no apartment precondition today, so it does not throw from MTA.
2. `UiThreadInitApartmentContract_Tests.Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` — red for the same reason: `Init()` does not throw, and the four monitoring-configuration assignments at `UtilitiesCS/Threading/UiThread.cs:26-35` run on every call ahead of the latch, so the fields do not stay at their reset values.
3. `UiThreadInitApartmentContract_Tests.Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` — red because the MTA leg records no exception; there is no boundary today.
4. `UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` — red because the latch at `UiThread.cs:36` is consumed before `Initialize()` runs, so the retry is a silent no-op and the four capture fields stay unset.
5. `UiThreadInitRetryContract_Tests.AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` — red because the later lazy read raises no apartment exception: `Init()` is a no-op on the consumed latch and the accessor falls back to `SizeF(1f, 1f)`.
6. `SynchronizationContextAwaiter_Tests.IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` — red because `UiThread.cs:100` compares contexts by reference, so a dispatcher context evaluated against a different ambient returns false.

No further row is `Failed`, and none of the six is absent.

## Why the remaining sixteen are already green

Sixteen is twenty-two minus six.

- `Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields` — `Init()` already succeeds from STA and already captures all four values through the Phase 1 factory seam.
- `Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` — a throwing `Initialize()` already propagates without assigning any capture field; only the retry is broken today, not the failure itself.
- `Init_CalledConcurrentlyFromTwoStaThreads_InvokesTheFactoryExactlyOnce` — the existing `Interlocked.Exchange` latch already admits exactly one caller. The Phase 3 lock preserves that and additionally closes the C04 race the latch never covered.
- `Init_WithMonitorUiThreadEnabled_ConstructsAndRunsTheThreadMonitorWithTheInjectedTimeProvider` — the monitor branch already runs when `Initialize()` succeeds; the Phase 1 factory seam is what made it reachable from a test, and no Phase 3 change is required for it.
- `UiSyncContext_ReadWithNullBackingFieldFromStaThread_InitializesThroughTheLazyPath` — the lazy branch already works when the latch is unconsumed, which the reset scope guarantees; it was simply never exercised with a null field before.
- The five pre-existing `SynchronizationContextAwaiter_Tests` methods — unchanged behaviour, asserted before this delivery and still asserted.
- `IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue` — the reference fast path is the current implementation.
- `IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse` — a non-null context is not reference-equal to a null ambient today either.
- `IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse` — reference inequality already returns false; the Phase 3 predicate returns false through the sentinel guard instead.
- `IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse` — reference inequality already returns false; after the fix the deciding clause becomes the dispatcher comparison.
- `IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` — reference inequality already returns false; after the fix the deciding clause becomes the type test, which is the regression guard for the `WinFormsPumpHostTests` failure mode.
- `IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue` — `null == null` is true today and `ReferenceEquals(null, null)` is true after the fix, so the default-instance behaviour is unchanged.

Every assertion in the six failing tests is synchronous, so no async boundary can swallow the failure.
