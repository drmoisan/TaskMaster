# Negative-control batch C run (P5-T22)

Timestamp: 2026-10-02T01-15
Task: P5-T22 [expect-fail]
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-CONTROLS-C (tests 3 and 4 by FullyQualifiedName=, QCT. expanded), TASKID p5-t22 and NAMES "RemainingLoadActive_AfterLoaderCompletes_BecomesFalse", "RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally", executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONTROLS-C" "/ResultsDirectory:coverage\test-results\950\p5-t22" "/Logger:trx;LogFileName=p5-t22.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Edits under test: C1 and C2 (`pump.Drain();` deleted from tests 3 and 4; the loader is still released).

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=2 executed=2 passed=0 failed=2
RESULT_COUNT: 2
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Failed duration=00:00:00.0021400
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Failed duration=00:00:00.1620202
MESSAGE RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally :: Expected ReadLivenessFlag(model) to be False because the finally must clear the flag on the throwing path too, or the gate would poll forever, but found True.
MESSAGE RemainingLoadActive_AfterLoaderCompletes_BecomesFalse :: Expected ReadLivenessFlag(model) to be False because the finally around the awaited loader must clear the flag once it completes, but found True.

Verdict: with the loader released but Drain() removed, both tests fail at once, so the flag clears only through Drain(). The loader's continuation is posted to the installed DrainableSynchronizationContext rather than run inline inside SetResult (D-2; RunContinuationsAsynchronously on the release source). Neither test passed (no DRAIN NOT LOAD-BEARING); no Timeout, Aborted or NotExecuted outcome and no Sequence file (no CONTROL DEFECT).
