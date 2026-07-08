# Baseline Full First-Party Suite with Coverage (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage /Logger:trx;LogFileName=baseline-full.trx`
(Seven first-party test assemblies. Vendored test projects — UtilitiesSwordfish.Test, SVGControl.Test — excluded per G4. Invoked via VS18 vstest.console.exe with MSYS_NO_PATHCONV=1.)

EXIT_CODE: 1

Output Summary:
- Total tests: 4064. Passed: 4053. Failed: 11. (No skipped.)
- The 11 failures decompose EXACTLY into the in-scope and out-of-scope expected sets:
  - 3 in-scope target tests (Finding A/B), failing deterministically:
    - `FromSeed_ShouldBuildFileNameFromParts`
    - `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths`
    - `People_Deserialize_CanDeserializePatternCorrectly`
  - 8 pre-existing flaky wall-clock-timer tests (G7, OUT OF SCOPE, must not be modified), comprising 8 failing result rows across 7 distinct names (`Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` appears in two assemblies):
    - `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
    - `AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress`
    - `EmptyQueue_AfterSeveralIntervals_StopsTimer`
    - `Enqueue_InvokesBatchActionsOnTimerInterval`
    - `RequestTask_WithConfiguredTask_InvokesTaskAfterInterval`
    - `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` (x2 assemblies)
    - `TimeoutAfter_GenericTask_ShouldThrowTimeoutException_WhenTaskExceedsTimeout`
- The 4th target test, `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` (Finding C, also a wall-clock-timer flaky), PASSED in this full-suite run (it passes under this load); its fail-before evidence is the structural dossier `evidence/regression-testing/fail-before-exception.2026-06-08T21-53.md`.

## Coverage Headline (numeric)

- Coverage artifact: `.coverage` at `trx-full/f6b0d8f7-3887-4017-80dc-9358746bfc82/DanMoisan_MEGALODON4_2026-06-08.22_10_28.coverage`, converted to cobertura via `dotnet-coverage merge ... --output-format cobertura` -> `baseline-coverage.cobertura.xml`.
- Raw merged cobertura aggregate: `line-rate=0.5904483566441676` (59.04%), `lines-covered=101824`, `lines-valid=172452`, `branch-rate=1`.
- NOTE: This raw aggregate denominator (172,452 valid lines) includes test assemblies and instrumented vendored/third-party code, which deflates the headline below the first-party application-code figure that the >=80% policy targets. The aggregate is recorded here as the literal repo-wide measured value for the no-regression delta comparison in P5-T5; the post-change run (P5-T4) is measured identically so the comparison is apples-to-apples.
- No-regression baseline pass set: 4053 passing tests (the post-change suite must not drop below this once the three in-scope target tests are converted from FAIL to PASS, i.e., expected post-change >= 4056 passing, holding the 8 out-of-scope flaky-timer tests as the only permitted non-passes).
