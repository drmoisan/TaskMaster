# Negative-control batch B run (P5-T16)

Timestamp: 2026-10-02T01-13
Task: P5-T16 [expect-fail]
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-CONTROLS-B (FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces), TASKID p5-t16 and NAMES "RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces", executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONTROLS-B" "/ResultsDirectory:coverage\test-results\950\p5-t16" "/Logger:trx;LogFileName=p5-t16.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Edit under test: B1 (no-op starter in StartHeldOpenLoader).

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=1 executed=1 passed=0 failed=1
RESULT_COUNT: 1
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Failed duration=00:00:00.2592311
MESSAGE RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces :: Expected entered.Task.IsCompleted to be True because the synchronous starter must reach the injected loader before returning, but found False.

Verdict: Failed at once with the message "the synchronous starter must reach the injected loader before returning". No CONTROL DEFECT.
