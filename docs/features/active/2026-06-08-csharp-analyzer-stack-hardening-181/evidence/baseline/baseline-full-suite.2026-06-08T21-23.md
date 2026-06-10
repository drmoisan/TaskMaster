# Baseline Full First-Party Suite with Coverage (Cycle 4, Issue #181)

Timestamp: 2026-06-08T21-23

Command: `vstest.console.exe "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "Tags.Test\bin\Debug\Tags.Test.dll" "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" "TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll" "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "VBFunctions.Test\bin\Debug\VBFunctions.Test.dll" /EnableCodeCoverage`
(First-party set = the 7 non-vendored Test projects. Vendored SVGControl.Test and UtilitiesSwordfish.NET.Test are excluded per the analyzer-stack first-party scope.)

EXIT_CODE: 1

## Output Summary

- Total tests: 4064
- Passed: 4052
- Failed: 12
- Test Run Failed (due to the target + flaky tests below)

### Failed-set breakdown (12 failures)

Target tests (in this cycle's fix scope, expected fail-before):
1. `People_Deserialize_CanDeserializePatternCorrectly` — FAILED (Config.Disk.FileName == "")
2. `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` — FAILED under full-suite parallel load (flaky timer; PASSED in isolation)
3. `FromSeed_ShouldBuildFileNameFromParts` — FAILED (FolderPath corrupted to "C:\")
4. `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` — FAILED (245 vs expected 239)

Non-target flaky tests (PRE-EXISTING, OUTSIDE this cycle's scope; NOT among the 4 authorized tests and NOT touched by the 2 authorized production files). All 8 PASS deterministically when run in isolation (verified — see note below), and only fail under full-suite parallel timer contention:
5. `Enqueue_InvokesBatchActionsOnTimerInterval` (UtilitiesCS.Test/ReusableTypeClasses/TimedQueueOfActions_Tests)
6. `EmptyQueue_AfterSeveralIntervals_StopsTimer` (same class)
7. `AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress` (UtilitiesCS.Test/Threading/AsyncMultiTasker_Tests)
8. `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` (UtilitiesCS.Test/ReusableTypeClasses/SmartSerializable_Tests) — reported twice in run output
9. `ConcurrentEnqueue_BatchesAllItems` (TimedQueueOfActions_Tests)
10. `RequestTask_WithConfiguredTask_InvokesTaskAfterInterval`
11. `WireNotifications_OnMappedToChange_RaisesPropertyChanged`

Isolation verification: a targeted re-run of the 7 distinct non-target flaky tests above (excluding the duplicate Serialize entry) returned `Test Run Successful. Total tests: 8; Passed: 8`. They are wall-clock-timer-dependent flaky tests, not deterministic baseline passes. They are therefore not part of the "currently-passing deterministic" no-regression baseline set, and the cycle-4 fix does not address them (out of scope).

### No-regression baseline definition

- Currently-passing deterministic test count at baseline: 4052 passing in this parallel run; the 8 non-target failures are flaky (pass in isolation), and the 4 target failures are the in-scope fix set.
- The cycle exit requires: the 4 target tests pass deterministically AND no currently-passing deterministic test regresses. The 8 pre-existing flaky tests are not a no-regression obligation of this cycle and may continue to exhibit load-dependent flakiness independent of these changes.

## Coverage Headline (baseline)

Source `.coverage` converted to Cobertura via `dotnet-coverage merge ... -f cobertura`.

- Raw merged repository-wide line-rate (ALL modules incl. third-party/vendored): 0.5903 = 59.03% (lines-covered 101806 / lines-valid 172452). This raw figure is consistent with the cycle-3 recorded ~58% and is depressed by out-of-scope third-party/vendored assemblies (Mono.Reflection, Swordfish.NET.General, etc.).
- First-party application module under change — UtilitiesCS: line-rate 0.8743 = 87.43% (both changed files live in UtilitiesCS).
- Other first-party application packages (informational; many are low because the suite focuses unit coverage on specific modules): VBFunctions 100%, Tags 31.40%, TaskMaster 25.78%, QuickFiler 25.20%, ToDoModel 10.82%, TaskVisualization 0.37%.

The UtilitiesCS first-party figure (87.43%) is the relevant no-regression and >=80% reference for the changed lines, since both changed files are in UtilitiesCS. The raw merged 59.03% is recorded as the repository-wide aggregate headline for cross-cycle continuity.
