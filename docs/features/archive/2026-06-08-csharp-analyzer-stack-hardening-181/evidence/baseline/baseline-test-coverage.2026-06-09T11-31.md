# Baseline — Full First-Party Suite with Coverage

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe QuickFiler.Test.dll Tags.Test.dll TaskMaster.Test.dll TaskVisualization.Test.dll ToDoModel.Test.dll UtilitiesCS.Test.dll VBFunctions.Test.dll /EnableCodeCoverage /InIsolation
EXIT_CODE: 1

Output Summary:
- Total tests: 4065; Passed: 4058; Failed: 7 (run 1). A second confirmation run produced 8
  failures (the count varies run-to-run), confirming the non-determinism this cycle remediates.
- Coverage source: TestResults/.../DanMoisan_MEGALODON4_2026-06-09.11_46_14.coverage
  -> merged to evidence/baseline/baseline-coverage.2026-06-09T11-31.xml

Numeric coverage (line coverage = lines_covered / (covered + partial + not_covered)):
- Primary in-scope first-party production assembly:
  - UtilitiesCS.dll: lines_covered=35034, partial=886, not_covered=5047 -> 85.52% line coverage
    (all 24 cataloged timer occurrences and all 8 production seam targets live in UtilitiesCS.dll)
- Other first-party production assemblies (context): Tags.dll 29.90%, ToDoModel.dll 10.86%,
  TaskMaster.dll 25.17%, TaskVisualization.dll 0.36%, QuickFiler.dll 23.55%, VBFunctions.dll 100.00%.
- Test assemblies (informational): UtilitiesCS.Test.dll 95.81%, TaskMaster.Test.dll 93.08%,
  QuickFiler.Test.dll 90.74%, Tags.Test.dll 100.00%, VBFunctions.Test.dll 87.50%,
  ToDoModel.Test.dll 62.04%, TaskVisualization.Test.dll 1.35%.

Baseline failing tests (run 1 = 7; a confirmation run showed these 8 names, the extra one being
a second flaky occurrence). ALL are wall-clock/timer-dependent flaky tests; the timer-conversion
inventory rows are mapped:
1. Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite (SmartSerializableBase_Tests) — rows A1/A2
2. Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite (SmartSerializable_Tests) — row A3
3. Enqueue_InvokesBatchActionsOnTimerInterval — row C1
4. EmptyQueue_AfterSeveralIntervals_StopsTimer — row C2
5. ConcurrentEnqueue_BatchesAllItems — row C3
6. AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress — row E1
7. RequestTask_WithConfiguredTask_InvokesTaskAfterInterval — NOT in the 24-row inventory (see note)
8. AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException — NOT in the 24-row inventory (see note)

Note on items 7-8: `RequestTask_WithConfiguredTask_InvokesTaskAfterInterval` and
`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` are flaky at baseline
but are NOT cataloged in the research inventory (24 rows, groups A-L). They are pre-existing flaky
tests outside this cycle's conversion scope. They are recorded here as baseline-flaky so the final
QA gate can distinguish a genuine regression (a test that passes at baseline but fails after) from
pre-existing flakiness. The Phase 7 final gate requires zero failures; if these two remain flaky
after the in-scope conversions and block the gate, that is a scope-change finding to escalate, not
a masking action.

Coverage headline for no-regression comparison: UtilitiesCS.dll = 85.52% line coverage at baseline
(repo first-party production assembly carrying all changed lines this cycle).
