# P2-T9 Confirming Runs Before Controls

Timestamp: 2026-09-29T09-20
ITERATION: 1
Command: (1) vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow|FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction" "/ResultsDirectory:coverage\test-results\931\p2-t9-four" "/Logger:trx;LogFileName=p2-t9-four.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; (2) vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~OpenRead_ShouldReturnReadableStreamForWrappedFile" "/ResultsDirectory:coverage\test-results\931\p2-t9-openread" "/Logger:trx;LogFileName=p2-t9-openread.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; (3) vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/ResultsDirectory:coverage\test-results\931\p2-t9-affinity" "/Logger:trx;LogFileName=p2-t9-affinity.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; (4) vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~FileInfoWrapper_Tests" "/ResultsDirectory:coverage\test-results\931\p2-t9-fiw" "/Logger:trx;LogFileName=p2-t9-fiw.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" - each resolved through vswhere (CMD-VSTEST)
EXIT_CODE: 0

Output Summary:
- EXIT_CODE is scoped to run 1 (p2-t9-four).
- OPENREAD-VSTEST-EXIT: 0
- AFFINITY-VSTEST-EXIT: 0
- FIW-VSTEST-EXIT: 0
- RUNSETTINGS-HASH (P0-T4): 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- Observation: each run printed a coverage attachment under its git-ignored results directory (the root runsettings collector), while COLLECTOR_LINES was 0 in each run.
- Acceptance: all four exit codes 0; totals 4, 1, 7, 8 with failed 0; every RESULT line Passed; every targeted name present; every RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - HOLD. No repair iteration was needed.

## Run 1: p2-t9-four (ASSEMBLY-QF, FILTER-FOUR, NAMES-FOUR)

    RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
    VSTEST_EXIT_CODE: 0
    TRX_PRESENT: True
    SEQUENCE_FILES: 0
    COLLECTOR_LINES: 0
    COUNTERS total=4 executed=4 passed=4 failed=0
    RESULT_COUNT: 4
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Passed
    MESSAGE lines: none

## Run 2: p2-t9-openread (ASSEMBLY-UCS, FILTER-OPENREAD, NAMES-FIW)

    RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
    VSTEST_EXIT_CODE: 0
    TRX_PRESENT: True
    SEQUENCE_FILES: 0
    COLLECTOR_LINES: 0
    COUNTERS total=1 executed=1 passed=1 failed=0
    RESULT_COUNT: 1
    RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed
    MESSAGE lines: none

## Run 3: p2-t9-affinity (ASSEMBLY-QF, FILTER-AFFINITY-CLASS, NAMES-AFFINITY)

    RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
    VSTEST_EXIT_CODE: 0
    TRX_PRESENT: True
    SEQUENCE_FILES: 0
    COLLECTOR_LINES: 0
    COUNTERS total=7 executed=7 passed=7 failed=0
    RESULT_COUNT: 7
    RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
    MESSAGE lines: none

## Run 4: p2-t9-fiw (ASSEMBLY-UCS, FILTER-FIW-CLASS, NAMES-FIW)

    RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
    VSTEST_EXIT_CODE: 0
    TRX_PRESENT: True
    SEQUENCE_FILES: 0
    COLLECTOR_LINES: 0
    COUNTERS total=8 executed=8 passed=8 failed=0
    RESULT_COUNT: 8
    RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed
    RESULT Properties_ShouldMirrorWrappedFileInfo = Passed
    RESULT Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException = Passed
    RESULT StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo = Passed
    RESULT ToString_ShouldDelegateToWrappedFileInfo = Passed
    RESULT AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo = Passed
    RESULT PropertyDelegates_ShouldMirrorMockedIFileInfo = Passed
    RESULT ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory = Passed
    MESSAGE lines: none
