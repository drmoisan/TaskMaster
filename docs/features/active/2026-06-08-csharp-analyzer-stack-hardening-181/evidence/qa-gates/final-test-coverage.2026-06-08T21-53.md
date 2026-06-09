# Final QA — Test with Coverage (P5-T4) (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /InIsolation /EnableCodeCoverage /Logger:trx;LogFileName=final-full2.trx`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1; `/InIsolation` to apply the test assemblies' Moq/STTE binding redirects. Seven first-party assemblies; vendored test projects excluded per G4.)

EXIT_CODE: 1 (the only failures are the pre-existing out-of-scope flaky wall-clock-timer/dispatcher tests — see below)

Output Summary:
- Total tests: 4064. Passed: 4055. Failed: 9. (No skipped.)
- All FOUR in-scope target tests PASS in this full-suite run: `FromSeed_ShouldBuildFileNameFromParts`, `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths`, `People_Deserialize_CanDeserializePatternCorrectly`, `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` (none appear in the failure list).
- The 3 `ScoDictionaryConverterTests` integration tests (`TypedConverter_IntegrationTest_SerializeAndDeserialize`, `UntypedConverter_IntegrationTest_SerializeAndDeserialize`, `UntypedConverter_IntegrationTest_SerializeAndDeserialize_InternalJsonProperty`) PASS — the transient Finding-B regression they exhibited was resolved by the `NormalizeEmptyDiskFilePaths` in-budget fix in `WrapperScoDictionary.cs`.
- The 9 failures are ALL pre-existing flaky wall-clock-timer / UI-dispatcher tests (G7 family, out of scope, must not be modified). They pass in isolation and fail intermittently under full-suite CPU contention; the exact membership varies run-to-run:
  - `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
  - `AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress`
  - `EmptyQueue_AfterSeveralIntervals_StopsTimer`
  - `Enqueue_InvokesBatchActionsOnTimerInterval`
  - `RequestTask_WithConfiguredTask_InvokesTaskAfterInterval`
  - `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` (x2 assemblies)
  - `ToList_InternalHelper_ConsumesEnumerableAndReportsProgress`
  - `WireNotifications_OnMappedToChange_RaisesPropertyChanged`
- The two failures in this run that were NOT in the cycle-5 baseline failing set (`ToList_InternalHelper_ConsumesEnumerableAndReportsProgress`, `WireNotifications_OnMappedToChange_RaisesPropertyChanged`) were re-run in isolation and BOTH PASS, confirming they are flaky-under-load members of the same pre-existing family, not regressions from the cycle-5 edits.

## Coverage Headline (numeric)

- Coverage artifact: `trx-final2/5143f515-.../...22_38_26.coverage`, converted via `dotnet-coverage merge ... --output-format cobertura` -> `final-coverage.cobertura.xml`.
- Post-change raw merged cobertura aggregate: `line-rate=0.5906484621851176` (59.06%), `lines-covered=101878`, `lines-valid=172485`, `branch-rate=1`.
- Baseline (P0-T7) aggregate: 59.04% (101824/172452). Post-change is 59.06% (101878/172485) — coverage did NOT regress (+0.02pp; +54 covered lines). The aggregate denominator includes test assemblies and instrumented vendored code (measured identically baseline vs post-change for an apples-to-apples comparison); see P5-T5 for the delta/threshold analysis.

## No-regression conclusion

- Baseline passing set: 4053. Post-change passing set: 4055 (the three previously-failing in-scope target tests now pass; the flaky-timer family's run-to-run variance accounts for the per-run count). No previously-passing non-flaky test regressed. The `PeopleScoConverter`/shortcut path still passes.
