# Liveness pass-after (issue #968, task P5-T7)

Timestamp: 2026-10-03T03-12
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle|FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelTests.DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` (FILTER-LIVENESS-PAIR), TASKID p5-t7 and NAMES-LIVENESS; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-LIVENESS-PAIR" "/ResultsDirectory:coverage\test-results\968\p5-t7" "/Logger:trx;LogFileName=p5-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=2 executed=2 passed=2 failed=0
- RESULT_COUNT: 2
- RESULT DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed duration=00:00:00.1623663
- RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1602447
- This is the measured pass-after run of the two rewritten tests.
