# P3-T7 — Cross-Class `UiThreadDispatcherGate` Serialization

Timestamp: 2026-08-22T10-37

Command:
```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx `
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p3-t7 `
  /TestCaseFilter:"FullyQualifiedName~QfcItemController_SeamFactoryTests|FullyQualifiedName~QfcItemController_InitializationTests"
```

Both classes run in the **same** invocation, so class-level parallelization exercises the shared
process-wide `UiThreadDispatcherGate`.

EXIT_CODE: 0

Output Summary:

TRX: `evidence/regression-testing/p3-t7/DanMoisan_MEGALODON4_2026-08-22_10_37_42_net481.trx`

TRX `<Counters>` verbatim:

```
total="20" executed="20" passed="20" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
```

| Class | Tests in run | Passed | Failed |
| --- | --- | --- | --- |
| `QfcItemController_InitializationTests` | 11 | **11** | 0 |
| `QfcItemController_SeamFactoryTests` | 9 | **9** | 0 |
| Total | 20 | **20** | 0 |

Total wall-clock duration: **4.7090 seconds**.

Acceptance:

- Recorded failed count is **0**.
- Both class names appear in the TRX with at least one passed test each (11 and 9 respectively).
- No test is recorded as failing on its `[Timeout]`: the TRX carries `timeout="0"` and zero
  `outcome="Timeout"` results. The four longest tests in the run are
  `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` (1 s),
  `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` (411 ms),
  `PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference` (306 ms) and
  `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` (202 ms), all far below
  the 60,000 ms `PumpTimeoutMs`.

The gate cascade the task warns about did not occur. The acquire-and-release structure verified
statically in P2-T2 is confirmed here at runtime: `QfcItemController_SeamFactoryTests` acquires the
same gate through the `internal static BuildPumpHarnessAsync` wrapper, and every fixture released it
via `PumpHarness.Restore`, so no class starved the other.

Both new regression tests ran inside this two-class invocation and passed:
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` (101 ms) and
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` (90 ms).
