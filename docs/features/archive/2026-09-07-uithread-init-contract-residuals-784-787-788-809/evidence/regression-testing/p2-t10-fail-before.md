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

---

# Correction and re-measurement (recorded during [P3-T6])

CORRECTION_TIMESTAMP: 2026-09-08T02-05

The first pass recorded above is superseded in part. Reading the verbatim failure messages out of `p2t10.trx` during [P3-T6] showed that **two of the six rows failed for a reason other than the defect they were written to expose**, so the first pass is not admissible fail-before evidence for those two. The remaining four rows are unaffected and stand as recorded.

## Verbatim failure messages from the first pass (`p2t10.trx`)

| Fully-qualified test | Message |
|---|---|
| `Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` | `Expected Thread.CurrentThread.GetApartmentState() to be ApartmentState.MTA {value: 1}, but found ApartmentState.STA {value: 0}.` |
| `Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` | `Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.` |
| `Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` | `Expected mtaOutcome to be System.InvalidOperationException, but found <null>.` |
| `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | `Expected working not to be <null>.` |
| `AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` | `Expected InvalidOperationException.Message to be System.InvalidOperationException, but found <null>.` |
| `IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` | `Expected result to be True, but found False.` |

## The measured environmental fact

The first message states it directly: the MSTest worker executing the plain `[TestMethod]` cases of `UiThreadInitApartmentContract_Tests` reported `ApartmentState.STA`, not `ApartmentState.MTA`.

Research R4 concluded that a plain `[TestMethod]` runs MTA, and `UtilitiesCS.Test/test.runsettings` does record that global STA execution is intentionally disabled. That premise does not hold for this scheduling arrangement: `UiThreadInitRetryContract_Tests` is `[STATestClass]` and both classes are `[DoNotParallelize]`, so they share one serial execution thread, and the thread that serial bucket runs on was created STA. The ambient apartment of a plain `[TestMethod]` is therefore not a reliable source of an MTA caller in this file.

The consequence for the two affected rows:

- `Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` failed on its own Arrange premise and never reached the Act, so it measured nothing about `Init()`.
- `Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` did reach the Act, but on an STA thread, where the AC1 precondition correctly does **not** throw. It would therefore have stayed red after the fix, for a reason unrelated to the defect.

The four other rows were unaffected because each already drove its Act on a dedicated thread with an explicitly set apartment.

## Remedy

Both methods were re-authored to run the Act on a dedicated MTA thread through `ApartmentThreadRunner.RunOnThread(ApartmentState.MTA, ...)`, which is the mechanism the four unaffected rows already use and which the first pass measured as producing a genuine MTA caller. Neither method was renamed, no method was added or removed, and no assertion was weakened: each now asserts the same contract against a caller whose apartment is known rather than assumed.

## Re-measured fail-before

The re-measurement restored `UtilitiesCS/Threading/UiThread.cs` and `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` to their committed Phase 2 state at `f7294d71`, rebuilt the solution with `/t:Rebuild`, and ran the identical filter against the re-authored tests.

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p2t10b.trx' '/ResultsDirectory:TestResults\809-p2t10b' '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitApartmentContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests'
```

REMEASURED_EXIT_CODE: 1
REMEASURED_EXPECTED_EXIT_CODE: 1

TRX selected: `p2t10b.trx`, `LastWriteTimeUtc` `2026-09-08T04:50:49.5639685Z`. Counters: `total` 22, `executed` 22, `passed` 16, `failed` 6. Derived skipped count: 0.

The set of `Failed` rows is **the same six**, no more and no fewer, and every one now fails on the defect:

| Fully-qualified test | Re-measured message |
|---|---|
| `Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState` | `Expected observed to be System.InvalidOperationException, but found <null>.` |
| `Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` | `Expected observed to be System.InvalidOperationException, but found <null>.` |
| `Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta` | `Expected mtaOutcome to be System.InvalidOperationException, but found <null>.` |
| `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | `Expected working not to be <null>.` |
| `AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` | `Expected InvalidOperationException.Message to be System.InvalidOperationException, but found <null>.` |
| `IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` | `Expected result to be True, but found False.` |

`found <null>` on the first three is the AC1 defect: pre-fix, `Init()` from a genuine MTA thread returns normally instead of throwing. The remaining sixteen rows were `Passed`, unchanged from the first pass.

`UtilitiesCS/Threading/UiThread.cs` and `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` were restored to their Phase 3 state immediately afterwards, the solution was rebuilt, and [P3-T6] was re-run; that run is recorded in `p3-t6-pass-after.md`.

**This re-measurement is the fail-before evidence of record for all six tests.** The first pass is retained above rather than deleted, because it is what surfaced the environmental fact.
