# Parallel-Suite Run (P4-T5): QuickFiler.Test, full assembly, after the fix

Timestamp: 2026-09-29T09-36
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/ResultsDirectory:coverage\test-results\931\p4-t5" "/Logger:trx;LogFileName=p4-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST with ASSEMBLY-QF, NAMES-QF, TASKID p4-t5). Runsettings file: TaskMaster.runsettings (repository root; Workers 0, Scope ClassLevel). Isolation switch: /InIsolation. Filter: no test-case filter. Console transcript teed to the git-ignored coverage\logs\p4-t5.vstest.log.
EXIT_CODE: 0
ITERATION: 1

Output Summary:
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA (equals RUNSETTINGS-HASH from P0-T4)
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- COUNTERS total=1468 executed=1468 passed=1468 failed=0
- RESULT_COUNT: 1468
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
- RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed
- RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
- RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
- RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
- RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
- RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
- RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Passed
- MESSAGE lines: none
- NEWLY-FAILING: NONE (BASELINE-FAILED-QF: NONE; this run's failed set is empty)
- executed 1468 is at least the P0-T10 QuickFiler executed value 1468.

Acceptance: EXIT_CODE 0; failed 0 and executed at least the baseline value; SEQUENCE_FILES 0; the eight NAMES-QF RESULT lines present and each Passed (the Part2 file and the helper compiled and were discovered, AC6); RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; NEWLY-FAILING NONE. All six hold.

The TRX document stays under the git-ignored coverage\test-results\931\ tree; this file is the committed projection.
