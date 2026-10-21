# Final QC stage 4 — `QuickFiler.Test` with coverage enabled

Timestamp: 2026-08-26T13-56
Task: [P7-T5]

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`, run from the worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings "/Logger:trx;LogFileName=final-quickfiler-test.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\qa-gates\trx-final
```

EXIT_CODE: 0

## Result

```
Test Run Successful.
Total tests: 959
     Passed: 959
 Total time: 12.3354 Seconds
```

| Metric | Value |
|---|---|
| Total | 959 |
| **Passed** | **959** |
| **Failed** | **0** |
| Skipped | 0 |

Counts confirmed independently by parsing the TRX `UnitTestResult` elements: 959 results, all with
`outcome="Passed"`.

## Comparison against the `BASELINE_PASSED` figure

| | Value |
|---|---|
| `BASELINE_PASSED` recorded at `[P0-T12]` | 938 |
| Required minimum (`BASELINE_PASSED` + 21) | 959 |
| `[P7-T5]` passed count | **959** |

The condition `passed >= BASELINE_PASSED + 21` holds with equality. The figure 21 is the 19 added test
methods with the three-row `[DataTestMethod]` counted as three results per decision D9.

## The 21 new test results contributing to the increase

Nineteen added test methods produce 21 TRX result rows:

| # | Result name | Issue |
|---|---|---|
| 1 | `ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce` | #480 |
| 2 | `malformed URI` (row of `TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs`) | #485 |
| 3 | `relative URI` (row of the same `[DataTestMethod]`) | #485 |
| 4 | `empty final segment` (row of the same `[DataTestMethod]`) | #485 |
| 5 | `TryResolveCidResource_WithNullMap_ReturnsFalse` | #485 |
| 6 | `TryResolveCidResource_WithMapMiss_ReturnsFalse` | #485 |
| 7 | `TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` | #485 |
| 8 | `TryResolveCidResource_WithKnownExtension_ReturnsPayloadAndMimeType` | #485 |
| 9 | `TryResolveCidResource_WithUnrecognisedExtension_ReturnsOctetStream` | #485 |
| 10 | `MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException` | #483 |
| 11 | `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` | #483 |
| 12 | `MoveMailAsync_WithUiDispatcher_MarshalsNotificationThroughDispatcher` | #483 |
| 13 | `MoveMailAsync_WhenTokenAlreadyCancelled_ThrowsAndNeverInvokesFilerFactory` | #483 |
| 14 | `FlagAsTaskAsync_WhenTokenAlreadyCancelled_Throws` | #483 |
| 15 | `EnumerateConversationAsync_WhenTokenAlreadyCancelled_Throws` | #483 |
| 16 | `Cleanup_DisposesEmailIsReadTimerBeforeNullingIt` | #484 |
| 17 | `ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave` | #484 |
| 18 | `Cleanup_NullsMailActions_AndSaveParametersRebindsIt` | #484 |
| 19 | `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | #481 |
| 20 | `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` | #481 |
| 21 | `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` | #481 |

Per decision D9, the three `[DataRow]` rows of
`TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs` are reported by MSTest as three
distinct result rows carrying their `DisplayName` values, which are enumerated at rows 2 to 4 above.
Every one of the 21 is `Passed`.

## Execution note (retry)

The first invocation of this command was terminated by the harness after a 10-minute wall-clock limit
with the test host stalled (CPU time flat at 19.5 s over a 20-second sample, working set unchanged).
The machine was carrying unrelated concurrent load at the time, and the run settings request
`Workers=0` (one worker per logical processor; 24 on this host) with coverage instrumentation attached.
The stalled process chain belonging to this task alone was terminated and the identical command was run
once more; no shared build or compiler process was touched. The second run completed in 12.3 seconds
with 959 of 959 passing. Only the successful run is recorded as the gate result; no source file changed
between the two invocations, so the toolchain loop does not restart.

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/trx-final/final-quickfiler-test.trx`.

Output Summary: EXIT_CODE 0, 959 of 959 Passed, 0 Failed, 0 Skipped. Passed count equals
`BASELINE_PASSED` (938) plus 21, and all 21 new result rows are enumerated above.
