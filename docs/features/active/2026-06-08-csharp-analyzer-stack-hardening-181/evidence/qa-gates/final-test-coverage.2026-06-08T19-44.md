# Final QC Step 4 — MSTest Suite with Coverage (Cycle 3)

Timestamp: 2026-06-08T19-44

Command: MSYS_NO_PATHCONV=1 vstest.console.exe "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "Tags.Test\bin\Debug\Tags.Test.dll" "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" "TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll" "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "VBFunctions.Test\bin\Debug\VBFunctions.Test.dll" /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:"...\evidence\qa-gates\trx"

EXIT_CODE: 1

TRX: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/trx/DanMoisan_MEGALODON4_2026-06-08_19_57_49_net481.trx
Coverage: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/trx/56e1c77d-cad9-458d-950b-8c783b33d1d7/DanMoisan_MEGALODON4_2026-06-08.19_58_46.coverage

## Output Summary

- Total tests: 4064. Passed: 4054. Failed: 10. Total time: 1.02 minutes.
- Seven first-party test assemblies executed (vendored SVGControl.Test and
  UtilitiesSwordfish.Test excluded).

## Failed Tests (10)

| Test | First-line failure | Category |
|---|---|---|
| People_Deserialize_CanDeserializePatternCorrectly | Assert.AreEqual failed. Expected string length 11 but was 0. expected '"pplkey.json"', actual 'people.Config.Disk.FileName' | **Re-enabled regression test — NEW FINDING (blocking)** |
| FromSeed_ShouldBuildFileNameFromParts | Expected fph.FolderPath to be "C:\data" len 7, but "C:\" len 3 | path/env-dependent (not a timer) |
| CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths | Expected result 239 but found 245 (diff 6) | path/length-dependent (not a timer) |
| EmptyQueue_AfterSeveralIntervals_StopsTimer | SpinWait.SpinUntil(!TimerActive, 5000) False | wall-clock timer (candidate flaky) |
| Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite (x2) | signal.Wait(1000) False | wall-clock timer (candidate flaky) |
| RequestTask_WithConfiguredTask_InvokesTaskAfterInterval | Task.WhenAny(...Task.Delay(1000)) timed out | wall-clock timer (candidate flaky) |
| Enqueue_InvokesBatchActionsOnTimerInterval | signal.Wait(1000) False | wall-clock timer (candidate flaky) |
| ConcurrentEnqueue_BatchesAllItems | signal.Wait(1000) False | wall-clock timer (candidate flaky) |
| AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress | Expected reports {...} (timer-interval progress) | wall-clock timer (candidate flaky) |

## Coverage (raw merged, for evidence completeness)

- Raw aggregate across ALL modules in the .coverage file (including third-party
  FSharp.Core, log4net, Deedle, System.Linq.Async, FluentAssertions, Mono.Reflection,
  and vendored SVGControl/Swordfish): lines_covered=105848 / 182312 = 58.06%. This raw
  number is NOT the policy metric; the denominator is dominated by third-party/vendored
  assemblies that are out of coverage scope.
- First-party application-code line coverage (representative modules):
  - UtilitiesCS.dll: 85.47% (34971 / 40030)
  - TaskMaster.dll, QuickFiler.dll, Tags.dll, ToDoModel.dll, TaskVisualization.dll vary;
    these are dominated by VSTO/UI glue and are below the suite's covered subset.
- A precise first-party-only repository-wide coverage headline and the changed-line
  coverage delta (P2-T7) were NOT computed because the cycle is HALTED at P2-T6 by the
  failing re-enabled regression test; coverage reconciliation is deferred to the
  follow-up remediation cycle.

## Toolchain-Loop Status

The toolchain pass did NOT complete clean: the test step (this step) failed with a
re-enabled regression test failing. Per the Scope-Change Escalation Rule, this is a new
finding, not a loop-restart condition that the executor may resolve by editing tests.
Execution is HALTED; no commit/push is performed.
