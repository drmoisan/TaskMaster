# Liveness sensitivity check (issue #968, task P5-T8, expect-fail)

## Labelled sensitivity check (not a fail-before of the old tests)

Timestamp: 2026-10-03T03-13
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-LIVENESS-PAIR (`FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle|FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelTests.DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive`), TASKID p5-t8 and NAMES-LIVENESS (step 4; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted)
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-LIVENESS-PAIR" "/ResultsDirectory:coverage\test-results\968\p5-t8" "/Logger:trx;LogFileName=p5-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 in every payload of steps 2, 3, 4 and 6
- Step (1): Edit tool replaced the line `                () => _remainingLoadActive,` in QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs with `                () => false,` (working copy only)
- Step (2): CMD-SPAN-TOKEN-COUNT on GATE-LAMBDA (exit 0): SPAN: 299-310; `() => _remainingLoadActive,` 0; `() => false,` 1 (edit applied)
- Step (3): CMD-BUILD with TASKID p5-t8 (exit 0): MSBUILD_EXIT_CODE: 0; ERRORS: 0; TEST_DLL_ADVANCED: True; PROD_DLL_ADVANCED: True; CSC_OUT_QUICKFILER: 2; CSC_OUT_QUICKFILER_TEST: 2
- Step (4): VSTEST_EXIT_CODE: 1; TRX_PRESENT: True; SEQUENCE_FILES: 0; COUNTERS total=2 executed=2 passed=0 failed=2; RESULT_COUNT: 2
  - RESULT DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Failed duration=00:00:00.2568927
  - RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Failed duration=00:00:00.2589193
  - MESSAGE DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive :: Expected first to refer to System.Threading.Tasks.Task`1[System.Boolean] {Status=WaitingForActivation} because the datamodel source-active signal must keep polling while the worker can still add candidates, but found System.Threading.Tasks.Task`1[System.Collections.Generic.IList`1[Microsoft.Office.Interop.Outlook.MailItem]] {Status=RanToCompletion}.
  - MESSAGE DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle :: Expected first to refer to System.Threading.Tasks.Task`1[System.Boolean] {Status=WaitingForActivation} because the loader is still producing, so the gate must arm a second wait rather than treat an empty queue as an exhausted source and return an early partial batch, but found System.Threading.Tasks.Task`1[System.Collections.Generic.IList`1[Microsoft.Office.Interop.Outlook.MailItem]] {Status=RanToCompletion}.
  - Both outcomes are `Failed` (not Timeout, not Aborted). The Liveness MESSAGE contains `to refer to` and `the gate must arm a second wait`; the sibling MESSAGE contains `to refer to` and `must keep polling while the worker can still add candidates`. Each test failed on its re-arm assertion, so the new shape is sensitive to a dishonest liveness signal and fails crisply rather than hanging.
- Step (5): Edit tool restored the line to `                () => _remainingLoadActive,`
- Step (6): CMD-SPAN-TOKEN-COUNT on GATE-LAMBDA (exit 0): SPAN: 299-310; `() => _remainingLoadActive,` 1; `() => false,` 0 (edit reverted)
- Step (7): git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs -> exit 0, no output (the file is byte-identical to BASE)
- Step (8): git -C WORKTREE status --porcelain -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs -> exit 0, prints nothing for that path
- SENSITIVITY-EDIT-REVERTED: YES

The temporary edit was never staged or committed: it existed only in the working copy between steps (1) and (5), and steps (7) and (8) prove the file is at BASE content with no working-copy change.
