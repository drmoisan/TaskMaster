# Ambient SynchronizationContext on the MSTest worker thread (P1-T5, AC16)

Timestamp: 2026-10-02T00-59
Task: P1-T5 [expect-fail]
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-PROBE (FullyQualifiedName=QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.Issue950_AmbientSynchronizationContextProbe), TASKID p1-t5 and NAMES "Issue950_AmbientSynchronizationContextProbe", executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-PROBE" "/ResultsDirectory:coverage\test-results\950\p1-t5" "/Logger:trx;LogFileName=p1-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Setup: Delivered Source P-PROBE was inserted temporarily into the unmodified QfcDatamodelLivenessTests.cs (P1-T1), the tree was built (P1-T4), and the probe ran alone under the repository runsettings (Workers=0, Scope=ClassLevel). The probe always fails by construction (D-4), so its failure message carries the observed values. The probe is reverted in P1-T7 and is never committed.

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=1 executed=1 passed=0 failed=1
RESULT_COUNT: 1
RESULT Issue950_AmbientSynchronizationContextProbe = Failed duration=00:00:00.1313302
MESSAGE Issue950_AmbientSynchronizationContextProbe :: Expected report to be a match with the expectation because AMBIENT-SYNC-CONTEXT=null THREAD-POOL=True APARTMENT=MTA, but it differs at index 0:  (actual) "AMBIENT-SYNC-CONTEXT=null THREAD-POOL=True APARTMENT=MTA" "PROBE-ALWAYS-FAILS"  (expected)

AMBIENT-SYNCHRONIZATION-CONTEXT: null
THREAD-POOL: True
APARTMENT: MTA

The recorded value is an observation (spec Assumptions), not a requirement: liveness tests 3 and 4 install and restore their own DrainableSynchronizationContext, so the design does not depend on it.
